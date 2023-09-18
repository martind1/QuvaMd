using Quva.Database.Models;
using Quva.Services.Enums;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading.Helper;
using Quva.Services.Loading.Interfaces;
using Serilog;

namespace Quva.Services.Loading;

public class LoadOrderService : ILoadOrderService
{
    private readonly ILogger _log;
    private readonly ILoadingDbService _loadingDbService;
    private readonly IBasetypeService _basetypeService;
    private readonly IAgreementsService _agreementsService;

    public LoadOrderService(ILoadingDbService loadingDbService,
        IBasetypeService basetypeService,
        IAgreementsService agreementsService)
    {
        _log = Log.ForContext(GetType());
        _loadingDbService = loadingDbService;
        _basetypeService = basetypeService;
        _agreementsService = agreementsService;
    }

    public async Task ActivateLoadorder(long idLoadorder)
    {

        LoadingPoint? loadingPoint = await _loadingDbService.GetLoadingPointByLoadorder(idLoadorder);
        ArgumentNullException.ThrowIfNull(loadingPoint, $"No LoadingPoint for Loadorder Id({idLoadorder})");
        if (loadingPoint.IdLoadorder != null)
        {
            // TODO: erkennen ob bereits Beladung läuft (LoadingPoint.Loadorder.State)
        }
        await _loadingDbService.SetLoadingPointLoadorder(loadingPoint, idLoadorder);
    }

