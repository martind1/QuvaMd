using Quva.Services.Services.Shared;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace Quva.Services.Devices.ComPort;

public class TcpPort : IComPort
{
    private readonly ByteBuff _inBuff;
    private readonly ILogger _log;
    private readonly ByteBuff _outBuff;
    private IPAddress? _ipAddress;
    private IPEndPoint? _ipEndPoint;
    private IPHostEntry? _ipHostEntry;
    private NetworkStream? _stream;

    private TcpClient? _tcpClient;
    private TcpListener? _tcpServer;

    public TcpPort(ComDevice device) : this(device.Code, device.Device.Paramstring ?? string.Empty)
    {
    }

    public TcpPort(string deviceCode, string paramString)
    {
        _log = Log.ForContext(GetType());
        DeviceCode = deviceCode; //for Debug Output
        ComParameter = new ComParameter();
        TcpParameter = new TcpParameter();
        SetParamString(paramString);
        _inBuff = new ByteBuff(4096);
        _outBuff = new ByteBuff(4096);
    }

    public TcpParameter TcpParameter { get; set; }
    public string DeviceCode { get; }
    public PortType PortType { get; } = PortType.Tcp;
    public ComParameter ComParameter { get; set; }
    public bool DirectMode { get; } = false;
    public uint Bcc { get; set; }

    public bool IsConnected()
    {
        return _tcpClient != null;
    }

