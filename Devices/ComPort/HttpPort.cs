using Serilog;
using System.Net;
using System.Net.Sockets;

namespace Quva.Devices.ComPort
{
    public class HttpPort : IComPort, IAsyncDisposable
    {
        private readonly ILogger CLog;
        public string DeviceCode { get; }
        public PortType PortType { get; } = PortType.Http;
        public ComParameter ComParameter { get; set; }
        public HttpParameter HttpParameter { get; set; }
        public HttpClient? httpClient { get; set; }
        public bool DirectMode { get; } = true;
        //Runtime:
        public uint Bcc { get; set; }
        public bool IsConnected() => httpClient != null;

        public HttpPort(ComDevice device) : this(device.Code, device.Device.ParamString ?? string.Empty)
        {
        }

        public HttpPort(string deviceCode, string paramString)
        {
            CLog = Log.ForContext<DeviceService>();
            DeviceCode = deviceCode;  //for Debug Output
            ComParameter = new();
            HttpParameter = new();
            SetParamString(paramString);
            httpClient = new HttpClient();  //remains until dispose
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            CLog.Warning($"[{DeviceCode}] {nameof(HttpPort)}.DisposeAsyncCore({httpClient != null})");
            if (httpClient != null)
            {
                await Task.Run(() => { httpClient.Dispose(); });
            }
            httpClient = null;
            //await CloseAsync();
        }

        public async ValueTask DisposeAsync()
        {
            CLog.Warning($"[{DeviceCode}] {nameof(HttpPort)}.DisposeAsync()");
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }


        // Setzt Parameter
        public void SetParamString(string paramstring)
        {
            HttpParameter.URL = paramstring;
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
            ArgumentNullException.ThrowIfNull(HttpParameter.URL, "HttpParameter.URL");
            Uri uri = new(HttpParameter.URL);
            ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
            buffer.Buff = await httpClient.GetByteArrayAsync(uri);
            buffer.Cnt = buffer.Buff.Length;
            CLog.Debug($"HttpPort.ReadAsync {buffer.Cnt} {uri.Host}");
            return await Task.FromResult(buffer.Cnt);
        }

        /// <summary>
        /// write only to internal buffer. Write to network later in flush.
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
}
