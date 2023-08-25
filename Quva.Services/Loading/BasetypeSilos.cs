using Microsoft.Extensions.Logging.Abstractions;
using Quva.Database.Models;
using Serilog;
using System.Reflection.Metadata.Ecma335;

namespace Quva.Services.Loading;

public record BtsContext(QuvaContext context, ILogger log, long idLocation);

/// <summary>
/// Verwaltung der möglichen Silokombinationen. Analog View V_GRSO_SILOS
/// </summary>
public class BasetypeSilos
{
    public List<SiloSet> SiloSets { get; set; } = new();

    public static BasetypeSilos CreateFromDelivery(BtsContext btsc, long idDelivery)
    {
        // TODO: Options?
        return new BasetypeSilos();
    }

    public static async Task<BasetypeSilos> CreateAll(BtsContext btsc)
    {
        // alle Basetyp Silos der Location. Entspricht alter View. 
        // Ohne Kontingente, Ohne Kundenvereinbarungen
        // mit Mischsorten (MappingBaseType). Mit Grundsorte2 (ADDITIONAL_BASIC_TYPE)

        // TODO: Options?
        return await CreateFromFilter(btsc,
            new BaseTypeSiloFilter
            {
                idMaterial = null,
            });
    }

    private static async Task<BasetypeSilos> CreateFromFilter(BtsContext btsc, BaseTypeSiloFilter filter)
    {
        // Erzeugt BaseTypeSilos mit den SiloSets
        // Generische Basisfunktion anhand Filter
        BasetypeSilos result = new();
        List<string> debugList = new();

        // BaseTypes mit Material plus MapingBasicType
        List<BasicType> basicTypes = await LoadingDbService.GetBasicTypesFromMaterialId(btsc, filter.idMaterial);
        foreach (var basicType in basicTypes)
        {
            var mappedBasicTypes = await LoadingDbService.GetMappedTypesFromBasicType(btsc, basicType.Id);
            if (mappedBasicTypes.Count <= 0)  //if (basicType.MixIndex <= 0)
            {
                if (basicType.MixIndex > 0)
                {
                    btsc.log.Error($"Falscher MixIndex {basicType.MixIndex}. Muss 0 sein wenn kein Mix. BasicType:{basicType.IdMaterialNavigation.Code}");
                }
                var lpSilos = await LoadingDbService.GetLoadingpointSilosFromBasictype(btsc, basicType.Id, null);
                foreach (var lpSilo in lpSilos)
                {
                    Silo silo = lpSilo.IdSiloNavigation;
                    if (filter.idLoadingPoints.Count > 0 && !filter.idLoadingPoints.Contains(lpSilo.IdLoadingPoint))
                    {
                        continue;
                    }
                    SiloSet siloSet = new()
                    {
                        TheBasicType = basicType,
                        Priority = silo.Priority,
                        TheLoadingPoint = lpSilo.IdLoadingPointNavigation,
                        MixIndex = basicType.MixIndex
                    };
                    SiloItem siloItem = new()
                    {
                        Position = 1,
                        TheSilo = silo,
                        Percentage = 999,
                    };
                    siloSet.SiloItems.Add(siloItem);
                    result.AddSiloSet(siloSet);  // mit Checks, Priority
                }

            }
            else
            {
                // Mischsorten:
                if (basicType.MixIndex == 0)
                {
                    btsc.log.Error($"Falscher MixIndex 0. Muss >0 sein bei Mix. BasicType:{basicType.IdMaterialNavigation.Code}");
                }
                var loadingPoints = await LoadingDbService.GetLoadingPoints(btsc);
                foreach (var loadingPoint in loadingPoints)
                {
                    if (filter.idLoadingPoints.Count > 0 && !filter.idLoadingPoints.Contains(loadingPoint.Id))
                    {
                        continue;
                    }
                    List<List<MappingSiloLoadingPoint>> siloLists = new();  //Silo+LoadingPoint
                    Dictionary<int, decimal> percentageList = new();

                    debugList.Clear();
                    foreach (var mappedbasicType in mappedBasicTypes)
                    {
                        var lpSilos = await LoadingDbService.GetLoadingpointSilosFromBasictype(btsc, mappedbasicType.IdOtherType,
                            loadingPoint.Id);
                        if (lpSilos.Count > 0)
                        {
                            siloLists.Add(lpSilos);
                            var i = mappedBasicTypes.IndexOf(mappedbasicType);
                            percentageList[i] = mappedbasicType.Percentage;
                        }
                        if (lpSilos.Count == 0)
                        {
                            debugList.Add(string.Format("kein Silo für Material:{0} BasicType:{1} Point:{2}",
                                basicType.IdMaterialNavigation.Code, basicType.Id, loadingPoint.Name));
                        }
                    }
                    if (siloLists.Count == 0)
                    {
                        continue;
                    }
                    if (siloLists.Count < mappedBasicTypes.Count)
                    {
                        btsc.log.Error(string.Join(Environment.NewLine, debugList));
                        //continue;  //produktiv
                    }

                    // Varianten aller Silo Kombinationen
                    //btsc.log.Information($"Silo Kombinationen Basetype:{basicType.IdMaterialNavigation.Code} Point:{loadingPoint.Name}");
                    btsc.log.Information("Silo Kombinationen Basetype:{0} Point:{1}",
                        basicType.IdMaterialNavigation.Code, loadingPoint.Name);


                    foreach (IEnumerable<Object> siloList in CartesianProductContainer.Cartesian(siloLists))
                    {
                        debugList.Clear();
                        SiloSet siloSet = new()
                        {
                            TheBasicType = basicType,
                            Priority = 99,  // see AddSiloSet
                            TheLoadingPoint = loadingPoint,
                            MixIndex = basicType.MixIndex
                        };

                        int pos = 1;
                        foreach (var silo in siloList.Cast<MappingSiloLoadingPoint>())
                        {
                            SiloItem siloItem = new()
                            {
                                Position = pos,
                                TheSilo = silo.IdSiloNavigation,
                                Percentage = percentageList[pos - 1],
                            };
                            debugList.Add(
                                siloItem.Position.ToString() + ")" +
                                silo.IdLoadingPointNavigation.LoadingNumber + "." +
                                silo.IdSiloNavigation.SiloNumber.ToString() + 
                                " " + siloItem.Percentage + '%');

                            pos++;
                            siloSet.SiloItems.Add(siloItem);
                        }
                        btsc.log.Information(string.Join(", ", debugList));

                        result.AddSiloSet(siloSet);  // mit Checks, Priority
                    }

                }
            }
        }
        return result;
    }

