using Quva.Model.Dtos.RootManagement;
using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.ComPort;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Devices.Simul;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Services.Shared;
using Serilog;
using static Quva.Services.Devices.ComProtocol;

namespace Quva.Services.Devices;

public partial class ComDevice
{
    private readonly IDeviceService _dataService;
    private readonly ILogger _log;

    private readonly SemaphoreSlim _slim;
    private DeviceDto? _device; // from database table 
    private TimerAsync? _timerAsync;
    public DeviceOptions? Options;


    public ComDevice(IDeviceService dataService)
    {
        //Code = devicecode; * kein Parameter wg CS0304
        _log = Log.ForContext<DeviceService>();
        Code = string.Empty;
        _slim = new SemaphoreSlim(1);

        _dataService = dataService;
    }

    public DeviceDto Device
    {
        get => _device ?? throw new ArgumentNullException(nameof(_device));
        set => _device = value;
    }

    public string Code { get; set; }

    // work items:
    public IComPort? ComPort { get; set; }
    public IScaleApi? ScaleApi { get; set; }
    public ICardApi? CardApi { get; set; }
    public IDisplayApi? DisplayApi { get; set; }
    public ICamApi? CamApi { get; set; }
    public ISimulApi? SimulApi { get; set; }
    public IModbusApi? ModbusApi { get; set; }

    public virtual async Task Open()
    {
        // ComPort öffnen
        if (ComPort == null) throw new NullReferenceException("ComPort is null");
        if (!ComPort.IsConnected())
        {
            _log.Information($"[{Code}] ComDevice.Open:");
            await ComPort.OpenAsync();
            _log.Information($"[{Code}] ComDevice.Open Ok");
        }
    }

    public virtual async Task Close()
    {
        // Dispose ComPort 
        _log.Information($"[{Code}] ComDevice.Close({ComPort != null},{ComPort?.IsConnected()}):");
        try
        {
            if (_timerAsync != null)
            {
                await _timerAsync.StopAsync();
                _timerAsync.Dispose();
                _timerAsync = null;
            }

            if (ComPort != null && ComPort.IsConnected())
            {
                await ComPort.CloseAsync();
                _log.Information($"[{Code}] ComDevice.Close Ok");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"[{Code}] ComDevice.Close()");
        }
    }

    public async Task Load()
    {
        _log.Information($"[{Code}] ComDevice.Load");

        // [Code] von DB laden
        _device = await _dataService.GetDevice(Code);
        if (_device == null)
        {
            throw new ArgumentException($"Device Code not found ({Code})");
        }

        Options = new DeviceOptions(Code, Device.DeviceParameter);

        ComPort = DeviceFactory.GetComPort(this);
        if ((DeviceType)Device.DeviceType == DeviceType.Scale)
            ScaleApi = DeviceFactory.GetScaleApi(this);
        else if ((DeviceType)Device.DeviceType == DeviceType.Card)
            CardApi = DeviceFactory.GetCardApi(this);
        else if ((DeviceType)Device.DeviceType == DeviceType.Display)
            DisplayApi = DeviceFactory.GetDisplayApi(this);
        else if ((DeviceType)Device.DeviceType == DeviceType.Cam)
            CamApi = DeviceFactory.GetCamApi(this);
        else if ((DeviceType)Device.DeviceType == DeviceType.Simul)
            SimulApi = DeviceFactory.GetSimulApi(this);
        else if ((DeviceType)Device.DeviceType == DeviceType.Modbus)
            ModbusApi = DeviceFactory.GetModbusApi(this);
        else
            throw new NotImplementedException($"DeviceType {(DeviceType)Device.DeviceType}");
    }

}