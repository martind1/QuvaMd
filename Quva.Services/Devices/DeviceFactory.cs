using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.ComPort;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Devices.Simul;

namespace Quva.Services.Devices;

public class DeviceFactory
{
    public static IComPort? GetComPort(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.PortType);
        var portType = device.Device.PortType;
        IComPort? comPort = portType switch
        {
            PortType.Tcp => new TcpPort(device),
            PortType.Http => new HttpPort(device),
            PortType.Serial => new ComxPort(device),
            PortType.Udp => throw new NotImplementedException("UDP not implemented"),
            PortType.None => null,
            _ => throw new NotImplementedException($"PortType {portType} not implemented")
        };
        return comPort;
    }

    public static IScaleApi GetScaleApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        var modulCode = device.Device.ModulCode;
        IScaleApi Api = modulCode.ToUpper() switch
        {
            "IT9000" => new IT9000(device),
            "FAWAWS" => new FawaWs(device),
            _ => throw new NotImplementedException($"Modulcode.Scale {modulCode}")
        };
        return Api;
    }

    public static ISimulApi GetSimulApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        var modulCode = device.Device.ModulCode;
        ISimulApi Api = modulCode.ToUpper() switch
        {
            "SIM.IT9000" => new IT9000Simul(device),
            _ => throw new NotImplementedException($"Modulcode.Scale {modulCode}")
        };
        return Api;
    }

    public static ICardApi GetCardApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        var modulCode = device.Device.ModulCode;
        ICardApi Api = modulCode.ToUpper() switch
        {
            "READER" => new Reader(device),
            _ => throw new NotImplementedException($"Modulcode.Card {modulCode}")
        };
        return Api;
    }

    public static IDisplayApi GetDisplayApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        var modulCode = device.Device.ModulCode;
        IDisplayApi Api = modulCode.ToUpper() switch
        {
            "REMOTEDISPLAY" => new RemoteDisplay(device),
            _ => throw new NotImplementedException($"Modulcode.Display {modulCode}")
        };
        return Api;
    }

    public static ICamApi GetCamApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        var modulCode = device.Device.ModulCode;
        ICamApi Api = modulCode.ToUpper() switch
        {
            "HTTPCAM" => new HttpCam(device),
            _ => throw new NotImplementedException($"Modulcode.Cam {modulCode}")
        };
        return Api;
    }

    public static IModbusApi GetModbusApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        var modulCode = device.Device.ModulCode;
        IModbusApi Api = modulCode.ToUpper() switch
        {
            "WAGO" => new WagoController(device),
            _ => throw new NotImplementedException($"Modulcode.Modbus {modulCode}")
        };
        return Api;
    }
}