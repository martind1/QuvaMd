using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement;
using Quva.Services.Devices;
using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Devices.Sps;
using Quva.Services.Interfaces.Shared;
using Serilog;
using static Quva.Services.Devices.ComDevice;
using static Quva.Services.Devices.ComProtocol;

namespace Quva.Services.Services.Shared;

public class DeviceService : IAsyncDisposable, IDeviceService
{
    private readonly ILogger _log;
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _disposeFlag = true;
    private readonly SemaphoreSlim _slim;

    public DeviceService(IServiceScopeFactory scopeFactory)
    {
        DeviceList = new Dictionary<string, ComDevice>();
        _log = Log.ForContext(GetType());
        _log.Information("creating DeviceService");
        _scopeFactory = scopeFactory;
        _slim = new SemaphoreSlim(1);
    }

    private IDictionary<string, ComDevice> DeviceList { get; set; }

    public async ValueTask DisposeAsync()
    {
        _log.Information($"{nameof(DeviceService)}.DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _log.Information($"{nameof(DeviceService)}.DisposeAsyncCore({_disposeFlag})");
        if (_disposeFlag)
            foreach (var d in DeviceList.Values)
                await CloseDevice(d);
        _disposeFlag = false;
    }

    /// <summary>
    /// Get Device from DB per Device Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<DeviceDto?> GetDevice(string code)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuvaContext>();

        var query = (from d in context.Device
             .Include(p => p.DeviceParameter)
                     where d.Code == code
                     select d);
        var value = await query.FirstOrDefaultAsync();
        return await Task.FromResult(value?.Adapt<DeviceDto>());
    }

    public IServiceScopeFactory GetServiceScopeFactory()
    {
        return _scopeFactory;
    }


    #region Card

    public async Task<CardData> CardRead(string devicecode)
    {
        return await CardCommand(devicecode, CardCommands.Read.ToString());
    }

