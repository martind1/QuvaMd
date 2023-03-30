using Quva.Services.Services.Shared;
using Serilog;
using System.IO.Ports;
using System.Linq.Expressions;
using System.Net.Sockets;

namespace Quva.Services.Devices.ComPort;

public class ComxPort : IComPort
{
    public SerialParameter SerialParameter { get; set; }
    public string DeviceCode { get; }
    public PortType PortType { get; } = PortType.Serial;
    public ComParameter ComParameter { get; set; }
    public bool DirectMode { get; } = false;
    public uint Bcc { get; set; }

    private readonly ByteBuff _inBuff;
    private readonly ILogger _log;
    private readonly ByteBuff _outBuff;
    private SerialPort _serialPort;
    public ComxPort(ComDevice device) : this(device.Code, device.Device.ParamString ?? string.Empty)
    {
    }

    public ComxPort(string deviceCode, string paramString)
    {
        _log = Log.ForContext<DeviceService>();
        DeviceCode = deviceCode; //for Debug Output
        ComParameter = new ComParameter();
        SerialParameter = new SerialParameter();
        _serialPort = new SerialPort();
        SetParamString(paramString);
        _inBuff = new ByteBuff(4096);
        _outBuff = new ByteBuff(4096);
    }

    public bool IsConnected()
    {
        return _serialPort.IsOpen;
    }

    public async ValueTask DisposeAsync()
    {
        _log.Warning($"[{DeviceCode}] {nameof(ComxPort)}.DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }


    // Setzt Parameter
    // zB "COM1:9600:8:1:N[:H]"
    public void SetParamString(string paramstring)
    {
        var SL = paramstring.Split(":");
        if (SL.Length < 5)
            throw new ArgumentException($"Wrong Paramstring({paramstring}). Must be COMx:Baud:Databits:Stopbits:Parity", nameof(paramstring));
        SerialParameter.ParamString = paramstring;

        SerialParameter.PortName = SL[0].ToUpper();
        SerialParameter.BaudRate = int.Parse(SL[1]);
        SerialParameter.DataBits = int.Parse(SL[2]);
        SerialParameter.StopBits = (StopBits)int.Parse(SL[3]);
        SerialParameter.Parity = SL[4] switch
        {
            "N" => Parity.None,
            "O" => Parity.Odd,
            "E" => Parity.Even,
            "M" => Parity.Mark,
            "S" => Parity.Space,
            _ => throw new ArgumentOutOfRangeException(nameof(Parity), $"Not expected Parity value: {SL[4]}"),
        };
        if (SL.Length > 5)
        {
            SerialParameter.Handshake = SL[5] switch
            {
                "N" => Handshake.None,
                "H" => Handshake.RequestToSend,
                "S" => Handshake.XOnXOff,
                _ => throw new ArgumentOutOfRangeException(nameof(Handshake), $"Not expected Handshake value: {SL[5]}"),
            };
        }
        else
        {
            SerialParameter.Handshake = Handshake.None;
        }
    }

    public async Task OpenAsync()
    {
        if (IsConnected())
        {
            _log.Warning($"[{DeviceCode}] TCP({SerialParameter.ParamString}): allready opened");
            return;
        }
        try
        {
            _log.Information($"[{DeviceCode}] SerialPort.OpenAsync {SerialParameter.ParamString}");
            _serialPort.PortName = SerialParameter.PortName;
            _serialPort.BaudRate = SerialParameter.BaudRate;
            _serialPort.DataBits = SerialParameter.DataBits;
            _serialPort.StopBits = SerialParameter.StopBits;
            _serialPort.Parity = SerialParameter.Parity;
            _serialPort.Handshake = SerialParameter.Handshake;
    
            if (ComParameter.TimeoutMs == 0)
                ComParameter.TimeoutMs = 10000; //overwritable in Desc. Bevore: Timeout.Infinite;
            if (ComParameter.Timeout2Ms == 0)
                ComParameter.Timeout2Ms = 500; //overwritable in Desc (T2:500)

            _serialPort.ReadTimeout = ComParameter.TimeoutMs;
            _serialPort.WriteTimeout = ComParameter.TimeoutMs;
            _serialPort.Open();
        }
        catch
        {
            throw;
        }
    }

    public async Task CloseAsync()
    {
        if (!IsConnected())
        {
            _log.Warning($"[{DeviceCode}] TCP({SerialParameter.ParamString}): allready closed");
            return;
        }

        _log.Debug($"[{DeviceCode}] TCP({SerialParameter.ParamString}): CloseAsync()");
        try
        {
            await Task.Run(() =>
            {
            });
        }
        finally
        {
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _log.Warning($"[{DeviceCode}] {nameof(ComxPort)}.DisposeAsyncCore()");
        await CloseAsync();
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
        return await Task.FromResult(_inBuff.Cnt);
    }

    // read with timeout. into Buffer offset 0; max buffer.Cnt bytes. Returns 0 when no data available
    public async Task<int> ReadAsync(ByteBuff buffer)
    {
        var result = 0;
        int readCount = 0;
        var offset = _inBuff.Cnt;
        _log.Debug($"[{DeviceCode}] READ offs:{offset} len:{_inBuff.Buff.Length - _inBuff.Cnt} timeout:{ComParameter.TimeoutMs}");
        try
        {
        }
        catch (Exception ex)
        {
            _log.Warning($"[{DeviceCode}] Read error {ex.Message}");
            readCount = 0;
        }
        if (readCount == 0)
        {
            //reading zero bytes - blog.stephencleary.com/2009/06/using-socket-as-connected-socket.html
            _log.Warning($"[{DeviceCode}] Read 0bytes. Close _tcpClient:");
            await CloseAsync();
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

    // write only to internal buffer. Write to network later in flush.
    public bool Write(ByteBuff buffer)
    {
        for (var i = 0; i < buffer.Cnt; i++) Bcc ^= buffer.Buff[i];
        buffer.AppendTo(_outBuff);

        return true;
    }

    //muss vor Read und vor InCount und am Telegram Ende aufgerufen werden
    public async Task FlushAsync()
    {
        if (_outBuff.Cnt > 0)
        {
            _log.Debug($"[{DeviceCode}] WRITE \'{_outBuff.DebugString()}\'");
            _outBuff.Cnt = 0;
            try
            {
            }
            catch (Exception ex)
            {
                _log.Warning($"[{DeviceCode}] error closing SerialPort at WriteAsync {SerialParameter.ParamString}", ex);
            }
        }
    }
}
#endregion input/output

public class SerialParameter
{
    public string PortName { get; set; } = string.Empty;
    public int BaudRate { get; set; }
    public int DataBits { get; set; }
    public StopBits StopBits { get; set; }
    public Parity Parity { get; set; }
    public Handshake Handshake { get; set; }

    public string? ParamString { get; set; }
}