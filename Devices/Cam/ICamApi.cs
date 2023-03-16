namespace Quva.Devices.Cam;

public interface ICamApi
{
    Task<CamData> CamCommand(string command, int camNumber);
}

public class CamData : DeviceData
{
    public CamData(string deviceCode, string command) : base(deviceCode, command)
    {
    }

    public string? Url { get; set; }
    public int CamNumber { get; set; }
    public byte[]? ImageBytes { get; set; }
    public int ImageSize { get; set; }
    public string? ImageFormat { get; set; }
    public CamStatus Status { get; set; }
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
    FormatError, //no picture format
    Timeout //no connection
}