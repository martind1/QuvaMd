namespace Quva.Devices.Display;

/// <summary>
/// Interface for concrete Device Dialog Api
/// </summary>
public interface IDisplayApi
{
    Task<DisplayData> DisplayCommand(string command, string message);

    DisplayData ShowData { get; set; }
}

public class DisplayData : DeviceData
{
    //für Fernanzeigen: keine spezifischen Rückgabenwerte
    public string Message = string.Empty;

    public DisplayData(string deviceCode, string command) : base(deviceCode, command)
    {
    }
}

// Commands for Display:
public enum DisplayCommands
{
    Show
}

