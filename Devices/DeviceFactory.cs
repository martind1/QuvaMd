using Quva.Devices.Cam;
using Quva.Devices.Card;
using Quva.Devices.ComPort;
using Quva.Devices.Display;
using Quva.Devices.Scale;

namespace Quva.Devices;

public class DeviceFactory
{
    public static IComPort? GetComPort(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.PortType);
        PortType portType = device.Device.PortType;
        IComPort? comPort = portType switch
        {
            PortType.Tcp => new TcpPort(device),
            PortType.Http => new HttpPort(device),
            PortType.Serial => throw new NotImplementedException("Serial not implemented"),
            PortType.Udp => throw new NotImplementedException("UDP not implemented"),
            PortType.None => null,
            _ => throw new NotImplementedException("only TCP implemented")
        };
        return comPort;
    }

    public static IScaleApi GetScaleApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        IScaleApi scaleApi = modulCode.ToUpper() switch
        {
            "IT6000" => new IT6000(device),
            "FAWAWS" => new FawaWs(device),
            _ => throw new NotImplementedException($"Modulcode.Scale {modulCode}")
        };
        return scaleApi;
    }

    public static ICardApi GetCardApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        ICardApi cardApi = modulCode.ToUpper() switch
        {
            "READER" => new Reader(device),
            _ => throw new NotImplementedException($"Modulcode.Card {modulCode}")
        };
        return cardApi;
    }

    public static IDisplayApi GetDisplayApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        IDisplayApi displayApi = modulCode.ToUpper() switch
        {
            "REMOTEDISPLAY" => new RemoteDisplay(device),
            _ => throw new NotImplementedException($"Modulcode.Display {modulCode}")
        };
        return displayApi;
    }

    public static ICamApi GetCamApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        ICamApi displayApi = modulCode.ToUpper() switch
        {
            "HTTPCAM" => new HttpCam(device),
            _ => throw new NotImplementedException($"Modulcode.Cam {modulCode}")
        };
        return displayApi;
    }
}
