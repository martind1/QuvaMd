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
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dflt"></param>
    /// <returns></returns>
    public string? Option(string key, string? dflt)
    {
        ArgumentNullException.ThrowIfNull(Options);
        if (!Options.TryGetValue(key, out var result)) result = dflt;
        return result;
    }

    public int? Option(string key, int? dflt)
    {
        try
        {
            var s = Option(key, dflt?.ToString());
            return s != null ? int.Parse(s) : null;
        }
        catch (Exception ex)
        {
            CLog.Warning($"[{DeviceCode}] Fehler bei int Device.Option({key})", ex);
            return dflt;
        }
    }

    public float? Option(string key, float? dflt)
    {
        try
        {
            var s = Option(key, dflt?.ToString());
            return s != null ? float.Parse(s) : null;
        }
        catch (Exception ex)
        {
            CLog.Warning($"[{DeviceCode}] Fehler bei float Device.Option({key})", ex);
            return dflt;
        }
    }
}