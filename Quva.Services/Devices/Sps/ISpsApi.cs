using Quva.Database.Models;
using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Sps;
using Quva.Services.Enums;

namespace Quva.Services.Devices.Sps;

public interface ISpsApi
{
    int PollInterval { get; set; }
    Task<SpsData> SpsCommand(string command);
}

public class SpsData : DeviceData
{
    public SpsData(string deviceCode, string command) : base(deviceCode, command)
    {
    }

    public void FromLoadorder(LoadorderHead src)
    {
        IdLoadorder = src.Id;
        LoadorderState = (LoadorderStateValues)src.LoadorderState;
        StateText = src.StateText;
        ErrorMessages = src.ErrorMessages;
        TargetQuantity = src.TargetQuantity;
        ActualQuantity = src.ActualQuantity;
        MaxGross = src.MaxGross;
        TaraCalibNumber = src.TaraCalibNumber;
        TaraWeight = src.TaraWeight;
        GrossCalibNumber = src.GrossCalibNumber;
        GrossWeight = src.GrossWeight;
        NetCalibNumber = src.NetCalibNumber;
        NetWeight = src.NetWeight;
        FlagOffeneBel = src.FlagOffeneBel;
        FlagKombiKnz = src.FlagKombiKnz;
        FlagProbenahme = src.FlagProbenahme;
        FlagPattern = src.FlagPattern;
        FlagAnalyse = src.FlagAnalyse;
        FlagAuftWdhlg = src.FlagAuftWdhlg;
        PatternNumber = src.PatternNumber;
        SilochangesCount = src.SilochangesCount;
        ActivePartNumber = src.ActivePartNumber;
        MoistLock = src.MoistLock;
        MoistPercentage = src.MoistPercentage;
        // Teilmengen:
        foreach (var part in src.LoadorderPart)
        {
            switch (part.PartNumber)
            {
                case 1:
                    Part1Target = part.TargetQuantity;
                    Part1Actual = part.ActualQuantity;
                    break;
                case 2:
                    Part2Target = part.TargetQuantity;
                    Part2Actual = part.ActualQuantity;
                    break;
                case 3:
                    Part3Target = part.TargetQuantity;
                    Part3Actual = part.ActualQuantity;
                    break;
                case 4:
                    Part4Target = part.TargetQuantity;
                    Part4Actual = part.ActualQuantity;
                    break;
                case 5:
                    Part5Target = part.TargetQuantity;
                    Part5Actual = part.ActualQuantity;
                    break;
            }
        }

        //Berechnungen:
        TotalPercentage = ActualQuantity * 100 / TargetQuantity;
        Part1Percentage = Part1Actual * 100 / Part1Target;
        Part2Percentage = Part2Actual * 100 / Part2Target;
        Part3Percentage = Part3Actual * 100 / Part3Target;
        Part4Percentage = Part4Actual * 100 / Part4Target;
        Part5Percentage = Part5Actual * 100 / Part5Target;
    }

    public void ToSpsData(SpsData dst)
    {
        dst.IdLoadorder = IdLoadorder;
        dst.LoadorderState = LoadorderState;
        dst.StateText = StateText;
        dst.ErrorMessages = ErrorMessages;
        dst.TargetQuantity = TargetQuantity;
        dst.ActualQuantity = ActualQuantity;
        dst.MaxGross = MaxGross;
        dst.TaraCalibNumber = TaraCalibNumber;
        dst.TaraWeight = TaraWeight;
        dst.GrossCalibNumber = GrossCalibNumber;
        dst.GrossWeight = GrossWeight;
        dst.NetCalibNumber = NetCalibNumber;
        dst.NetWeight = NetWeight;
        dst.FlagOffeneBel = FlagOffeneBel;
        dst.FlagKombiKnz = FlagKombiKnz;
        dst.FlagProbenahme = FlagProbenahme;
        dst.FlagPattern = FlagPattern;
        dst.FlagAnalyse = FlagAnalyse;
        dst.FlagAuftWdhlg = FlagAuftWdhlg;
        dst.PatternNumber = PatternNumber;
        dst.SilochangesCount = SilochangesCount;
        dst.ActivePartNumber = ActivePartNumber;
        dst.MoistLock = MoistLock;
        dst.MoistPercentage = MoistPercentage;

        dst.TotalPercentage = TotalPercentage;

        dst.Part1Actual = Part1Actual;
        dst.Part1Target = Part1Target;
        dst.Part1Percentage = Part1Percentage;
        dst.Part2Actual = Part2Actual;
        dst.Part2Target = Part2Target;
        dst.Part2Percentage = Part2Percentage;
        dst.Part3Actual = Part3Actual;
        dst.Part3Target = Part3Target;
        dst.Part3Percentage = Part3Percentage;
        dst.Part4Actual = Part4Actual;
        dst.Part4Target = Part4Target;
        dst.Part4Percentage = Part4Percentage;
        dst.Part5Actual = Part5Actual;
        dst.Part5Target = Part5Target;
        dst.Part5Percentage = Part5Percentage;
    }

    public SpsStatus Status { get; set; }
    public int LoadingPoint { get; set; }

    public long IdLoadorder { get; set; }

    public LoadorderStateValues LoadorderState { get; set; }
    public string? StateText { get; set; }
    public string? ErrorMessages { get; set; }

    public decimal? TargetQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
    public decimal? MaxGross { get; set; }
    public string? TaraCalibNumber { get; set; }
    public decimal? TaraWeight { get; set; }
    public string? GrossCalibNumber { get; set; }
    public decimal? GrossWeight { get; set; }
    public string? NetCalibNumber { get; set; }
    public decimal? NetWeight { get; set; }

    public bool? FlagOffeneBel { get; set; }
    public bool? FlagKombiKnz { get; set; }
    public bool? FlagProbenahme { get; set; }
    public bool? FlagPattern { get; set; }
    public bool? FlagAnalyse { get; set; }
    public bool? FlagAuftWdhlg { get; set; }
    public string? PatternNumber { get; set; }

    public int? SilochangesCount { get; set; }
    public int? ActivePartNumber { get; set; }
    public decimal? MoistLock { get; set; }
    public decimal? MoistPercentage { get; set; }

    // Parts
    public decimal? Part1Target { get; set; }
    public decimal? Part2Target { get; set; }
    public decimal? Part3Target { get; set; }
    public decimal? Part4Target { get; set; }
    public decimal? Part5Target { get; set; }
    public decimal? Part1Actual { get; set; }
    public decimal? Part2Actual { get; set; }
    public decimal? Part3Actual { get; set; }
    public decimal? Part4Actual { get; set; }
    public decimal? Part5Actual { get; set; }

    // Calculated:
    public decimal? TotalPercentage { get; set; }
    public decimal? Part1Percentage { get; set; }
    public decimal? Part2Percentage { get; set; }
    public decimal? Part3Percentage { get; set; }
    public decimal? Part4Percentage { get; set; }
    public decimal? Part5Percentage { get; set; }
}

// Commands for Sps Device:
public enum SpsCommands
{
    None,
    Read
}

//Statusflags for Sps Device
[Flags]
public enum SpsStatus
{
    Ok,
    NotFound, //Error getting Data
    Timeout //no connection
}