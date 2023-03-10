using Serilog;

namespace Quva.Devices;

public class DeviceOptions
{
    private readonly ILogger CLog;
    public string DeviceCode { get; set; }
    public Dictionary<string, string>? Options { get; set; }

    public DeviceOptions(string deviceCode, Dictionary<string, string>? options)
    {
        CLog = Log.ForContext<DeviceService>();
        DeviceCode = deviceCode;
        Options = options;
    }

    public string Option(string key, string dflt)
    {
        ArgumentNullException.ThrowIfNull(Options);
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
            CLog.Warning($"[{DeviceCode}] Fehler bei int Device.Option({key})", ex);
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
            CLog.Warning($"[{DeviceCode}] Fehler bei float Device.Option({key})", ex);
            return dflt;
        }
    }


}
