namespace Devices.Scale;

/// <summary>
///     Interface for concrete Device Dialog Api
/// </summary>
public interface IScaleApi
{
    ScaleData StatusData { get; set; }
    Task<ScaleData> ScaleCommand(string command);
}

public class ScaleData : DeviceData
{
    public ScaleData(string deviceCode, string command) : base(deviceCode, command)
    {
    }

    public override void Reset()
    {
        base.Reset();
        Display = string.Empty;
        Weight = 0;
        CalibrationNumber = 0;
        Unit = ScaleUnit.Ton;
        Status = ScaleStatus.None;
    }

    //für Waagen:
    public string Display { get; set; } = string.Empty; //<Wert Einheit>, Wert+Einh, Fehlertext
    public double Weight { get; set; }
    public int CalibrationNumber { get; set; } //Eichnummer
    public ScaleUnit Unit { get; set; } //AppUnit
    public ScaleStatus Status { get; set; }
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
    None = 0,
    WeightOK = 1,
    Underload = 2, //Unterlast
    Overload = 4, //Überlast
    NoStandstill = 8, //kein Stillstand
    NoWeight = 16, //kein Gewichtswert
    Null = 32, //Null Gewicht
    Timeout = 64, //keine Verbindung
    ScaleFault = 128, //Waagenstörung
    PrinterFault = 256, //Druckerstörung
    MemoryFault = 512, //Speicherfehler
    PositionError = 1024, //falsche Position
    ScaleNumberError = 2048 //falsche Waagennummer
}