using Serilog;

namespace Devices.ComPort;

public class HttpPort : IComPort
{
    private readonly HttpParameter _httpParameter;
    private readonly ILogger _log;
    private HttpClient? _httpClient;

    public HttpPort(ComDevice device) : this(device.Code, device.Device.ParamString ?? string.Empty)
    {
    }

    public HttpPort(string deviceCode, string paramString)
    {
        _log = Log.ForContext<DeviceService>();
        DeviceCode = deviceCode; //for Debug Output
        ComParameter = new ComParameter();
        _httpParameter = new HttpParameter();
        SetParamString(paramString);
        _httpClient = new HttpClient(); //remains until dispose
    }

    public string DeviceCode { get; }
    public PortType PortType { get; } = PortType.Http;
    public ComParameter ComParameter { get; set; }
    public bool DirectMode { get; } = true;
    public uint Bcc { get; set; }

    public bool IsConnected()
    {
        return _httpClient != null;
    }

    public async ValueTask DisposeAsync()
    {
        _log.Warning($"[{DeviceCode}] {nameof(HttpPort)}.DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }


    // Setzt Parameter
    public void SetParamString(string paramstring)
    {
        _httpParameter.URL = paramstring;
    }


    public async Task OpenAsync()
    {
        //nothing todo here
        await Task.Delay(0);
    }

    public async Task CloseAsync()
    {
        //nothing todo here
        await Task.Delay(0);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _log.Warning($"[{DeviceCode}] {nameof(HttpPort)}.DisposeAsyncCore({_httpClient != null})");
        if (_httpClient != null) await Task.Run(() => { _httpClient.Dispose(); });
        _httpClient = null;
        //await CloseAsync();
    }

    #region input/output

    public async Task ResetAsync()
    {
        //nothing todo here
        await Task.Delay(0);
    }

    public async Task<int> InCountAsync(int WaitMs)
    {
        //0 important for ComProtocol.ReadDataAsync!
        return await Task.FromResult(0);
    }

    public async Task<int> ReadAsync(ByteBuff buffer)
    {
        //load image
        ArgumentNullException.ThrowIfNull(_httpParameter.URL);
        Uri uri = new(_httpParameter.URL);
        ArgumentNullException.ThrowIfNull(_httpClient, nameof(_httpClient));
        buffer.Buff = await _httpClient.GetByteArrayAsync(uri);
        buffer.Cnt = buffer.Buff.Length;
        _log.Debug($"[{DeviceCode}] HttpPort.ReadAsync {buffer.Cnt} {uri.Host}");
        return await Task.FromResult(buffer.Cnt);
    }

    /// <summary>
    ///     write only to internal buffer. Write to network later in flush.
    /// </summary>
    /// <returns>new Count in internal buffer</returns>
    public async Task<bool> WriteAsync(ByteBuff buffer)
    {
        //nothing to write
        return await Task.FromResult(true);
    }

    // write only to internal buffer. Write to network later in flush.
    public bool Write(ByteBuff buffer)
    {
        //nothing to write
        return true;
    }

    //muss vor Read und vor InCount und am Telegram Ende aufgerufen werden
    public async Task FlushAsync()
    {
        //nothing to flush
        await Task.Delay(0);
    }

    #endregion input/output
}

public class HttpParameter
{
    public string? URL { get; set; }
}