    public async ValueTask DisposeAsync()
    {
        _log.Warning($"[{DeviceCode}] {nameof(TcpPort)}.DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }


    // Setzt Parameter
    // zB "COM1:9600:8:1:N" oder "localhost:1234" oder "listen:1234"
    public void SetParamString(string paramstring)
    {
        var SL = paramstring.Split(":");
        if (SL.Length != 2)
            throw new ArgumentException("Wrong Paramstring. Must be Host:Port or Listen:Port", nameof(paramstring));
        TcpParameter.ParamString = paramstring;
        TcpParameter.Remote = Remote.Client;
        TcpParameter.Host = SL[0];
        TcpParameter.Port = int.Parse(SL[1]);
        if (SL[0].Equals("Listen", StringComparison.OrdinalIgnoreCase))
        {
            TcpParameter.Remote = Remote.Host;
            TcpParameter.Host = "localhost";
        }
    }

    // Object als Parameter
    public void SetParameter(object parameter)
    {
        //nothing to do
    }

    public async Task OpenAsync()
    {
        if (IsConnected())
        {
            _log.Warning($"[{DeviceCode}] TCP({TcpParameter.ParamString}): allready opened");
            return;
        }

        try
        {
            _log.Information($"[{DeviceCode}] TcpPort.OpenAsync Host:{TcpParameter.Host} Port:{TcpParameter.Port}");
            _ipHostEntry = await Dns.GetHostEntryAsync(TcpParameter.Host ?? "localhost");
            // only IPV4:
            _ipAddress = _ipHostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                         ?? _ipHostEntry.AddressList[0];
            _ipEndPoint = new IPEndPoint(_ipAddress, TcpParameter.Port);

            if (TcpParameter.Remote == Remote.Host)
            {
                _log.Debug($"[{DeviceCode}] Listen {TcpParameter.Port}");
                _tcpServer ??= new TcpListener(IPAddress.Any, TcpParameter.Port); //on all local IPV4 addresses
                _tcpServer.Start(1); //only 1 connection. No problem when already active
                _tcpClient = await _tcpServer.AcceptTcpClientAsync(); //waiting until connected
                _log.Debug($"[{DeviceCode}] AcceptAsync EndPoint:{_tcpClient.Client.RemoteEndPoint}");
                _tcpServer.Stop(); //no more listen
            }
            else
            {
                _tcpClient = new TcpClient();
                _log.Debug($"[{DeviceCode}] ConnectAsync EndPoint:{_ipEndPoint} - {_ipAddress.AddressFamily}");
                await _tcpClient.ConnectAsync(_ipEndPoint);
            }


            _stream = _tcpClient.GetStream();
            if (ComParameter.TimeoutMs == 0)
                ComParameter.TimeoutMs = 10000; //overwritable in Desc. Bevore: Timeout.Infinite;
            _stream.ReadTimeout = ComParameter.TimeoutMs;
            _stream.WriteTimeout = ComParameter.TimeoutMs;
            if (ComParameter.Timeout2Ms == 0)
                ComParameter.Timeout2Ms = 500; //overwritable in Desc (T2:500)
        }
        catch
        {
            _tcpClient = null;
            throw;
        }
    }

    public async Task CloseAsync()
    {
        if (!IsConnected())
        {
            _log.Warning($"[{DeviceCode}] TCP({TcpParameter.ParamString}): allready closed");
            return;
        }

        _log.Debug($"[{DeviceCode}] TCP({TcpParameter.ParamString}): CloseAsync({_tcpClient != null})");
        if (_tcpClient != null)
        {
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        _tcpClient.Client.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        _tcpClient.Close();
                    }
                });
            }
            finally
            {
                _tcpClient = null;
                _stream = null;
            }
        }
        //_tcpServer remains (is stopped). see dispose
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _log.Warning($"[{DeviceCode}] {nameof(TcpPort)}.DisposeAsyncCore({_tcpClient != null})");
        //if (_tcpClient != null)
        //{
        //    await Task.Run(() => { _tcpClient.Dispose(); });
        //}
        //_tcpClient = null;
        //if (_tcpServer != null)
        //{
        //    await Task.Run(() => { _tcpServer.Stop(); });
        //}
        await CloseAsync();
        _tcpServer = null;
    }

    #region input/output

    public async Task ResetAsync()
    {
        if (IsConnected())
            await CloseAsync();
        await OpenAsync();
    }

    private static bool ClientIsConnected(Socket socket)
    {
        return !socket.Poll(1000, SelectMode.SelectRead) || socket.Available > 0;
    }

    //muss vor Read aufgerufen werden. Ruft Flush auf.
    public async Task<int> InCountAsync(int WaitMs)
    {
        //clearinput: WaitMs=-1: always check tcp
        if (_inBuff.Cnt > 0 && WaitMs >= 0)
        {
            return await Task.FromResult(_inBuff.Cnt);
        }
        var waitedMs = 0;
        await FlushAsync();
        ArgumentNullException.ThrowIfNull(_tcpClient);
        while (true)
        {
            if (_tcpClient.Available > 0)
            {
                var offset = _inBuff.Cnt;
                var n = await _tcpClient.GetStream()
                    .ReadAsync(_inBuff.Buff.AsMemory(offset, _inBuff.Buff.Length - _inBuff.Cnt));
                _inBuff.Cnt += n;
                _log.Debug($"[{DeviceCode}] [{_ipEndPoint}] AVAIL({WaitMs}) \'{_inBuff.DebugString(offset)}\'");
                break;
            }

            if (!ClientIsConnected(_tcpClient.Client))
            {
                await CloseAsync();
                break;
            }

            if (WaitMs == 0)
                break;
            if (waitedMs >= WaitMs)
                break;
            waitedMs += 100;
            _log.Debug($"[{DeviceCode}] [{_ipEndPoint}] AVAIL delay(100/{waitedMs})");
            await Task.Delay(100);
        }
        return await Task.FromResult(_inBuff.Cnt);
    }

    // read with timeout. into Buffer offset 0; max buffer.Cnt bytes. Returns 0 when no data available
    public async Task<int> ReadAsync(ByteBuff buffer)
    {
        var result = 0;
        int readCount = 0;
        // read from network into internal buffer, with timeout, only if internal buffer not long enough:
        if (_inBuff.Cnt < buffer.Cnt)
        {
            var offset = _inBuff.Cnt;
            await FlushAsync();
            ArgumentNullException.ThrowIfNull(_tcpClient, nameof(_tcpClient));
            _log.Debug(
                $"[{DeviceCode}] [{_ipEndPoint}] READ offs:{offset} len:{_inBuff.Buff.Length - _inBuff.Cnt} timeout:{ComParameter.TimeoutMs}");
            try
            {
                var readTask = _tcpClient.GetStream().ReadAsync(_inBuff.Buff, offset, _inBuff.Buff.Length - _inBuff.Cnt);
                if (ComParameter.TimeoutMs > 0)
                    await Task.WhenAny(readTask, Task.Delay(ComParameter.TimeoutMs)); //<-- timeout
                else
                    await readTask; //ohne timeout
                if (!readTask.IsCompleted)
                {
                    try
                    {
                        //beware! int n = await readTask;
                        _log.Warning($"[{DeviceCode}] [{_ipEndPoint}] Read Timeout. Close _tcpClient:");
                        await CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        // maybe ObjectDisposedException - https://stackoverflow.com/questions/62161695
                        _log.Warning($"[{DeviceCode}] error closing TcpPort at ReadAsync {TcpParameter.ParamString}", ex);
                    }
                }
                else
                {
                    readCount = await readTask;
                    _inBuff.Cnt += readCount;
                    _log.Debug($"[{DeviceCode}] [{_ipEndPoint}] READ \'{_inBuff.DebugString(offset)}\'");
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"[{DeviceCode}] [{_ipEndPoint}] Read error {ex.Message}");
                readCount = 0;
            }
            if (readCount == 0)
            {
                //reading zero bytes - blog.stephencleary.com/2009/06/using-socket-as-connected-socket.html
                _log.Warning($"[{DeviceCode}] [{_ipEndPoint}] Read 0bytes. Close _tcpClient:");
                await CloseAsync();
            }
        }

        // move from internal buffer[0..] to buffer[0..]; shift internal buffer; max buffer.Cnt
        if (_inBuff.Cnt > 0)
        {
            result = _inBuff.MoveTo(buffer);
        }
        return await Task.FromResult(result);
    }

    /// <summary>
    ///     write only to internal buffer. Write to network later in flush.
    /// </summary>
    /// <returns>new Count in internal buffer</returns>
    public async Task<bool> WriteAsync(ByteBuff buffer)
    {
        for (var i = 0; i < buffer.Cnt; i++) Bcc ^= buffer.Buff[i];
        buffer.AppendTo(_outBuff);

        return await Task.FromResult(true);
    }

    //muss vor Read und vor InCount und am Telegram Ende aufgerufen werden
    public async Task FlushAsync()
    {
        if (_outBuff.Cnt > 0)
        {
            _log.Debug($"[{DeviceCode}] [{_ipEndPoint}] WRITE \'{_outBuff.DebugString()}\'");
            ArgumentNullException.ThrowIfNull(_tcpClient, nameof(_tcpClient));
            var writeTask = _tcpClient.GetStream().WriteAsync(_outBuff.Buff, 0, _outBuff.Cnt);
            _outBuff.Cnt = 0;
            if (ComParameter.TimeoutMs > 0)
                await Task.WhenAny(writeTask, Task.Delay(ComParameter.TimeoutMs)); //<-- timeout
            else
                await writeTask; //ohne timeout
            if (!writeTask.IsCompleted)
                try
                {
                    _tcpClient.Close();
                }
                catch (Exception ex)
                {
                    // maybe ObjectDisposedException - https://stackoverflow.com/questions/62161695
                    _log.Warning($"[{DeviceCode}] error closing TcpPort at WriteAsync {TcpParameter.ParamString}", ex);
                }
        }
    }

    #endregion input/output
}

public class TcpParameter
{
    public Remote Remote { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }

    public string? ParamString { get; set; }
}