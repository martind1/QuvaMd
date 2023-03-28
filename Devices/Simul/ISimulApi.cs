using Devices.Scale;

namespace Devices.Simul;

/// <summary>
///     Interface for Device Simulations
/// </summary>
public interface ISimulApi
{
    Task<SimulData> SimulCommand();
}


public class SimulData : DeviceData
{
    public SimulData(string deviceCode, string command) : base(deviceCode, command)
    {
    }

    public override void Reset()
    {
        base.Reset();
        Weight = 0;
        CalibrationNumber = 0;
        Unit = ScaleUnit.Ton;
        Status = ScaleStatus.None;
    }

    //for Scales:
    public double Weight { get; set; }
    public int CalibrationNumber { get; set; } //Eichnummer
    public ScaleUnit Unit { get; set; } //AppUnit
    public ScaleStatus Status { get; set; }
    //for IT9000
    public string? unitStr { get; set; }
    public bool Stillstand { get; set; }
    public bool Negative { get; set; }


    //für Card Reader/Dispencer:
    public string CardNumber { get; set; } = string.Empty;

}
