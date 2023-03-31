using Quva.Services.Services.Shared;
using Serilog;
using System.IO.Ports;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
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
    private readonly SerialPort _serialPort;
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
    // zB "COM1:9600:E:8:1:N[:H]"
    public void SetParamString(string paramstring)
    {
        var SL = paramstring.Split(":");
        if (SL.Length < 5)
            throw new ArgumentException($"Wrong Paramstring({paramstring}). Must be COMx:Baud:Databits:Stopbits:Parity", nameof(paramstring));
        SerialParameter.ParamString = paramstring;

        SerialParameter.PortName = SL[0].ToUpper();
        SerialParameter.BaudRate = int.Parse(SL[1]);
        SerialParameter.Parity = SL[2] switch
        {
            "N" => Parity.None,
            "O" => Parity.Odd,
            "E" => Parity.Even,
            "M" => Parity.Mark,
            "S" => Parity.Space,
            _ => throw new ArgumentOutOfRangeException(nameof(paramstring), $"Not expected Parity value: {SL[4]}"),
        };
        SerialParameter.DataBits = int.Parse(SL[3]);
        SerialParameter.StopBits = (StopBits)int.Parse(SL[4]);
        if (SL.Length > 5)
        {
            SerialParameter.Handshake = SL[5] switch
            {
                "N" => Handshake.None,
                "H" => Handshake.RequestToSend,
                "S" => Handshake.XOnXOff,
                _ => throw new ArgumentOutOfRangeException(nameof(paramstring),
                                                           $"Not expected Handshake value: {SL[5]}"),
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
            await Task.Run(() =>
            {
                _serialPort.Open();  //since now is BaseStream available
            });
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
            _log.Warning($"[{DeviceCode}] [{_serialPort.PortName}] allready closed");
            return;
        }

        _log.Debug($"[{DeviceCode}] [{_serialPort.PortName}] CloseAsync()");
        await Task.Run(() =>
        {
            _serialPort.Close();  //with serialport.dispose 
        });
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
        while (true)
        {
            if (_serialPort.BytesToRead > 0)
            {
                var offset = _inBuff.Cnt;
                var readCount = await _serialPort.BaseStream
                    .ReadAsync(_inBuff.Buff.AsMemory(offset, _inBuff.Buff.Length - _inBuff.Cnt));
                _inBuff.Cnt += readCount;
                _log.Debug($"[{DeviceCode}] AVAIL({WaitMs}) \'{_inBuff.DebugString(offset)}\'");
                break;
            }
            if (WaitMs == 0)
                break;
            if (waitedMs >= WaitMs)
                break;
            waitedMs += 100;
            _log.Debug($"[{DeviceCode}] AVAIL delay({waitedMs}/{WaitMs})");
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
            _log.Debug(
                $"[{DeviceCode}] [{_serialPort.PortName}] READ offs:{offset} len:{_inBuff.Buff.Length - _inBuff.Cnt} timeout:{ComParameter.TimeoutMs}");
            try
            {
                var readTask = _serialPort.BaseStream.ReadAsync(_inBuff.Buff, offset, _inBuff.Buff.Length - _inBuff.Cnt);
                if (ComParameter.TimeoutMs > 0)
                    await Task.WhenAny(readTask, Task.Delay(ComParameter.TimeoutMs)); //<-- timeout
                else
                    await readTask; //ohne timeout
                if (!readTask.IsCompleted)
                {
                    try
                    {
                        _log.Warning($"[{DeviceCode}] [{_serialPort.PortName}] Read Timeout. Close _tcpClient:");
                        await CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        // maybe ObjectDisposedException - https://stackoverflow.com/questions/62161695
                        _log.Warning($"[{DeviceCode}] error closing SerialPort at ReadAsync {_serialPort.PortName}", ex);
                    }
                }
                else
                {
                    readCount = await readTask;
                    _inBuff.Cnt += readCount;
                    _log.Debug($"[{DeviceCode}] [{_serialPort.PortName}] READ \'{_inBuff.DebugString(offset)}\'");
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"[{DeviceCode}] [{_serialPort.PortName}] Read error {ex.Message}");
                readCount = 0;
            }
            if (readCount == 0)
            {
                _log.Warning($"[{DeviceCode}] [{_serialPort.PortName}] Read 0bytes. Close _tcpClient:");
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
            _log.Debug($"[{DeviceCode}] [{_serialPort.PortName}] WRITE \'{_outBuff.DebugString()}\'");
            var writeTask = _serialPort.BaseStream.WriteAsync(_outBuff.Buff, 0, _outBuff.Cnt);
            _outBuff.Cnt = 0;
            if (ComParameter.TimeoutMs > 0)
                await Task.WhenAny(writeTask, Task.Delay(ComParameter.TimeoutMs)); //<-- timeout
            else
                await writeTask; //ohne timeout
            if (!writeTask.IsCompleted)
                try
                {
                    _serialPort.Close();
                }
                catch (Exception ex)
                {
                    // maybe ObjectDisposedException - https://stackoverflow.com/questions/62161695
                    _log.Warning($"[{DeviceCode}] error closing TcpPort at WriteAsync {_serialPort.PortName}", ex);
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