    private void AddSiloSet(SiloSet siloSet)
    {
        // 1. sort Silonumber
        // 2. check if already exists
        // 3. compute priority when mix

        siloSet.SortSiloNumber();

        foreach (var silo in SiloSets)
        {
            if (silo.Equals(siloSet)) return;
        }

        siloSet.ComputePriotity();

        SiloSets.Add(siloSet);
    }

}

public record SiloSet
{
    // Verwaltung einer Silokombination: Silos mit Anteilen

    public int Priority { get; set; }  //Berechnung nach werksbezogenen Regeln (zB 99 wenn Silostand niedrig)

    public LoadingPoint? TheLoadingPoint { get; set; }
    public BasicType? TheBasicType { get; set; }  // also MIX
    public int MixIndex { get; set; } = 0;

    public Contingent? TheContingent { get; set; }
    public int ContingentSiloset { get; set; }  //0,1,2

    public List<SiloItem> SiloItems { get; set; } = new List<SiloItem>();

    public bool LockRail { get => SiloItems.Where(si => si.TheSilo!.LockRail == true).FirstOrDefault() != null; }
    public bool LockTruck { get => SiloItems.Where(si => si.TheSilo!.LockTruck == true).FirstOrDefault() != null; }
    public bool LockLaboratory { get => SiloItems.Where(si => si.TheSilo!.LockLaboratory == true).FirstOrDefault() != null; }
    public bool LockForSensitiveCustomer { get => SiloItems.Where(si => si.TheSilo!.LockForSensitiveCustomer == true).FirstOrDefault() != null; }

    public void SortSiloNumber()
    {
        //var query = SiloItems
        //            .OrderBy(si => si.TheSilo!.SiloNumber)
        //            .Select(si => si);
        //SiloItems = query.ToList();
        SiloItems = SiloItems.OrderBy(si => si.TheSilo!.SiloNumber).ToList();
    }

    public void ComputePriotity()
    {
        Priority = 0;
        if (SiloItems == null || SiloItems.Count == 0) return;
        // Standard: Niedrigste Prio (höchster Wert)
        Priority = SiloItems.Select(x => x.TheSilo!.Priority).Max();
        // Option: Wenn Silostand niedrig dann 99
        // Option: Wenn Mix dann +10
    }

    public virtual bool Equals(SiloSet? other)
    {
        if (other == null) return false;

        bool b1 = TheLoadingPoint != null && TheLoadingPoint.Equals(other.TheLoadingPoint);
        if (!b1) return false;

        foreach (SiloItem silo in SiloItems)
        {
            bool found = false;
            foreach (SiloItem otherSilo in other.SiloItems)
            {
                if (silo.Equals(otherSilo))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }

    public override int GetHashCode() => TheLoadingPoint!.GetHashCode() + SiloItems.GetHashCode();
}


public record SiloItem
{
    // Verwaltung eines Silos einer Kombination
    public int Position { get; set; }  // ab 1
    public Silo? TheSilo { get; set; }
    private decimal _percentage = 0;
    public decimal Percentage
    {
        get => _percentage;
        set => _percentage = TheSilo != null
                ? TheSilo.MaxPercentage is null or 0
                    ? Math.Min(value, 100)
                    : Math.Min((decimal)TheSilo.MaxPercentage, value)
                : value;
    }
    public decimal PowerTh { get; set; } = 0;  // in t/h, Haltern

    public virtual bool Equals(SiloItem? other)
    {
        return TheSilo != null && other != null && TheSilo == other.TheSilo;
    }

    public override int GetHashCode() => TheSilo!.GetHashCode();
}

public record BaseTypeSiloFilter
{
    // Filterwerte
    public long IdLocation { get; set; }
    public long? idDebitor { get; set; } = null;
    public long? idMaterial { get; set; } = null;
    public List<long> idLoadingPoints { get; set; } = new();
    public bool ContingentRequired { get; set; } = false;
}

/*
// Varianten aller Silo Kombinationen
//var listOfList = siloLists.CartesianProduct();
var listOfList = siloLists.CrossProduct18().ToArray();
var siloSelect = from si in listOfList select si;
foreach (IEnumerable<object> siloList in siloSelect)
{
    List<string> debugList = new();
    List<MappingSiloLoadingPoint> siloSets = new();
    foreach (MappingSiloLoadingPoint silo in siloList)
    {
        debugList.Add(silo.IdSiloNavigation.SiloNumber.ToString());
        siloSets.Add(silo);
    }
    btsc.log.Debug(string.Join(',', debugList));
}
btsc.log.Debug("slp end");
*/