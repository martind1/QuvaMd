using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public interface IComPort
{
    /// <summary>
    /// Setzt Parameter
    /// </summary>
    /// <param name="paramstring">
    /// zB "COM1:9600:8:1:N" oder "localhost:1234" oder "listen:1234"
    /// </param>
    void SetParamString(string paramstring);

    Task OpenAsync();
    Task CloseAsync();
    Task ResetAsync();

    int InCount();
    void Read(byte[] buffer, int count);

    void Write(byte[] buffer, int count);
    void Flush();

    ComParameter ComParameter { get; set; }
    //Runtime:
    uint Bcc { get; set; }
}

public class ComParameter
{
    public int TimeoutMs { get; set; }
    public bool DoubleDle { get; set; }
}

public enum Remote
{
    Host,
    Client
}
