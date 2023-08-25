using Quva.Database.Models;
using Serilog;

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

    public static async Task<BasetypeSilos> CreateFromFilter(BtsContext btsc, BaseTypeSiloFilter filter)
    {
        // Erzeugt BaseTypeSilos mit den SiloSets
        // Generische Basisfunktion anhand Filter
        BasetypeSilos result = new();
        // BaseTypes mit Material:
        List<BasicType> basicTypes = await LoadingDbService.GetBasicTypesFromMaterialId(btsc, filter.idMaterial);

        // plus MapingBasicType
        foreach (var basicType in basicTypes)
        {
            var mappedBasicTypes = await LoadingDbService.GetMappedTypesFromBasicType(btsc, basicType.Id);
            //if (basicType.MixIndex <= 0)
            if (mappedBasicTypes.Count <= 0)
            {
                if (basicType.MixIndex > 0)
                {
                    btsc.log.Error($"Falscher MixIndex {basicType.MixIndex}. Muss 0 sein wenn kein Mix. BasicType:{basicType.Id}");
                }
                var lpSilos = await LoadingDbService.GetLoadingpointSilosFromBasictype(btsc, basicType.Id);
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
                    };
                    SiloItem siloItem = new()
                    {
                        Position = 1,
                        TheSilo = silo,
                        Percentage = 999,
                    };
                    siloSet.SiloItems.Add(siloItem);
                    result.SiloSets.Add(siloSet);
                }

            }
            else
            {
                if (basicType.MixIndex == 0)
                {
                    btsc.log.Error($"Falscher MixIndex 0. Muss >0 sein bei Mix. BasicType:{basicType.Id}");
                }
                List<List<MappingSiloLoadingPoint>> siloLists = new();  //Listen von Silolisten+LoadingPoint
                foreach (var mappedbasicType in mappedBasicTypes)
                {
                    var lpSilos = await LoadingDbService.GetLoadingpointSilosFromBasictype(btsc, mappedbasicType.IdOtherType);
                    siloLists.Add(lpSilos);
                    if (lpSilos.Count() == 0)
                    {
                        btsc.log.Error($"kein Silo für Material:{basicType.IdMaterialNavigation.Code} BasicType:{basicType.Id}");
                    }
                }


                // Works Varianten aller Silo Kombinationen
                foreach (IEnumerable<object> siloObjects in CartesianProductContainer.Cartesian(siloLists))
                {
                    List<string> debugList = new();

                    List<MappingSiloLoadingPoint> siloSets = new();
                    foreach (MappingSiloLoadingPoint silo in siloObjects)
                    {
                        debugList.Add(silo.IdSiloNavigation.SiloNumber.ToString() + ':' + silo.IdLoadingPointNavigation.LoadingNumber);

                        siloSets.Add(silo);
                    }

                    btsc.log.Debug(string.Join(", ", debugList));
                }
                btsc.log.Debug("slp end");
            }



        }


        return new BasetypeSilos();
    }

}

public record SiloSet
{
    // Verwaltung einer Silokombination: Silos mit Anteilen
    public static int MaxSiloItems = 9;

    public int Priority { get; set; }  //Berechnung nach werksbezogenen Regeln (zB 99 wenn Silostand niedrig)

    public LoadingPoint? TheLoadingPoint { get; set; }
    public BasicType? TheBasicType { get; set; }  // also MIX
    public int MixIndex { get; set; } = 0;

    public Contingent? TheContingent { get; set; }
    public int ContingentSiloset { get; set; }  //0,1,2

    public List<SiloItem> SiloItems { get; set; } = new List<SiloItem>();

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
                foreach (IEnumerable<object> siloObjects in siloSelect)
                {
                    List<string> debugList = new();
                    List<MappingSiloLoadingPoint> siloSets = new();
                    foreach (MappingSiloLoadingPoint silo in siloObjects)
                    {
                        debugList.Add(silo.IdSiloNavigation.SiloNumber.ToString());
                        siloSets.Add(silo);
                    }
                    btsc.log.Debug(string.Join(',', debugList));
                }
                btsc.log.Debug("slp end");
*/