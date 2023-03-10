using Devices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            PortType.Serial => throw new NotImplementedException("Serial not implemented"),
            PortType.Udp => throw new NotImplementedException("UDP not implemented"),
            PortType.None => null,
            _ => throw new NotImplementedException("only TCP implemented")
        };
        return comPort;
    }

    public static IScaleApi? GetScaleApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        IScaleApi? scaleApi = modulCode switch
        {
            "IT6000" => new ScaleIT6000(device),
            "FAWAWS" => new ScaleFawaWs(device),
            _ => throw new NotImplementedException($"Modulcode.Scale {modulCode}")
        };
        return scaleApi;
    }

    public static ICardApi? GetCardApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        ICardApi? cardApi = modulCode switch
        {
            "READER" => new CardReader(device),
            _ => throw new NotImplementedException($"Modulcode.Card {modulCode}")
        };
        return cardApi;
    }

    public static IDisplayApi? GetDisplayApi(ComDevice device)
    {
        ArgumentNullException.ThrowIfNull(device.Device.ModulCode);
        string modulCode = device.Device.ModulCode;
        IDisplayApi? displayApi = modulCode switch
        {
            "DEFAULT" => new DisplayDefault(device),
            _ => throw new NotImplementedException($"Modulcode.Display {modulCode}")
        };
        return displayApi;
    }
}
