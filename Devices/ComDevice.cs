using Microsoft.Extensions.DependencyInjection;
using Quva.Devices.Cam;
using Quva.Devices.Card;
using Quva.Devices.ComPort;
using Quva.Devices.Data;
using Quva.Devices.Display;
using Quva.Devices.Scale;
using Serilog;

namespace Quva.Devices;

public class ComDevice
{
    private readonly ILogger _log;
    private Device? _device;    // from database table 
    public Device Device { get => _device ?? throw new ArgumentNullException(nameof(_device)); set => _device = value; }
    public DeviceOptions? Options;
    public string Code { get; set; }
    // work items:
    public IComPort? ComPort { get; set; }
    public IScaleApi? ScaleApi { get; set; }
    public ICardApi? CardApi { get; set; }
    public IDisplayApi? DisplayApi { get; set; }
    public ICamApi? CamApi { get; set; }

    private readonly IDataService _dataService;


    public ComDevice()
    {
        //Code = devicecode; * kein Parameter wg CS0304
        _log = Log.ForContext<DeviceService>();
        Code = string.Empty;
        _slim = new SemaphoreSlim(1);

        ArgumentNullException.ThrowIfNull(Program.host, nameof(Program.host));
        using IServiceScope serviceScope = Program.host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _dataService = provider.GetRequiredService<IDataService>();
    }

    public virtual async Task Open()
    {
        // ComPort öffnen
        if (ComPort == null)
        {
            throw new NullReferenceException("ComPort is null");
        }
        if (!ComPort.IsConnected())
        {
            _log.Information($"[{Code}] ComDevice.Open:");
            await ComPort.OpenAsync();
            _log.Information($"[{Code}] ComDevice.Open OK");
        }
    }

    public virtual async Task Close()
    {
        // Dispose ComPort 
        _log.Information($"[{Code}] ComDevice.Close({ComPort != null},{ComPort?.IsConnected()}):");
        if (_timerAsync != null)
        {
            await _timerAsync.StopAsync();
            _timerAsync.Dispose();
            _timerAsync = null;
        }
        if (ComPort != null && ComPort.IsConnected())
        {
            await ComPort.CloseAsync();
            _log.Information($"[{Code}] ComDevice.Close OK");
        }
    }

    public async Task Load()
    {
        _log.Information($"[{Code}] ComDevice.Load");

        // [Code] von DB laden - erstmal von Test Service laden:
        //var _dataService = new DataService();
        _device = await _dataService.GetDevice(Code);

        Options = new DeviceOptions(Code, _device.Options);

        ComPort = DeviceFactory.GetComPort(this);
        if (_device.DeviceType == DeviceType.Scale)
        {
            ScaleApi = DeviceFactory.GetScaleApi(this);
        }
        else if (_device.DeviceType == DeviceType.Card)
        {
            CardApi = DeviceFactory.GetCardApi(this);
        }
        else if (_device.DeviceType == DeviceType.Display)
        {
            DisplayApi = DeviceFactory.GetDisplayApi(this);
        }
        else if (_device.DeviceType == DeviceType.Cam)
        {
            CamApi = DeviceFactory.GetCamApi(this);
        }
        else
        {
            throw new NotImplementedException($"DeviceType {_device.DeviceType}");
        }
    }

    private readonly SemaphoreSlim _slim;
    private TimerAsync? _timerAsync;
    private string _timerCommand = string.Empty;

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
        _log.Debug($"[{Code}] CALLBACK Device.ScaleCommandStart({command})");
        ArgumentNullException.ThrowIfNull(ScaleApi);
        _onScaleStatus = onScaleStatus;
        _timerCommand = command;
        _timerAsync = new TimerAsync(OnScaleCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));
    }

    private async Task OnScaleCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested)
        {
            return;
        }
        ScaleData result;
        _log.Debug($"[{Code}] OnScaleCommand({_timerCommand})");
        try
        {
            await Open();
            result = await ScaleCommand(_timerCommand);
        }
        catch (Exception ex)
        {
            _log.Warning($"[{Code}] Fehler OnScaleCommand({ex.Message})");
            await Close().ConfigureAwait(false);
            result = new ScaleData(Code, _timerCommand)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
            };
        }
        ArgumentNullException.ThrowIfNull(_onScaleStatus);
        _onScaleStatus(result);
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
        _log.Debug($"[{Code}] CALLBACK Device.CardCommandStart({command})");
        ArgumentNullException.ThrowIfNull(CardApi);
        _onCardRead = onCardRead;
        _timerCommand = command;
        _timerAsync = new TimerAsync(OnCardCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));
    }

    private async Task OnCardCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested)
        {
            return;
        }
        CardData result;
        _log.Debug($"[{Code}] OnCardCommand({_timerCommand})");
        try
        {
            await Open();
            result = await CardCommand(_timerCommand);
        }
        catch (Exception ex)
        {
            _log.Warning($"[{Code}] Fehler OnCardCommand({ex.Message})");
            await Close().ConfigureAwait(false);
            result = new CardData(Code, _timerCommand)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
            };
        }
        ArgumentNullException.ThrowIfNull(_onCardRead);
        _onCardRead(result);
    }

    #endregion Card Commands

    #region Display Commands

    public delegate void OnDisplayShow(DisplayData displayData);
    public OnDisplayShow? onDisplayShow { get; set; }  //timer function
    public string? ScaleCode;  //show scale display
    private string? _oldMessage;

    public async Task<DisplayData> DisplayCommand(string command, string message)
    {
        DisplayData result;
        string messageDebug = message.Trim().Length > 8 ? message.Trim()[..8] + ".." : message.Trim();
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
        _log.Debug($"[{Code}] CALLBACK Device.DisplayCommandStart({command})");
        ArgumentNullException.ThrowIfNull(DisplayApi);
        this.onDisplayShow = onDisplayShow;
        _timerCommand = command;
        _timerAsync = new TimerAsync(OnDisplayCommand, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(500));
    }

    private async Task OnDisplayCommand(CancellationToken arg)
    {
        if (arg.IsCancellationRequested)
        {
            return;
        }
        ArgumentNullException.ThrowIfNull(DisplayApi);
        var displayData = DisplayApi.ShowData; // new DisplayData(Code, command: DisplayCommands.Show.ToString());
        _log.Debug($"[{Code}] OnDisplayCommand({_timerCommand})");
        try
        {
            await Open();
            ArgumentNullException.ThrowIfNull(onDisplayShow);
            onDisplayShow(displayData);  //fills .Message
            if (_oldMessage != displayData.Message)
            {
                _oldMessage = displayData.Message;
                _ = await DisplayCommand(_timerCommand, displayData.Message);
            }
        }
        catch (Exception ex)
        {
            _log.Warning($"[{Code}] Fehler OnDisplayCommand({ex.Message})");
            await Close().ConfigureAwait(false);
            _ = new DisplayData(Code, _timerCommand)
            {
                ErrorNr = 99,
                ErrorText = ex.Message,
            };
        }
    }

    #endregion Display Commands

    #region Camera Commands

    public delegate void OnCamShow(CamData displayData);

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
}
