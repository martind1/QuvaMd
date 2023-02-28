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
    private Device? device;    // from database table 
    public Device Device { get => device ?? throw new ArgumentNullException(); set => device = value; }
    public string Code { get; set; }
    // work items:
    public IComPort? ComPort { get; set; }
    public IScaleApi? ScaleApi { get; set; }


    public ComDevice()
    {
        //Code = devicecode; * kein Parameter wg CS0304
        Code = string.Empty;
    }

    public virtual async Task Open()
    {
        // ComPort öffnen
        Log.Information($"[{Code}] ComDevice.Open:");
        if (ComPort == null) 
            throw new NullReferenceException(nameof(ComPort));
        if (!ComPort.IsConnected())
        {
            await ComPort.OpenAsync();
            Log.Information($"[{Code}] ComDevice.Open OK");
        }
    }

    public virtual async Task Close()
    {
        // Dispose ComPort 
        Log.Information($"[{Code}] ComDevice.Close:");
        if (ComPort != null && ComPort.IsConnected())
        {
            await ComPort.CloseAsync();
            Log.Information($"[{Code}] ComDevice.Close OK");
        }
    }

    public async Task Load()
    {
        Log.Information($"[{Code}] ComDevice.Load");
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
            Log.Warning($"[{Code}] Fehler bei int Device.Option({key})", ex);
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
            Log.Warning($"[{Code}] Fehler bei float Device.Option({key})", ex);
            return dflt;
        }
    }

    public async Task<ScaleData> ScaleCommand(string command)
    {
        Log.Information($"[{Code}] Device.ScaleCommand({command})");
        ArgumentNullException.ThrowIfNull(ScaleApi);
        var result = await ScaleApi.ScaleCommand(command);

        return await Task.FromResult(result);
    }

}
