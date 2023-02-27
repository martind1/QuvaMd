using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quva.Devices;

public enum DeviceType
{
    None,
    Scale,
    Card,
    Display,
    Cam,
    SpsVisu
}

[Flags]
public enum DeviceRoles
{
    None = 0,
    Entry = 1,  //'E'
    Exit = 2,   //'A'
}

public enum TransportType
{
    All,
    Truck,
    Rail
}

public enum PackagingType
{
    All,
    Bulk,
    Packaged
}

public enum ScaleUnit
{
    Ton,
    Kilogram,
    Gram
}

public static class DeviceUtils
{
    public static string UnitShort(ScaleUnit scaleUnit)
    {
        string result = scaleUnit switch
        {
            ScaleUnit.Ton => "t",
            ScaleUnit.Kilogram => "kg",
            ScaleUnit.Gram => "gr",
            _ => throw new NotImplementedException()
        };
        return result;
    }

}

public class DeviceData
{
    //für alle Geräte:
    public string DeviceCode { get; set; }
    public string Command { get; set; }
    public int ErrorNr { get; set; }
    public string ErrorText { get; set; } = string.Empty;

    public DeviceData(string deviceCode, string command)
    {
        DeviceCode = deviceCode;
        Command = command;
    }
}

public class ScaleData : DeviceData
{
    //für Waagen:
    public string Display { get; set; } = string.Empty;  //<Wert Einheit>, Wert+Einh, Fehlertext
    public double Weight { get; set; }
    public int CalibrationNumber { get; set; }  //Eichnummer
    public ScaleUnit Unit { get; set; }  //AppUnit
    public ScaleStatus Status { get; set; }

    public ScaleData(string deviceCode, string command) : base(deviceCode, command)
    {
    }
}

// Commands for Scale:
public enum ScaleCommands
{
    None,
    Status,
    Register
}

//Statusflags for Scalse
[Flags]
public enum ScaleStatus
{
    WeightOK = 1,
    Underload = 2,         //Unterlast
    Overload = 4,          //Überlast
    NoStandstill = 8,      //kein Stillstand
    NoWeight = 16,         //kein Gewichtswert
    Null = 32,             //Null Gewicht
    Timeout = 64,          //keine Verbindung
    ScaleFault = 128,      //Waagenstörung
    PrinterFault = 256,    //Druckerstörung
    MemoryFault = 512,     //Speicherfehler
    PositionError = 1024,  //falsche Position
    ScaleNoError = 2048    //falsche Waagennummer
}
