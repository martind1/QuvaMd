﻿namespace Quva.Devices.Display;

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

public enum DisplayOptions
{
    Template,  //Platzhalter: #L=Zeile #F=Font #T=Text  ^M^J=Endekennung
    Width,     // (character count)
    Height,    // (line count)
    Font       // (1,2,3,) bisher immer 1
}

// Commands for Display:
public enum DisplayCommands
{
    Show
}

// Functions for automatic
public enum DisplayFunction
{
    HOH_FW1_WEIGHT,    //show scale display
    FRE_GET_GROSS_TEXT //compute text from available types
}
