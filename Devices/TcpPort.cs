using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices
{
    public class TcpPort : IComPort
    {

        public TcpPort() 
        {
            ComParameter = new();
            TcpParameter = new();
        }

        // Setzt Parameter
        // zB "COM1:9600:8:1:N" oder "localhost:1234" oder "listen:1234"
        public void SetParamString(string paramstring)
        {

        }

        public async Task OpenAsync()
        {
            //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes?source=recommendations
            await Dns.GetHostEntryAsync("host.contoso.com");  //Test
        }
        public async Task CloseAsync()
        {
            await Dns.GetHostEntryAsync("host.contoso.com");  //Test
        }
        public async Task ResetAsync()
        {
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

        public ComParameter ComParameter { get; set; }
        public TcpParameter TcpParameter { get; set; }
        //Runtime:
        public uint Bcc { get; set; }

    }

    public class TcpParameter
    {
        public Remote Remote { get; set; }
        public string? Host { get; set; }
        public string? Port { get; set; }
    }
}
