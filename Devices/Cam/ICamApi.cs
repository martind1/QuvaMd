using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices.Cam;

public interface ICamApi
{
    public Task<CamData> CamCommand(string command, int camNumber);

}

public class CamDataArgs : EventArgs
{
    public CamData Data { get; set; }

    public CamDataArgs(CamData camData)
    {
        Data = camData;
    }
}


public class CamData : DeviceData
{

    public string? Url { get; set; }
    public int CamNumber { get; set; }
    public byte[]? ImageBytes { get; set; }
    public int ImageSize { get; set; }
    public string? ImageFormat { get; set; }
    public CamStatus Status { get; set; }

    public CamData(string deviceCode, string command) : base(deviceCode, command)
    {
    }
}

// Commands for Cam Device:
public enum CamCommands
{
    None,
    Load
}

//Statusflags for Cam Device
[Flags]
public enum CamStatus
{
    Ok,
    FormatError,   //no picture format
    Timeout        //no connection
}
