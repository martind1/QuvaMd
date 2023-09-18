using Quva.Database.Models;
using Quva.Services.Enums;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading.Helper;
using Quva.Services.Loading.Interfaces;
using Serilog;

namespace Quva.Services.Loading;

public partial class BasetypeService : IBasetypeService
{
    private readonly ILogger _log;
    private readonly ILoadingDbService _loadingDbService;
    private readonly IAgreementsService _agreementsService;

    private readonly List<ErrorLine> debugList = new();

    public BasetypeService(ILoadingDbService loadingDbService, IAgreementsService agreementsService)
    {
        _log = Log.ForContext(GetType());
        _loadingDbService = loadingDbService;
        _agreementsService = agreementsService;
    }

    public async Task<BasetypeSilos> GetAll(long idLocation)
    {
        // alle Basetyp Silos der Location. Entspricht alter View. 
        // Ohne Kontingente, Ohne Kundenvereinbarungen
        // mit Mischsorten (MappingBaseType). Mit Grundsorte2 (ADDITIONAL_BASIC_TYPE)

        return await GetByFilter(
            new BaseTypeSiloFilter
            {
                IdLocation = idLocation,
                idMaterial = null,
            },
            null);

    }

    public async Task<BasetypeSilos> GetByDelivery(long idDelivery, long? idLoadingPoint)
    {
        var delivery = await _loadingDbService.FindDelivery(idDelivery);
        if (delivery == null)
        {
            BasetypeSilos result = new();
            //AddError(result, $"Delivery not found ID:{idDelivery}");
            result.AddError(TrCode.LoadingService.DeliveryNotFound, idDelivery);
            return result;
        }
        return await GetByDelivery(delivery, idLoadingPoint);
    }

    public async Task<BasetypeSilos> GetByDelivery(DeliveryHead delivery, long? idLoadingPoint)
    {
        BasetypeSilos? result = null;
        var debitorNumber = delivery.DeliveryOrder?.DeliveryOrderDebitor.FirstOrDefault()?.DebitorNumber;
        var idDebitor = await _loadingDbService.GetIdDebitorByNumber(debitorNumber ?? 0);
        if (idDebitor == 0)
        {
            result = new();
            //AddError(result, $"Debitor not found: {debitorNumber ?? -1} in DeliveryId {delivery.Id}");
            result.AddError(TrCode.LoadingService.DebitorNotFound, debitorNumber ?? -1, delivery.Id);
            //no return, only warning
        }
        long idLocation = delivery.DeliveryOrder?.IdPlantNavigation.IdLocation ?? 0;
        if (idLocation == 0)
        {
            result = new();
            //AddError(result, $"Location is null; in DeliveryId {delivery.Id}");
            result.AddError(TrCode.LoadingService.LocationNull, delivery.Id);
            return result;
        }

        var materialCode = delivery.DeliveryOrder?.DeliveryOrderPosition?.FirstOrDefault()?.MaterialShortName;
        var idMaterial = await _loadingDbService.GetIdMaterialByCode(materialCode ?? "0");
        if (idMaterial == 0)
        {
            result = new();
            //AddError(result, $"Material not found; in DeliveryId Mat:{materialCode} DelId:{delivery.Id}");
            result.AddError(TrCode.LoadingService.MaterialNotFound, materialCode ?? "0", delivery.Id);
            return result;
        }

        var agr = await _agreementsService.GetAgreementsByDebitorMaterial(idLocation, idDebitor, idMaterial);
        var kontPflicht = agr.GetParameter<bool>("KONT_PFLICHT");

        var filter = new BaseTypeSiloFilter
        {
            IdLocation = idLocation,
            idDebitor = idDebitor,
            idMaterial = idMaterial,
            ContingentRequired = kontPflicht,
        };
        if (idLoadingPoint != null)
        {
            filter.idLoadingPoints.Add((long)idLoadingPoint);
        }
        return await GetByFilter(filter, result);
    }

    private async Task<BasetypeSilos> GetByFilter(BaseTypeSiloFilter filter, BasetypeSilos? result)
    {
        // Erzeugt BaseTypeSilos mit den SiloSets
        // Generische Basisfunktion anhand Filter
        result ??= new();
        result.filter = filter;

        if (filter.idDebitor != null && filter.idMaterial != null)
        {
            await AddSpecificContingents(result);
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
                    //AddError(result, "keine gültigen Kontingente trotz Kontingentpflicht; IdDebitor:{} IdMaterial:{}");
                    result.AddError(TrCode.LoadingService.ContingentRequired, filter.idDebitor, filter.idMaterial);
                    return result;
                }
            }
        }
        //Nein! else if (filter.idDebitor == null)
        {
            await AddGeneralContingents(result);
            // Allgemeine Kontingente vorhanden -> weiter suchen (wie bisher bei Mischsilos)
        }

        await AddTrueBasicTypes(result);   // reine Grundsorten
        await AddMixedBasicTypes(result);  // Mischsorten

        return result;
    }


    public List<string> DebugStringList(LanguageEnum language)
    {
        List<string> result = new();
        foreach (var err in debugList)
        {
            result.Add(err.ToString(language));
        }
        return result;
    }
}
