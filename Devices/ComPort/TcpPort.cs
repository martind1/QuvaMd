using Serilog;
using System.Net;
using System.Net.Sockets;

namespace Quva.Devices.ComPort
{
    public class TcpPort : IComPort, IAsyncDisposable
    {
        private readonly ILogger CLog;
        public string DeviceCode { get; }
        public PortType PortType { get; } = PortType.Tcp;
        public ComParameter ComParameter { get; set; }
        public TcpParameter TcpParameter { get; set; }
        public bool DirectMode { get; } = false;
        //Runtime:
        public uint Bcc { get; set; }
        public bool IsConnected() => tcpClient != null;

        public TcpPort(ComDevice device) : this(device.Code, device.Device.ParamString ?? string.Empty)
        {
        }

        public TcpPort(string deviceCode, string paramString)
        {
            CLog = Log.ForContext<DeviceService>();
            DeviceCode = deviceCode;  //for Debug Output
            ComParameter = new();
            TcpParameter = new();
            SetParamString(paramString);
            inBuff = new(4096);
            outBuff = new(4096);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            CLog.Warning($"[{DeviceCode}] {nameof(TcpPort)}.DisposeAsyncCore({tcpClient != null})");
            //if (tcpClient != null)
            //{
            //    await Task.Run(() => { tcpClient.Dispose(); });
            //}
            //tcpClient = null;
            //if (tcpServer != null)
            //{
            //    await Task.Run(() => { tcpServer.Stop(); });
            //}
            await CloseAsync();
            tcpServer = null;
        }

        public async ValueTask DisposeAsync()
        {
            CLog.Warning($"[{DeviceCode}] {nameof(TcpPort)}.DisposeAsync()");
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

        private TcpClient? tcpClient;
        private TcpListener? tcpServer;
        private IPHostEntry? ipHostEntry;
        private IPAddress? ipAddress;
        private IPEndPoint? ipEndPoint;
        private NetworkStream? stream;
        private readonly ByteBuff inBuff;
        private readonly ByteBuff outBuff;

        public async Task OpenAsync()
        {
            if (IsConnected())
            {
                CLog.Warning($"[{DeviceCode}] TCP({TcpParameter.ParamString}): allready opened");
                return;
            }
            CLog.Information($"[{DeviceCode}] TcpPort.OpenAsync Host:{TcpParameter.Host} Port:{TcpParameter.Port}");
            ipHostEntry = await Dns.GetHostEntryAsync(TcpParameter.Host ?? "localhost");
            // only IPV4:
            ipAddress = ipHostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                ?? ipHostEntry.AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddress, TcpParameter.Port);

            if (TcpParameter.Remote == Remote.Host)
            {
                CLog.Debug($"[{DeviceCode}] Listen {TcpParameter.Port}");
                tcpServer ??= new TcpListener(IPAddress.Any, TcpParameter.Port);  //on all local IPV4 addresses
                tcpServer.Start(1);  //only 1 connection. No problem when already active
                tcpClient = await tcpServer.AcceptTcpClientAsync();  //waiting until connected
                CLog.Debug($"[{DeviceCode}] AcceptAsync EndPoint:{tcpClient.Client.RemoteEndPoint}");
                tcpServer.Stop();  //no more listen
            }
            else
            {
                tcpClient = new TcpClient();
                CLog.Debug($"[{DeviceCode}] ConnectAsync EndPoint:{ipEndPoint} - {ipAddress.AddressFamily}");
                await tcpClient.ConnectAsync(ipEndPoint);
            }


            try
            {
                stream = tcpClient.GetStream();
                if (ComParameter.TimeoutMs == 0)
                    ComParameter.TimeoutMs = 10000;  //overwritable in Desc. Bevore: Timeout.Infinite;
                stream.ReadTimeout = ComParameter.TimeoutMs;
                stream.WriteTimeout = ComParameter.TimeoutMs;
                if (ComParameter.Timeout2Ms == 0)
                    ComParameter.Timeout2Ms = 500;  //overwritable in Desc (T2:500)
            }
            catch
            {
                tcpClient = null;
                throw;
            }
        }

