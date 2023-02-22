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
    // from database table Device:
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public DeviceRoles Roles { get; set; }
    public string ParamString { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public string ModulCode { get; set; } = string.Empty;
    public TransportType Transport { get; set; }
    public PackagingType Packaging { get; set; }
    public PortType PortType { get; set; }
    public IDictionary<string, string> Options { get; set; }
    // work items:
    public IComPort? ComPort { get; set; }
    public IScaleApi? ScaleApi { get; set; }


    public ComDevice()
    {
        Options = new Dictionary<string, string>();
        //Code = devicecode; * kein Parameter wg CS0304
        Code = string.Empty;
    }

    public virtual async Task Open()
    {
        // ComPort öffnen
        Log.Information($"{Code}.ComDevice.Open:");
        if (ComPort == null) 
            throw new NullReferenceException(nameof(ComPort));
        if (!ComPort.IsConnected())
        {
            await ComPort.OpenAsync();
            Log.Information($"{Code}.ComDevice.Open OK");
        }
            
    }

    public virtual async Task Close()
    {
        // Dispose ComPort 
        Log.Information($"{Code}.ComDevice.Close:");
        if (ComPort != null && ComPort.IsConnected())
        {
            await ComPort.CloseAsync();
            Log.Information($"{Code}.ComDevice.Close OK");
        }
            
    }

    public async Task Load()
    {
        Log.Information($"{Code}.ComDevice.Load");
        // TODO: [Code] von DB laden
        await Task.Delay( 200 );
        Id = 1;
        Name = "Testdevice";
        Roles = DeviceRoles.None;
        PortType = PortType.Tcp;
        ParamString = "111.111.111.111:9999";
        DeviceType = DeviceType.Scale;
        ModulCode = "IT6000";
        Transport = TransportType.Truck;
        Packaging = PackagingType.Bulk;
        Options.Add("GerNr", "1");
        Options.Add("Unit", ScaleUnit.Ton.ToString());

        ComPort = PortType switch
        {
            PortType.Tcp => new TcpPort(ParamString),
            PortType.None => null,
            _ => throw new NotImplementedException("only TCP implemented")
        };
        if (DeviceType == DeviceType.Scale)
        {
            ScaleApi = ModulCode switch
            {
                "IT6000" => new IT60Api(this),
                _ => throw new NotImplementedException($"Modulcode {ModulCode}")
            };
        }
    }

    public string Option(string key, string dflt)
    {
        if (!Options.TryGetValue(key, out var result))
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
            Log.Warning($"Fehler bei int Device({Code}).Option({key})", ex);
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
            Log.Warning($"Fehler bei float Device({Code}).Option({key})", ex);
            return dflt;
        }
    }

    public async Task<ScaleData> ScaleCommand(string command)
    {
        Log.Information($"{Code}.Device.ScaleCommand({command})");
        ArgumentNullException.ThrowIfNull(ScaleApi);
        var result = await ScaleApi.ScaleCommand(command);

        return await Task.FromResult(result);
    }

}
