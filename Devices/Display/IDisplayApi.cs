namespace Devices.Display;

/// <summary>
///     Interface for concrete Device Dialog Api
/// </summary>
public interface IDisplayApi
{
    DisplayData ShowData { get; set; }
    Task<DisplayData> DisplayCommand(string command, string message);
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