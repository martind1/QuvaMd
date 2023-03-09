using Quva.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

/// <summary>
/// Interface for concrete Device Dialog Api
/// </summary>
public interface IScaleApi
{
    public Task<ScaleData> ScaleCommand(string command);

    public ScaleData statusData { get; set; }
    public ScaleData registerData { get; set; }

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
    ScaleNumberError = 2048    //falsche Waagennummer
}