    // bevorzugter Rückgabewert: struct ScaleData
    private async Task<CardData> CardCommand(string devicecode, string command)
    {
        CardData result;
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            result = await device.CardCommand(command);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"CardCommand({devicecode}, {command})");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            result = new CardData(devicecode, command)
            {
                ErrorNr = 99,
                ErrorText = ex.Message
            };
        }

        return await Task.FromResult(result);
    }

    //with callback:

    public async Task<IResult> CardReadStart(string devicecode, OnCardRead onCardRead)
    {
        return await CardCommandStart(devicecode, CardCommands.Read.ToString(), onCardRead);
    }

    private async Task<IResult> CardCommandStart(string devicecode, string command, OnCardRead onCardRead)
    {
        ComDevice? device;
        IResult? result;
        try
        {
            result = Results.Ok();
            device = await AddDevice(devicecode, false);
            device.CardCommandStart(command, onCardRead);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"CardCommandStart({devicecode}, {command})");
            result = Results.NotFound(ex.Message);
            //beware device.Close()
        }

        return await Task.FromResult(result);
    }

    #endregion Card

    #region Sps

    public async Task<SpsData> SpsRead(string devicecode)
    {
        return await SpsCommand(devicecode, SpsCommands.Read.ToString());
    }

    private async Task<SpsData> SpsCommand(string devicecode, string command)
    {
        SpsData result;
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            result = await device.SpsCommand(command);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"SpsCommand({devicecode}, {command})");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            result = new SpsData(devicecode, command)
            {
                ErrorNr = 99,
                ErrorText = ex.Message
            };
        }

        return await Task.FromResult(result);
    }

    //with callback:

    public async Task<IResult> SpsReadStart(string devicecode, OnSpsRead onSpsRead)
    {
        return await SpsCommandStart(devicecode, SpsCommands.Read.ToString(), onSpsRead);
    }

    private async Task<IResult> SpsCommandStart(string devicecode, string command, OnSpsRead onSpsRead)
    {
        ComDevice? device;
        IResult? result;
        try
        {
            result = Results.Ok();
            device = await AddDevice(devicecode, false);
            device.SpsCommandStart(command, onSpsRead);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"SpsCommandStart({devicecode}, {command})");
            result = Results.NotFound(ex.Message);
            //beware device.Close()
        }

        return await Task.FromResult(result);
    }


    #endregion Sps

    #region Simul

    //only with callback:
    public async Task<ComDevice?> SimulCommandStart(string devicecode, SimulDelegate onSimul)
    {
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            device.SimulCommandStart(onSimul);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"SimulCommandStart({devicecode}])");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            device = null;
        }
        return await Task.FromResult(device);
    }

    #endregion Simul

    #region Camera

    public async Task<CamData> CamLoad(string devicecode, int camNumber)
    {
        return await CamCommand(devicecode, CamCommands.Load.ToString(), camNumber);
    }

    // bevorzugter Rückgabewert: struct ScaleData
    private async Task<CamData> CamCommand(string devicecode, string command, int camNumber)
    {
        CamData result;
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            result = await device.CamCommand(command, camNumber);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"CamCommand({devicecode}, {camNumber})");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            result = new CamData(devicecode, command)
            {
                ErrorNr = 99,
                ErrorText = ex.Message
            };
        }

        return await Task.FromResult(result);
    }

    #endregion Camera

    #region Display

    public async Task<DisplayData> DisplayShow(string devicecode, string message)
    {
        return await DisplayCommand(devicecode, DisplayCommands.Show.ToString(), message);
    }

    private async Task<DisplayData> DisplayCommand(string devicecode, string command, string message)
    {
        DisplayData result;
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            result = await device.DisplayCommand(command, message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"DisplayCommand({devicecode}, {command}, {message})");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            result = new DisplayData(devicecode, command)
            {
                ErrorNr = 99,
                ErrorText = ex.Message
            };
        }

        return await Task.FromResult(result);
    }

    //with callback:

    public async Task<IResult> DisplayShowStart(string devicecode, OnDisplayShow onDisplayShow)
    {
        return await DisplayCommandStart(devicecode, DisplayCommands.Show.ToString(), onDisplayShow, null);
    }

    public async Task<IResult> DisplayShowScale(string devicecode, string scaleCode)
    {
        return await DisplayCommandStart(devicecode, DisplayCommands.Show.ToString(), OnShowScale, scaleCode);
    }

    private async Task<IResult> DisplayCommandStart(string devicecode, string command, OnDisplayShow onDisplayShow,
        string? scaleCode)
    {
        ComDevice? device;
        IResult? result;
        try
        {
            result = Results.Ok();
            device = await AddDevice(devicecode, false);
            device.ScaleCode = scaleCode;
            device.DisplayCommandStart(command, onDisplayShow);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"DisplayCommandStart({devicecode}, {command})");
            result = Results.NotFound(ex.Message);
            //beware device.Close()
        }

        return await Task.FromResult(result);
    }

    #endregion display

    #region Scale

    public async Task<ScaleData> ScaleStatus(string devicecode)
    {
        return await ScaleCommand(devicecode, ScaleCommands.Status.ToString());
    }

    public async Task<ScaleData> ScaleRegister(string devicecode)
    {
        return await ScaleCommand(devicecode, ScaleCommands.Register.ToString());
    }

    // bevorzugter Rückgabewert: struct ScaleData
    private async Task<ScaleData> ScaleCommand(string devicecode, string command)
    {
        ScaleData result;
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            result = await device.ScaleCommand(command);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"ScaleCommand({devicecode}, {command})");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            result = new ScaleData(devicecode, command)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
                Display = ex.Message
            };
        }

        return await Task.FromResult(result);
    }

    // with callback:

    public async Task<IResult> ScaleStatusStart(string devicecode, OnScaleStatus onScaleStatus)
    {
        ComDevice? device;
        IResult? result;
        try
        {
            result = Results.Ok();
            device = await AddDevice(devicecode, false);
            device.ScaleCommandStart(ScaleCommands.Status.ToString(), onScaleStatus);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"ScaleStatusStart({devicecode})");
            result = Results.NotFound(ex.Message);
            //beware device.Close()
        }

        return await Task.FromResult(result);
    }

    #endregion Scale

    #region Modbus

    public async Task<ModbusData> ModbusReadStart(string devicecode, OnModbusRead? onModbusRead)
    {
        ComDevice? device;
        ModbusData result;
        try
        {
            device = await AddDevice(devicecode, false);
            result = device.ModbusCommandStart(ModbusCommands.ReadBlocks.ToString(), onModbusRead);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"ModbusStart({devicecode})");
            //beware device.Close()
            result = new ModbusData(devicecode, ModbusCommands.ReadBlocks.ToString(), null)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
            };
        }
        return await Task.FromResult(result);
    }

    public string GetModbusValue(string devicecode, string variableName)
    {
        string? result = null;
        if (DeviceList.TryGetValue(devicecode, out var device))
        {
            result = device.ModbusApi?.Data.GetValue(variableName);
        }
        return result ?? string.Empty;
    }

    public async Task<ModbusData> ModbusWrite(string devicecode, string variableName, string value)
    {
        return await ModbusCommand(devicecode, ModbusCommands.WriteVariable.ToString(), variableName, value);
    }

    private async Task<ModbusData> ModbusCommand(string devicecode, string command, string variableName, string value)
    {
        ModbusData result;
        ComDevice? device = null;
        try
        {
            device = await AddDevice(devicecode, true);
            result = await device.ModbusCommand(command, variableName, value);
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"ModbusCommand({devicecode}, {command})");
            if (device != null)
                await device.Close().ConfigureAwait(false);
            result = new ModbusData(devicecode, command, null)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
            };
        }
        return await Task.FromResult(result);
    }


    #endregion Modbus

    #region Helper functions

    /// <summary>
    ///     Gets the actual Display of a named scale
    /// </summary>
    /// <param name="scaleCode"></param>
    public string GetScaleDisplay(string scaleCode)
    {
        string? result = null;
        if (DeviceList.TryGetValue(scaleCode, out var device))
        {
            result = device.ScaleApi?.StatusData.Display;
        }
        return result ?? string.Empty;
    }

    private void OnShowScale(DisplayData displayData)
    {
        if (DeviceList.TryGetValue(displayData.DeviceCode, out var device))
        {
            ArgumentNullException.ThrowIfNull(device.ScaleCode);
            displayData.Message = GetScaleDisplay(device.ScaleCode);
        }
    }

    #endregion Helper functions


    #region internal calls

    private async Task<ComDevice> AddDevice(string devicecode, bool openFlag)
    {
        ComDevice? device;
        await _slim.WaitAsync();
        try
        {
            if (DeviceList.TryGetValue(devicecode, out device))
            {
                _log.Information($"[{devicecode}] AddDevice: bereits vorhanden");
            }
            else
            {
                _log.Information($"[{devicecode}] AddDevice: add");
                device = new ComDevice(this)
                {
                    Code = devicecode //wichtig weil nicht in Constructor
                };
                DeviceList.Add(devicecode, device);
                await device.Load();
            }
            if (openFlag)
            {
                await device.Open();
            }
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(device);
    }

    private async Task CloseDevice(ComDevice device)
    {
        await CloseDevice(device.Code);
    }

    public async Task CloseDevice(string devicecode)
    {
        if (DeviceList.TryGetValue(devicecode, out var device))
        {
            _log.Information($"CloseDevice({devicecode}): close");
            DeviceList.Remove(devicecode);
            await device.Close();
        }
        else
        {
            _log.Warning($"CloseDevice({devicecode}): not found");
        }
    }

    #endregion internal calls
}