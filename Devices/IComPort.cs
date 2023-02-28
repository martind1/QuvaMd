using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public interface IComPort : IAsyncDisposable
{
    // Set Parameter
    // like "COM1:9600:8:1:N" or "localhost:1234" or "listen:1234"
    // void SetParamString(string paramstring);
    // now see constructor 

    Task OpenAsync();
    Task CloseAsync();
    Task ResetAsync();

    Task<int> InCountAsync();
    Task<int> ReadAsync(ByteBuff buffer);  //Buff+Len

    Task<bool> WriteAsync(ByteBuff buffer);
    bool Write(ByteBuff buffer);
    Task FlushAsync();

    string DeviceCode { get; }
    PortType PortType { get; }
    ComParameter ComParameter { get; set; }
    //Runtime:
    uint Bcc { get; set; }
    public bool IsConnected();
}

public class ComParameter
{
    public int TimeoutMs { get; set; }
    public bool DoubleDle { get; set; }
    public bool Echo { get; set; }
}

public enum Remote
{
    Host,
    Client
}

public enum PortType
{
    None,
    Tcp,
    Udp,
    Serial
}
