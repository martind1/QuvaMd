using Quva.Database.Models;
using Quva.Services.Interfaces.Shared;
using Serilog;

namespace Quva.Services.Loading;



/// <summary>
/// Verwaltung der möglichen Silokombinationen. Analog View V_GRSO_SILOS
/// </summary>
public partial class BasetypeSilos
{
    public List<SiloSet> SiloSets { get; set; } = new();
    public List<string> ErrorLines { get; set; } = new();
    public BtsContext btsc;
    public BaseTypeSiloFilter filter;
    private readonly List<string> debugList = new();

    public BasetypeSilos(BtsContext btsc, BaseTypeSiloFilter filter)
    {
        this.btsc = btsc;
        this.filter = filter;
    }

    public static async Task<BasetypeSilos> CreateByDelivery(BtsContext btsc, long idDelivery, long? idLoadingPoint)
    {
        var delivery = await LoadingDbService.FindDelivery(btsc, idDelivery);
        if (delivery == null)
        {
            return Error(btsc, $"Delivery not found ID:{idDelivery}");
        }
        return await CreateByDelivery(btsc, delivery, idLoadingPoint);
    }

    public static async Task<BasetypeSilos> CreateByDelivery(BtsContext btsc, DeliveryHead delivery, long? idLoadingPoint)
    {
        var debitorNumber = delivery.DeliveryOrder?.DeliveryOrderDebitor.FirstOrDefault()?.DebitorNumber;
        var idDebitor = await LoadingDbService.GetIdDebitorByNumber(btsc, debitorNumber ?? 0);
        if (idDebitor == 0)
        {
            btsc.log.Error($"Debitor not found: {debitorNumber ?? -1} in DeliveryId {delivery.Id}");
            //return Error(btsc, $"Debitor not found: {debitorNumber ?? -1} in DeliveryId {idDelivery}");
        }
        if (btsc.idLocation == 0)
        {
            // changing init-only property:
            btsc = new BtsContext(btsc.context, btsc.customerAgreementService, btsc.log,
                delivery.DeliveryOrder!.IdPlantNavigation.IdLocation);
        }

        var materialCode = delivery.DeliveryOrder?.DeliveryOrderPosition.ElementAt(0).MaterialShortName;
        var idMaterial = await LoadingDbService.GetIdMaterialByCode(btsc, materialCode ?? "0");
        if (idMaterial == 0)
        {
            return Error(btsc, $"kein Material in DeliveryId {delivery.Id}");
        }

        var agr = await btsc.customerAgreementService.GetAgreementsByDebitorMaterial(btsc.idLocation, idDebitor, idMaterial);
        var kontPflicht = (bool)agr.GetParameter("KONT_PFLICHT");

        var filter = new BaseTypeSiloFilter
        {
            idDebitor = idDebitor,
            idMaterial = idMaterial,
            ContingentRequired = kontPflicht,
        };
        if (idLoadingPoint != null)
        {
            filter.idLoadingPoints.Add((long)idLoadingPoint);
        }
        return await CreateByFilter(btsc, filter); 
    }

    public static async Task<BasetypeSilos> CreateAll(BtsContext btsc)
    {
        // alle Basetyp Silos der Location. Entspricht alter View. 
        // Ohne Kontingente, Ohne Kundenvereinbarungen
        // mit Mischsorten (MappingBaseType). Mit Grundsorte2 (ADDITIONAL_BASIC_TYPE)

        // TODO: Options?
        return await CreateByFilter(btsc,
            new BaseTypeSiloFilter
            {
                idMaterial = null,
            });
    }

    public void SortByPrio()
    {
        // Sorts and deletes 0-Priorities
        SiloSets = SiloSets
            .Where(o => o.Priority > 0)
            .OrderBy(o => o.Priority)
            .ToList();
    }

    private static async Task<BasetypeSilos> CreateByFilter(BtsContext btsc, BaseTypeSiloFilter filter)
    {
        // Erzeugt BaseTypeSilos mit den SiloSets
        // Generische Basisfunktion anhand Filter
        BasetypeSilos result = new(btsc, filter);

        if (filter.idDebitor != null && filter.idMaterial != null)
        {
            await result.AddSpecificContingents();
            if (result.SiloSets.Count > 0)
            {
                // Spezifische Kontingente vorhanden -> nur diese nehmen
                return result;
            }
            else
            {
                //if KUVE.Kontingentpflicht then Error + Exit
                if (filter.ContingentRequired)
                {
                    result.AddError("keine gültigen Kontingente trotz Kontingentpflicht");
                    return result;
                }
            }
        }
        else if (filter.idDebitor == null)
        {
            await result.AddGeneralContingents();
            // Allgemeine Kontingente vorhanden -> weiter suchen (wie bisher bei Mischsilos)
        }

        await result.AddTrueBasicTypes();   // reine Grundsorten
        await result.AddMixedBasicTypes();  // Mischsorten

        return result;
    }

    private bool AddSiloSet(SiloSet siloSet)
    {
        // 1. sort Silonumber
        // 2. check if already exists
        // 3. compute priority when mix

        siloSet.SortSiloNumber();

        foreach (var otherSiloSet in SiloSets)
        {
            if (otherSiloSet.Equals(siloSet)) return false;
        }

        siloSet.ComputePriotity();

        SiloSets.Add(siloSet);

        return true;
    }

    private void AddError(string message)
    {
        btsc.log.Error(message);
        ErrorLines.Add(message);
    }

    private static BasetypeSilos Error(BtsContext btsc, string message)
    {
        var result = new BasetypeSilos(btsc, new BaseTypeSiloFilter());
        result.AddError(message);
        return result;
    }

}

