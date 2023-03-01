using Devices.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quva.Devices;

public class ComDevice
{
    private readonly ILogger CLog;
    private Device? device;    // from database table 
    public Device Device { get => device ?? throw new ArgumentNullException(); set => device = value; }
    public string Code { get; set; }
    // work items:
    public IComPort? ComPort { get; set; }
    public IScaleApi? ScaleApi { get; set; }


    public ComDevice()
    {
        //Code = devicecode; * kein Parameter wg CS0304
        CLog = Log.ForContext<DeviceService>();
        Code = string.Empty;
        slim = new SemaphoreSlim(1);
    }

    public virtual async Task Open()
    {
        // ComPort öffnen
        CLog.Information($"[{Code}] ComDevice.Open:");
        if (ComPort == null) 
            throw new NullReferenceException(nameof(ComPort));
        if (!ComPort.IsConnected())
        {
            await ComPort.OpenAsync();
            CLog.Information($"[{Code}] ComDevice.Open OK");
        }
    }

    public virtual async Task Close()
    {
        // Dispose ComPort 
        CLog.Information($"[{Code}] ComDevice.Close:");
        if (ComPort != null && ComPort.IsConnected())
        {
            await ComPort.CloseAsync();
            CLog.Information($"[{Code}] ComDevice.Close OK");
        }
    }

    public async Task Load()
    {
        CLog.Information($"[{Code}] ComDevice.Load");
        // [Code] von DB laden - erstmal von Test Service laden:
        var dataService = new DataService();
        device = await dataService.GetDevice(Code);

        ArgumentNullException.ThrowIfNull(Device.ParamString);
        ComPort = device.PortType switch
        {
            PortType.Tcp => new TcpPort(Code, Device.ParamString),
            PortType.None => null,
            _ => throw new NotImplementedException("only TCP implemented")
        };
        if (device.DeviceType == DeviceType.Scale)
        {
            ScaleApi = Device.ModulCode switch
            {
                "IT6000" => new IT6000Api(Code, this),
                "FAWAWS" => new FawaWsApi(Code, this),
                _ => throw new NotImplementedException($"Modulcode {Device.ModulCode}")
            };
        }
    }

    public string Option(string key, string dflt)
    {
        ArgumentNullException.ThrowIfNull(Device.Options);
        if (!Device.Options.TryGetValue(key, out var result))
        {
            result = dflt;
        }
        return result;
    }

    public int Option(string key, int dflt)
    {
        try
        {
            return int.Parse(Option(key, dflt.ToString()));
        }
        catch (Exception ex)
        {
            CLog.Warning($"[{Code}] Fehler bei int Device.Option({key})", ex);
            return dflt;
        }
    }

    public float Option(string key, float dflt)
    {
        try
        {
            return int.Parse(Option(key, dflt.ToString()));
        }
        catch (Exception ex)
        {
            CLog.Warning($"[{Code}] Fehler bei float Device.Option({key})", ex);
            return dflt;
        }
    }

    private readonly SemaphoreSlim slim;

    public async Task<ScaleData> ScaleCommand(string command)
    {
        ScaleData result;
        CLog.Debug($"[{Code}] WAIT Device.ScaleCommand({command})");
        // das ComDevice darf nur ein Command gleichzeitig ausführen (sonst Protokoll/TCP Murks)
        await slim.WaitAsync();
        try
        {
            CLog.Information($"[{Code}] START Device.ScaleCommand({command})");
            ArgumentNullException.ThrowIfNull(ScaleApi);
            result = await ScaleApi.ScaleCommand(command);
            CLog.Debug($"[{Code}] END Device.ScaleCommand({command})");
        }
        finally
        {
            slim.Release();
        }
        return await Task.FromResult(result);
    }

}
