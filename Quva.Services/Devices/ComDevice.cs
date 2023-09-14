using Quva.Model.Dtos.RootManagement;
using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.ComPort;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Devices.Simul;
using Quva.Services.Devices.Sps;
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
    private TimerAsync? _timerAsync = null;
    public DeviceOptions? Options;

    public string Code { get; set; }

    // work items:
    public IComPort? ComPort { get; set; }
    public IScaleApi? ScaleApi { get; set; }
    public ICardApi? CardApi { get; set; }
    public IDisplayApi? DisplayApi { get; set; }
    public ICamApi? CamApi { get; set; }
    public ISpsApi? SpsApi { get; set; }
    public ISimulApi? SimulApi { get; set; }
    public IModbusApi? ModbusApi { get; set; }


    public ComDevice(IDeviceService dataService)
    {
        //Code = devicecode; * kein Parameter wg CS0304
        _log = Log.ForContext(GetType());
        Code = string.Empty;
        _slim = new SemaphoreSlim(1);

        _dataService = dataService;
    }

    public DeviceDto Device
    {
        get => _device ?? throw new ArgumentNullException(nameof(_device));
        set => _device = value;
    }

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
        switch ((DeviceType)Device.DeviceType)
        {
            case DeviceType.Scale:
                ScaleApi = DeviceFactory.GetScaleApi(this);
                break;
            case DeviceType.Card:
                CardApi = DeviceFactory.GetCardApi(this);
                break;
            case DeviceType.Display:
                DisplayApi = DeviceFactory.GetDisplayApi(this);
                break;
            case DeviceType.Cam:
                CamApi = DeviceFactory.GetCamApi(this);
                break;
            case DeviceType.Sps:
                SpsApi = DeviceFactory.GetSpsApi(this, _dataService.GetServiceScopeFactory());
                break;
            case DeviceType.Simul:
                SimulApi = DeviceFactory.GetSimulApi(this);
                break;
            case DeviceType.Modbus:
                ModbusApi = DeviceFactory.GetModbusApi(this);
                break;
            default:
                throw new NotImplementedException($"DeviceType {(DeviceType)Device.DeviceType}");
        }
    }

}