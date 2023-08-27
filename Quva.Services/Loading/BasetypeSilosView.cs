using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Quva.Services.Loading;

public class BasetypeSilosView
{
    public List<BasetypeSilosItem> SiloSets { get; set; } = new();

    public static BasetypeSilosView FromBasetypeSilos(BasetypeSilos basetypeSilos)
    {
        BasetypeSilosView result = new();
        foreach (var siloset in basetypeSilos.SiloSets)
        {
            BasetypeSilosItem item = new()
            {
                BaseTypeCode = siloset.TheBasicType!.IdMaterialNavigation.Code,
                MaterialName = siloset.TheBasicType!.IdMaterialNavigation.Name,
                MixIndex = siloset.TheBasicType.MixIndex,
                Contingent = siloset.TheContingent != null ? siloset.ContingentSiloset : null,
                Point = siloset.TheLoadingPoint!.LoadingNumber,
                Priority = siloset.Priority,
                LockRail = CharFromBool(siloset.LockRail),
                LockTruck = CharFromBool(siloset.LockTruck),
                LockLab = CharFromBool(siloset.LockLaboratory),
                LockSens = CharFromBool(siloset.LockForSensitiveCustomer),
            };
            foreach (var siloItem in siloset.SiloItems)
            {
                switch (siloItem.Position)
                {
                    case 1:
                        item.Silo1 = siloItem.TheSilo!.SiloNumber.ToString();
                        item.Part1 = siloItem.Percentage;
                        break;
                    case 2:
                        item.Silo2 = siloItem.TheSilo!.SiloNumber.ToString();
                        item.Part2 = siloItem.Percentage;
                        break;
                    case 3:
                        item.Silo3 = siloItem.TheSilo!.SiloNumber.ToString();
                        item.Part3 = siloItem.Percentage;
                        break;
                }
            }
            result.SiloSets.Add(item);
        }
        return result;
    }

    public string ToCsv()
    {
        string result = string.Empty;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";"
        };
        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream))
        using (StreamReader reader = new StreamReader(stream))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords<BasetypeSilosItem>(SiloSets);
            writer.Flush();
            stream.Position = 0;
            result = reader.ReadToEnd();
        }
        return result;
    }

    private static char CharFromBool(bool value)
    {
        return value ? 'X' : '-';
    }
}

public record BasetypeSilosItem
{
    // record wg ToString()
    public string BaseTypeCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public int MixIndex { get; set; }
    public int? Contingent { get; set; }
    public int Point { get; set; } = 0;
    public int Priority { get; set; } = 0;
    public char LockRail { get; set; } = '-';
    public char LockTruck { get; set; } = '-';
    public char LockLab { get; set; } = '-';
    public char LockSens { get; set; } = '-';
    public string? Silo1 { get; set; }  // auch für AKZ
    public decimal? Part1 { get; set; }  //auch für Leistung
    public string? Silo2 { get; set; }
    public decimal? Part2 { get; set; }
    public string? Silo3 { get; set; }
    public decimal? Part3 { get; set; }
}
