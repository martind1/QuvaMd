using Serilog;

namespace Quva.Devices;

public class DeviceOptions
{
    private readonly ILogger CLog;

    public DeviceOptions(string deviceCode, Dictionary<string, string>? options)
    {
        CLog = Log.ForContext<DeviceService>();
        DeviceCode = deviceCode;
        //Options = options;
        //ignore case in key:
        var comparer = StringComparer.OrdinalIgnoreCase;
        ArgumentNullException.ThrowIfNull(options, "DeviceOptions(options)");
        Options = new Dictionary<string, string>(options, comparer);
    }

    public string DeviceCode { get; set; }
    public Dictionary<string, string>? Options { get; set; }

    /// <summary>
    ///     14.03.23 dflt can be null
    ///     15.03.23 ignore case in key
    ///     21.03.23 dflt can not be null
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dflt"></param>
    /// <returns></returns>
    public string Option(string key, string dflt)
    {
        ArgumentNullException.ThrowIfNull(Options);
        if (!Options.TryGetValue(key, out var result)) 
            result = dflt;
        return result;
    }

    public int Option(string key, int dflt)
    {
        try
        {
            var s = Option(key, dflt.ToString());
            return int.Parse(s);
        }
        catch (Exception ex)
        {
            CLog.Warning(ex, $"[{DeviceCode}] Fehler bei int Device.Option({key})");
            return dflt;
        }
    }

    public bool Option(string key, bool dflt)
    {
        return Option(key, dflt ? 1 : 0) != 0;
    }

    public double Option(string key, double dflt)
    {
        try
        {
            var s = Option(key, dflt.ToString());
            return double.Parse(s);
        }
        catch (Exception ex)
        {
            CLog.Warning(ex, $"[{DeviceCode}] Fehler bei double Device.Option({key})");
            return dflt;
        }
    }
}