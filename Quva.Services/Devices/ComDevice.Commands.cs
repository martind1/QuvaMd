using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using static Quva.Services.Devices.ComProtocol;

namespace Quva.Services.Devices;

public partial class ComDevice
{
    private string _timerCommand = string.Empty;

    #region Camera Commands

    public async Task<CamData> CamCommand(string command, int camNumber)
    {
        CamData result;
        _log.Debug($"[{Code}] WAIT Device.CamCommand({command},{camNumber})");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await _slim.WaitAsync();
        try
        {
            try
            {
                _log.Information($"[{Code}] START Device.CamCommand({command},{camNumber})");
                ArgumentNullException.ThrowIfNull(CamApi);
                result = await CamApi.CamCommand(command, camNumber);
                _log.Debug($"[{Code}] END Device.CamCommand({command},{camNumber})");
            }
            catch
            {
                //stacktrace see DeviceService call
                _log.Debug($"[{Code}] EXCEPTION Device.CamCommand({command},{camNumber})");
                throw;
            }
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(result);
    }

    #endregion Camera Commands

    #region Scale Commands

    public delegate void OnScaleStatus(ScaleData scaleData);

    private OnScaleStatus? _onScaleStatus { get; set; }

    public async Task<ScaleData> ScaleCommand(string command)
    {
        ScaleData result;
        _log.Debug($"[{Code}] WAIT Device.ScaleCommand({command})");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await _slim.WaitAsync();
        try
        {
            _log.Information($"[{Code}] START Device.ScaleCommand({command})");
            ArgumentNullException.ThrowIfNull(ScaleApi);
            //external device for Positioning
            if (ScaleApi.PositionCode != string.Empty)
            {
                //MODBUS_CODE=HOH.WAGO
                if (!ScaleApi.PositionInit)
                {
                    ScaleApi.PositionInit = true;
                    ScaleApi.PositionData = await _dataService.ModbusReadStart(ScaleApi.PositionCode, null);
                }

            }

            result = await ScaleApi.ScaleCommand(command);
            _log.Debug($"[{Code}] END Device.ScaleCommand({command})");
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(result);
    }

    public void ScaleCommandStart(string command, OnScaleStatus onScaleStatus)
    {
        if (_timerAsync != null)
            throw new Exception("already started");
        _log.Debug($"[{Code}] CALLBACK Device.ScaleCommandStart({command})");
        ArgumentNullException.ThrowIfNull(ScaleApi);
        _onScaleStatus = onScaleStatus;
        _timerCommand = command;
        _timerAsync = new TimerAsync(OnScaleCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));
    }

    private async Task OnScaleCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested) return;
        ScaleData result;
        _log.Debug($"[{Code}] OnScaleCommand({_timerCommand})");
        string debugStr = string.Empty;
        try
        {
            debugStr = "Open";
            await Open();
            try
            {
                debugStr = "Command";
                result = await ScaleCommand(_timerCommand);
            }
            catch
            {
                await Close(); //.ConfigureAwait(false);
                throw;
            }
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"[{Code}] Error OnScaleCommand() {debugStr}");
            result = new ScaleData(Code, _timerCommand)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
                Display = ex.Message,
            };
        }
        _onScaleStatus?.Invoke(result);
    }

    #endregion Scale Commands

    #region Card Commands

    public delegate void OnCardRead(CardData cardData);

    private OnCardRead? _onCardRead { get; set; }

    public async Task<CardData> CardCommand(string command)
    {
        CardData result;
        _log.Debug($"[{Code}] WAIT Device.CardCommand({command})");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await _slim.WaitAsync();
        try
        {
            _log.Information($"[{Code}] START Device.CardCommand({command})");
            ArgumentNullException.ThrowIfNull(CardApi);
            result = await CardApi.CardCommand(command);
            _log.Debug($"[{Code}] END Device.CardCommand({command})");
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(result);
    }

    public void CardCommandStart(string command, OnCardRead onCardRead)
    {
        if (_timerAsync != null)
            throw new Exception("already started");
        _log.Debug($"[{Code}] CALLBACK Device.CardCommandStart({command})");
        ArgumentNullException.ThrowIfNull(CardApi);
        _onCardRead = onCardRead;
        _timerCommand = command;
        _timerAsync = new TimerAsync(OnCardCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));
    }

    private async Task OnCardCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested) return;
        CardData result;
        _log.Debug($"[{Code}] OnCardCommand({_timerCommand})");
        try
        {
            await Open();
            result = await CardCommand(_timerCommand);
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"[{Code}] Error OnCardCommand()");
            await Close().ConfigureAwait(false);
            result = new CardData(Code, _timerCommand)
            {
                ErrorNr = 99,
                ErrorText = ex.Message
            };
        }
        _onCardRead?.Invoke(result);
    }

    #endregion Card Commands

    #region Display Commands

    public delegate void OnDisplayShow(DisplayData displayData);

    private OnDisplayShow? _onDisplayShow; //timer function
    public string? ScaleCode; //show scale display
    private string? _oldMessage;

    public async Task<DisplayData> DisplayCommand(string command, string message)
    {
        DisplayData result;
        var messageDebug = message.Trim().Length > 8 ? message.Trim()[..8] + ".." : message.Trim();
        _log.Debug($"[{Code}] WAIT Device.DisplayCommand({command}, {messageDebug})");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await _slim.WaitAsync();
        try
        {
            _log.Information($"[{Code}] START Device.DisplayCommand({command}, {messageDebug})");
            ArgumentNullException.ThrowIfNull(DisplayApi);
            result = await DisplayApi.DisplayCommand(command, message);
            _log.Debug($"[{Code}] END Device.DisplayCommand({command}, {messageDebug})");
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(result);
    }

    public void DisplayCommandStart(string command, OnDisplayShow onDisplayShow)
    {
        if (_timerAsync != null)
            throw new Exception("already started"); 
        _log.Debug($"[{Code}] CALLBACK Device.DisplayCommandStart({command})");
        ArgumentNullException.ThrowIfNull(DisplayApi);
        _onDisplayShow = onDisplayShow;
        _timerCommand = command;
        _timerAsync = new TimerAsync(OnDisplayCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
    }

    private async Task OnDisplayCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested) return;
        ArgumentNullException.ThrowIfNull(DisplayApi);
        var displayData = DisplayApi.ShowData; // new DisplayData(Code, command: DisplayCommands.Show.ToString());
        _log.Debug($"[{Code}] OnDisplayCommand({_timerCommand})");
        try
        {
            await Open();
            ArgumentNullException.ThrowIfNull(_onDisplayShow);
            _onDisplayShow(displayData); //fills .Message
            if (_oldMessage != displayData.Message)
            {
                _oldMessage = displayData.Message;
                _ = await DisplayCommand(_timerCommand, displayData.Message);
            }
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"[{Code}] Error OnDisplayCommand()");
            await Close().ConfigureAwait(false);
            _ = new DisplayData(Code, _timerCommand)
            {
                ErrorNr = 99,
                ErrorText = ex.Message
            };
        }
    }

    #endregion Display Commands

    #region Modbus Commands

    public delegate void OnModbusRead(ModbusData scaleData);

    private OnModbusRead? _onModbusRead { get; set; }


    public async Task<ModbusData> ModbusCommand(string command, string variableName, string value)
    {
        ModbusData result;
        _log.Debug($"[{Code}] WAIT Device.ModbusCommand({command}, {variableName})");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await _slim.WaitAsync();
        try
        {
            _log.Information($"[{Code}] START Device.ModbusCommand({command}, {variableName})");
            ArgumentNullException.ThrowIfNull(ModbusApi);
            result = await ModbusApi.ModbusCommand(command, variableName, value);
            _log.Debug($"[{Code}] END Device.ModbusCommand({command}, {variableName})");
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(result);
    }

    public ModbusData ModbusCommandStart(string command, OnModbusRead? onModbusRead)
    {
        if (_timerAsync != null)
        {
            _log.Debug($"[{Code}] ALREADY STARTED Device.ModbusCommandStart({command})");
            ArgumentNullException.ThrowIfNull(ModbusApi);
        }
        else
        {
            _log.Debug($"[{Code}] CALLBACK Device.ModbusCommandStart({command})");
            ArgumentNullException.ThrowIfNull(ModbusApi);
            _onModbusRead = onModbusRead;
            _timerCommand = command;
            _timerAsync = new TimerAsync(OnModbusCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
        }
        return ModbusApi.Data;  //for scale positioning
    }

    private async Task OnModbusCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested) return;
        ArgumentNullException.ThrowIfNull(ModbusApi);
        _log.Debug($"[{Code}] OnModbusCommand({_timerCommand})");
        ModbusData result;
        string debugStr = string.Empty;
        try
        {
            debugStr = "Open";
            await Open();
            try
            {
                debugStr = "Command";
                result = await ModbusCommand(_timerCommand, "", "");  //no variable at ReadBlocks
            }
            catch
            {
                await Close(); //.ConfigureAwait(false);
                throw;
            }
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"[{Code}] Error OnModbusCommand()");
            result = new ModbusData(Code, _timerCommand, null)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
            };
        }
        _onModbusRead?.Invoke(result);
    }

    #endregion Modbus Commands

    #region Simul Commands

    public async Task<DeviceData> SimulCommand()
    {
        DeviceData result;
        _log.Debug($"[{Code}] SimulCommand()");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await _slim.WaitAsync();
        try
        {
            _log.Information($"[{Code}] SimulCommand()");
            ArgumentNullException.ThrowIfNull(SimulApi);
            result = await SimulApi.SimulCommand();
            _log.Debug($"[{Code}] END Device.SimulCommand()");
        }
        finally
        {
            _slim.Release();
        }
        return await Task.FromResult(result);
    }

    public void SimulCommandStart(SimulDelegate onSimul)
    {
        if (_timerAsync != null)
            throw new Exception("already started");
        _log.Debug($"[{Code}] CALLBACK Device.SimulCommandStart()");
        if (SimulApi is ComProtocol comProtocol)
        {
            comProtocol.OnSimul = onSimul;
        }
        else
        {
            throw new Exception("SimulApi missing or not of type ComProtocol");
        }
        _timerAsync = new TimerAsync(OnSimulCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));
    }

    private async Task OnSimulCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested) return;
        _log.Debug($"[{Code}] OnSimulCommand()");
        try
        {
            await Open();
            await SimulCommand();
        }
        catch (Exception ex)
        {
            _log.Warning(ex, $"[{Code}] Error OnSimulCommand()");
            await Close().ConfigureAwait(false);
        }
    }


    #endregion Simul Commands

}