        public async Task CloseAsync()
        {
            if (!IsConnected())
            {
                CLog.Warning($"[{DeviceCode}] TCP({TcpParameter.ParamString}): allready closed");
                return;
            }
            CLog.Debug($"[{DeviceCode}] TCP({TcpParameter.ParamString}): CloseAsync({tcpClient != null})");
            if (tcpClient != null)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            tcpClient.Client.Shutdown(SocketShutdown.Both);
                        }
                        finally
                        {
                            tcpClient.Close();
                        }
                    });
                }
                finally
                {
                    tcpClient = null;
                    stream = null;
                }
            }
            //tcpServer remains (is stopped). see dispose
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
            if (inBuff.Cnt > 0)
            {
                return inBuff.Cnt;
            }
            int waitedMs = 0;
            await FlushAsync();
            ArgumentNullException.ThrowIfNull(tcpClient);
            while (true)
            {
                if (tcpClient.Available > 0)
                {
                    int offset = inBuff.Cnt;
                    int n = await tcpClient.GetStream().ReadAsync(inBuff.Buff.AsMemory(offset, inBuff.Buff.Length - inBuff.Cnt));
                    inBuff.Cnt += n;
                    CLog.Debug($"[{DeviceCode}] [{ipEndPoint}] AVAIL({WaitMs}) \'{inBuff.DebugString(offset)}\'");
                    break;
                }
                if (!ClientIsConnected(tcpClient.Client))
                {
                    await CloseAsync();
                    break;
                }
                if (WaitMs == 0)
                    break;
                if (waitedMs >= WaitMs)
                    break;
                waitedMs += 100;
                CLog.Debug($"[{DeviceCode}] [{ipEndPoint}] AVAIL delay(100/{waitedMs})");
                await Task.Delay(100);
            }
            return await Task.FromResult(inBuff.Cnt);
        }

        // read with timeout. into Buffer offset 0; max buffer.Cnt bytes. Returns 0 when no data available
        public async Task<int> ReadAsync(ByteBuff buffer)
        {
            int result = 0;
            // read from network into internal buffer, with timeout, only if internal buffer not long enough:
            if (inBuff.Cnt < buffer.Cnt)
            {
                int offset = inBuff.Cnt;
                await FlushAsync();
                ArgumentNullException.ThrowIfNull(tcpClient, nameof(tcpClient));
                CLog.Debug($"[{DeviceCode}] [{ipEndPoint}] READ offs:{offset} len:{inBuff.Buff.Length - inBuff.Cnt} timeout:{ComParameter.TimeoutMs}");
                Task<int> readTask = tcpClient.GetStream().ReadAsync(inBuff.Buff, offset, inBuff.Buff.Length - inBuff.Cnt);
                if (ComParameter.TimeoutMs > 0)
                    await Task.WhenAny(readTask, Task.Delay(ComParameter.TimeoutMs));  //<-- timeout
                else
                    await readTask;  //ohne timeout
                if (!readTask.IsCompleted)
                {
                    try
                    {
                        //beware! int n = await readTask;
                        CLog.Warning($"[{DeviceCode}] [{ipEndPoint}] Read Timeout. Close tcpClient:");
                        await CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        // maybe ObjectDisposedException - https://stackoverflow.com/questions/62161695
                        CLog.Warning($"[{DeviceCode}] error closing TcpPort at ReadAsync {TcpParameter.ParamString}", ex);
                    }
                }
                else
                {
                    int n = await readTask;
                    inBuff.Cnt += n;
                    CLog.Debug($"[{DeviceCode}] [{ipEndPoint}] READ \'{inBuff.DebugString(offset)}\'");
                    if (n == 0)
                    {
                        //reading zero bytes - blog.stephencleary.com/2009/06/using-socket-as-connected-socket.html
                        CLog.Warning($"[{DeviceCode}] [{ipEndPoint}] Read 0bytes. Close tcpClient:");
                        await CloseAsync();
                    }
                }
            }
            // move from internal buffer[0..] to buffer[0..]; shift internal buffer
            if (inBuff.Cnt > 0)
            {
                result = inBuff.MoveTo(buffer);  //max buffer.Cnt
            }
            return await Task.FromResult(result);
        }

        /// <summary>
        /// write only to internal buffer. Write to network later in flush.
        /// </summary>
        /// <returns>new Count in internal buffer</returns>
        public async Task<bool> WriteAsync(ByteBuff buffer)
        {
            for (int i = 0; i < buffer.Cnt; i++)
            {
                Bcc ^= buffer.Buff[i];
            }
            buffer.AppendTo(outBuff);

            return await Task.FromResult(true);
        }

        // write only to internal buffer. Write to network later in flush.
        public bool Write(ByteBuff buffer)
        {
            for (int i = 0; i < buffer.Cnt; i++)
            {
                Bcc ^= buffer.Buff[i];
            }
            buffer.AppendTo(outBuff);

            return true;
        }

        //muss vor Read und vor InCount und am Telegram Ende aufgerufen werden
        public async Task FlushAsync()
        {
            if (outBuff.Cnt > 0)
            {
                CLog.Debug($"[{DeviceCode}] [{ipEndPoint}] WRITE \'{outBuff.DebugString()}\'");
                ArgumentNullException.ThrowIfNull(tcpClient, nameof(tcpClient));
                Task writeTask = tcpClient.GetStream().WriteAsync(outBuff.Buff, 0, outBuff.Cnt);
                outBuff.Cnt = 0;
                if (ComParameter.TimeoutMs > 0)
                    await Task.WhenAny(writeTask, Task.Delay(ComParameter.TimeoutMs));  //<-- timeout
                else
                    await writeTask;  //ohne timeout
                if (!writeTask.IsCompleted)
                {
                    try
                    {
                        tcpClient.Close();
                    }
                    catch (Exception ex)
                    {
                        // maybe ObjectDisposedException - https://stackoverflow.com/questions/62161695
                        CLog.Warning($"[{DeviceCode}] error closing TcpPort at WriteAsync {TcpParameter.ParamString}", ex);
                    }
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
}
