namespace Quva.Devices.ComPort;

public interface IComPort : IAsyncDisposable
{
    // Set Parameter
    // like "COM1:9600:8:1:N" or "localhost:1234" or "listen:1234"
    // see constructor 
    void SetParamString(string paramstring);
    Task OpenAsync();
    Task CloseAsync();
    Task ResetAsync();

    Task<int> InCountAsync(int WaitMs);
    Task<int> ReadAsync(ByteBuff buffer);  //Buff+Len

    Task<bool> WriteAsync(ByteBuff buffer);
    bool Write(ByteBuff buffer);
    Task FlushAsync();

    string DeviceCode { get; }
    PortType PortType { get; }
    ComParameter ComParameter { get; set; }
    bool DirectMode { get; }
    //Runtime:
    uint Bcc { get; set; }
    bool IsConnected();
}

public class ComParameter
{
    public int TimeoutMs { get; set; }
    public int Timeout2Ms { get; set; }  //between characters in input _stream
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
    Http,
    Udp,
    Serial
}
