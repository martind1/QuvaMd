using Quva.Database.Models;
using Quva.Services.Interfaces.Shared;
using Serilog;

namespace Quva.Services.Loading;

public record BtsContext(QuvaContext context, ICustomerAgreementService customerAgreementService, ILogger log, long idLocation);

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

    public static async Task<BasetypeSilos> CreateFromDelivery(BtsContext btsc, long idDelivery)
    {
        var delivery = await LoadingDbService.FindDelivery(btsc, idDelivery);
        var debitorNumber = delivery?.DeliveryOrder?.DeliveryOrderDebitor.ElementAt(0).DebitorNumber;
        var idDebitor = await LoadingDbService.GetIdDebitorByNumber(btsc, debitorNumber ?? 0);
        if (idDebitor == 0)
        {
            return Error(btsc, $"kein Debitor in DeliveryId {idDelivery}");
        }

        var materialCode = delivery?.DeliveryOrder?.DeliveryOrderPosition.ElementAt(0).MaterialShortName;
        var idMaterial = await LoadingDbService.GetIdMaterialByCode(btsc, materialCode ?? "0");
        if (idMaterial == 0)
        {
            return Error(btsc, $"kein Material in DeliveryId {idDelivery}");
        }

        var agr = await btsc.customerAgreementService.GetAgreementsByDebitorMaterial(btsc.idLocation, idDebitor, idMaterial);
        var kontPflicht = (bool)agr.GetParameter("KONT_PFLICHT");

        return await CreateFromFilter(btsc,
            new BaseTypeSiloFilter
            {
                idDebitor = idDebitor,
                idMaterial = idMaterial,
                ContingentRequired = kontPflicht,
            }); 
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
            if (otherSiloSet.TheBasicType?.IdMaterialNavigation.Code !=
                siloSet.TheBasicType?.IdMaterialNavigation.Code) continue;

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