    public async Task<LoadingResult> CreateLoadorder(LoadingParameter parameter)
    {
        LoadingResult result = new();

        var delivery = await _loadingDbService.FindDelivery(parameter.IdDelivery);
        if (delivery == null)
        {
            //AddError(result, $"IdDelivery not found ({parameter.IdDelivery})");
            AddError(result, TrCode.LoadingService.DeliveryNotFound, parameter.IdDelivery);
            return result;
        }

        var loadingPoints = await _loadingDbService.GetLoadingPointsByShippingMethod(parameter.IdLocation,
            delivery.DeliveryOrder!.IdShippingMethodNavigation);
        if (loadingPoints.Count == 0)
        {
            //AddError(result, $"No Loading Points for IdDelivery:({parameter.IdDelivery})");
            AddError(result, TrCode.LoadingService.NoLoadingPoints, parameter.IdDelivery);
            return result;
        }

        if (delivery.DeliveryOrder.DeliveryOrderPosition.Count <= 0)
        {
            //AddError(result, $"No Main Positions for IdDelivery:({parameter.IdDelivery})");
            AddError(result, TrCode.LoadingService.NoMainPositions, parameter.IdDelivery);
            return result;
        }

        long idDebitor = await _loadingDbService.GetIdDebitorByNumber(
            delivery.DeliveryOrder.DeliveryOrderDebitor.First().DebitorNumber);
        long idMaterial = await _loadingDbService.GetIdMaterialByCode(
            delivery.DeliveryOrder.DeliveryOrderPosition.First().MaterialShortName);
        var agr = await _agreementsService.GetAgreementsByDebitorMaterial(parameter.IdLocation, idDebitor, idMaterial);

        foreach (var loadingPoint in loadingPoints)
        {
            // Check if active Loadorder with this Point already exists
            var activeStates = new int[] { (int)LoadorderStateValues.ToLoad, (int)LoadorderStateValues.LoadingRunning };
            var activeOrder = await _loadingDbService.GetActiveLoadorder(parameter.IdDelivery, loadingPoint.Id, activeStates);
            if (activeOrder != null)
            {
                //AddError(result, $"Active Loadorder already exists. Point({loadingPoint.Name}) ID({activeOrder.Id})");
                AddError(result, TrCode.LoadingService.LoadorderExists, loadingPoint.Name, activeOrder.Id);
                continue;
            }
            LoadorderHead hdr = new()
            {
                // obligatory:
                Id = 0,
                IdDelivery = parameter.IdDelivery,
                PositionNumber = delivery.DeliveryOrder.DeliveryOrderPosition.First().PositionNumber,
                LoadorderState = (int)LoadorderStateValues.ToLoad,
                IdLoadingPoint = loadingPoint.Id,
                CreateUser = delivery.CreateUser,

                TargetQuantity = parameter.TargetQuantity,
                MaxGross = delivery.MaxGross,
                WeighingUnit = delivery.NetUnit,

                // no, let null - ActivePartNumber = 1,
                // MoistLock (from CustAgree)
                MoistLock = agr.GetParameter<decimal?>(TypeAgreementOptionCode.SPERR_FEUCHTE),
            };
            // TODO: set Flag* Fields = 0 or from CustAgree/LocParam

            // Teilmengen:
            if (parameter.PartQuantities == null || parameter.PartQuantities.Count <= 0)
            {
                LoadorderPart part = new()
                {
                    PartNumber = 1,
                    TargetQuantity = hdr.TargetQuantity,
                    CreateUser = delivery.CreateUser,
                };
                hdr.LoadorderPart.Add(part);
            }
            else
            {
                int partNumber = 1;
                foreach (var quant in parameter.PartQuantities)
                {
                    LoadorderPart part = new()
                    {
                        PartNumber = partNumber++,
                        TargetQuantity = quant,
                        CreateUser = delivery.CreateUser,
                    };
                    hdr.LoadorderPart.Add(part);
                }
            }

            // Silos:
            List<SiloSet> siloSets;
            if (parameter.SiloSets.Count > 0)
            {
                //AddError(result, "LoadingParameter SiloSet is not implemented");
                //return result;
                siloSets = parameter.SiloSets;

                // TODO: unpassende wg LoadingPoint entfernen
            }
            else
            {
                var basetypeSilos = await _basetypeService.GetByDelivery(delivery, loadingPoint.Id);

                // Regeln prüfen, Silosets ggf entfernen:
                basetypeSilos.ApplyRules(new RuleToApply()
                {
                    TransportType = (TransportTypeValues)delivery.DeliveryOrder.IdShippingMethodNavigation.TransportType,
                    LoadingQuantity = parameter.TargetQuantity,
                    SensitiveCustomer = agr.GetParameter<bool>(TypeAgreementOptionCode.SENSITIVE_CUSTOMER),
                    LockRole = (LockRoleValues)loadingPoint.LockRole,  //BigBag, Truck2
                });

                result.AddErrorLines(basetypeSilos.ErrorLines);
                if (basetypeSilos.SiloSets.Count == 0)
                {
                    //"No Silos for IdDelivery:({parameter.IdDelivery}) Point({loadingPoint.LoadingNumber})"
                    AddError(result, TrCode.LoadingService.NoSiloDelivery, parameter.IdDelivery, loadingPoint.LoadingNumber);
                    return result;
                }
                basetypeSilos.SortByPrio();  // sorts silosets
                siloSets = basetypeSilos.SiloSets;
            }
            int idx = 0;
            foreach (var siloset in siloSets)
            {
                foreach (var siloitem in siloset.SiloItems)
                {
                    LoadorderSilo silo = new()
                    {
                        SiloSet = idx,
                        Position = siloitem.Position,
                        IdSilo = siloitem.TheSilo?.Id,
                        IdContingentSilo = siloitem.TheContingentSilo?.Id,
                        SiloNumber = siloitem.TheSilo?.SiloNumber,
                        Akz = siloitem.TheSilo?.Akz,
                        SpsCode = siloitem.TheSilo?.SpsCode,
                        IdBasicType = siloitem.TheSilo?.IdBasicType,
                        SiloLevelVolume = siloitem.TheSilo?.SiloLevelVolume,

                        Percentage = siloitem.Percentage,
                        PowerTh = siloitem.PowerTh,

                        CreateUser = delivery.CreateUser
                    };
                    hdr.LoadorderSilo.Add(silo);
                }
                idx++;
            }

            // persist:
            var idLoadorder = await _loadingDbService.SaveLoadorder(hdr);
            result.IdLoadorders.Add(idLoadorder);
            result.LoadingPoints.Add(loadingPoint.Name);

        }  // loadingPoint

        await Task.Delay(0);
        return result;
    }

    private void AddError(LoadingResult loadingResult, string code, params object[] parameter)
    {
        var line = new ErrorLine(code, parameter);
        _log.Warning(line.ToString(LanguageEnum.EN));
        loadingResult.ErrorLines.Add(line);
    }

}
