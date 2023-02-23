using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices
{
    public class TcpPort : IComPort, IAsyncDisposable
    {
        public PortType PortType { get; } = PortType.Tcp;
        public ComParameter ComParameter { get; set; }
        public TcpParameter TcpParameter { get; set; }
        //Runtime:
        public uint Bcc { get; set; }
        public bool IsConnected() => client != null;

        public TcpPort(string paramstring)
        {
            ComParameter = new();
            TcpParameter = new();
            SetParamString(paramstring);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            Log.Warning($"{nameof(TcpPort)}.DisposeAsyncCore({client != null})");
            if (client != null)
            {
                await Task.Run(() => { client.Dispose(); });
            }
            client = null;
            if (_Server != null)
            {
                await Task.Run(() => { _Server.Stop(); });
            }
            _Server = null;
        }

        public async ValueTask DisposeAsync()
        {
            Log.Warning($"{nameof(TcpPort)}.DisposeAsync()");
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }


        // Setzt Parameter
        // zB "COM1:9600:8:1:N" oder "localhost:1234" oder "listen:1234"
        private void SetParamString(string paramstring)
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

        private IPHostEntry? ipHostEntry;
        private IPAddress? ipAddress;
        private IPEndPoint? ipEndPoint;
        private NetworkStream? stream;

        public async Task OpenAsync()
        {
            if (IsConnected())
            {
                Log.Warning($"TCP({TcpParameter.ParamString}): allready opened");
                return;
            }
            //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes?source=recommendations
            Log.Information($"TcpPort.OpenAsync Host:{TcpParameter.Host} Port:{TcpParameter.Port}");
            ipHostEntry = await Dns.GetHostEntryAsync(TcpParameter.Host ?? "localhost");
            //ipAddress = ipHostEntry.AddressList[0];
            // only IPV4:
            ipAddress = ipHostEntry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) 
                ?? ipHostEntry.AddressList[0];
            ipEndPoint = new IPEndPoint(ipAddress, TcpParameter.Port);

            if (TcpParameter.Remote == Remote.Host) 
                throw new NotImplementedException($"TCP({TcpParameter.ParamString}): Remote.Host");

            client = new TcpClient();
            try
            {
                Log.Debug($"ConnectAsync EndPoint:{ipEndPoint}");
                await client.ConnectAsync(ipEndPoint);
                stream = client.GetStream();
                if (ComParameter.TimeoutMs == 0)
                    ComParameter.TimeoutMs = 10000;  //overwritable in Desc. Bevore: Timeout.Infinite;
                stream.ReadTimeout = ComParameter.TimeoutMs;
                stream.WriteTimeout = ComParameter.TimeoutMs;

            }
            catch
            {
                client = null;
                throw;
            }
        }

        public async Task CloseAsync()
        {
            if (!IsConnected())
            {
                Log.Warning($"TCP({TcpParameter.ParamString}): allready closed");
                return;
            }

            if (TcpParameter.Remote == Remote.Host)
                throw new NotImplementedException($"TCP({TcpParameter.ParamString}): Remote.Host");

            try
            {
                await Task.Run(() => { client?.Close(); });
            }
            finally
            {
                client = null;
                stream = null;
            }
        }

        public async Task ResetAsync()
        {
            if (IsConnected())
                await CloseAsync();
            await OpenAsync();
        }

        //muss vor Read aufgerufen werden. Ruft Flush auf.
        public int InCount()
        {
            return 0;
        }

        public void Read(byte[] buffer, int count)
        {

        }

        public void Write(byte[] buffer, int count)
        {

        }

        //muss nach Write aufgerufen werden (bzw. per InCount)
        public void Flush()
        {

        }

        #region internal stuff

        private TcpClient? client;
        private TcpListener? _Server;
        

        #endregion
    }

    public class TcpParameter
    {
        public Remote Remote { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }

        public string? ParamString { get; set; }
    }
}
