using Quva.Services.Enums;
using Quva.Services.Interfaces;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading.Interfaces;
using Serilog;

namespace Quva.Services.Loading;

public class LoadInfoService : ILoadInfoService
{
    private readonly ILogger _log;
    private readonly ILoadingDbService _loadingDbService;
    private readonly IAgreementsService _agreementsService;
    private readonly ILocationParameterService _locationParameterService;

    public LoadInfoService(ILoadingDbService loadingDbService,
        IAgreementsService agreementsService,
        ILocationParameterService locationParameterService)
    {
        _log = Log.ForContext(GetType());
        _loadingDbService = loadingDbService;
        _agreementsService = agreementsService;
        _locationParameterService = locationParameterService;
    }

    public async Task<LoadingInfo> GetLoadInfoByDelivery(long idDelivery)
    {
        LoadingInfo result = new();

        var delivery = await _loadingDbService.FindDelivery(idDelivery) ??
            throw new Exception($"IdDelivery not found ({idDelivery})");
        long idLocation = delivery.DeliveryOrder?.IdPlantNavigation.IdLocation ?? 0;
        long idPlant = delivery.DeliveryOrder!.IdPlantNavigation.Id;
        long idDebitor = await _loadingDbService.GetIdDebitorByNumber(
                        delivery.DeliveryOrder!.DeliveryOrderDebitor.First().DebitorNumber);
        long idMaterial = await _loadingDbService.GetIdMaterialByCode(
                        delivery.DeliveryOrder.DeliveryOrderPosition.First().MaterialShortName);
        var agr = await _agreementsService.GetAgreementsByDebitorMaterial(idLocation, idDebitor, idMaterial);
        var vehicle = await _loadingDbService.GetVehicleByPlate(delivery.VehicleNumber ?? "0");

        result.MaxGross = await _locationParameterService.GetParameter<decimal>(idLocation,
                            ApplicationOption.WeighingMode.MaxGross, idPlant);
        var custMaxGross = agr.GetParameter<decimal>(TypeAgreementOptionCode.MAX_BRUTTO);
        if (custMaxGross > 0)
        {
            _log.Debug($"custMaxGross({idDebitor}, {idMaterial})={custMaxGross}");
            // Customer Agreement
            result.MaxGross = Math.Min(result.MaxGross, custMaxGross);
        }
        var vehicleMaxGross = vehicle?.MaxGross ?? 0;
        if (vehicleMaxGross > 0)
        {
            _log.Debug($"vehicleMaxGross({vehicle!.LicensePlate})={vehicleMaxGross}");
            // Vehicle
            result.MaxGross = Math.Min(result.MaxGross, vehicleMaxGross);
        }

        // MaxNet
        var orderQuantity = delivery.DeliveryOrder.DeliveryOrderPosition.First().OrderQuantity;
        var maxSingleQuantity = await _locationParameterService.GetParameter<decimal>(idLocation,
                                ApplicationOption.WeighingMode.MaxSingleQuantity, idPlant);
        if (orderQuantity <= maxSingleQuantity)
        {
            _log.Debug($"orderQuantity({delivery.DeliveryOrder.OrderNumber})={orderQuantity}");
            result.MaxNet = orderQuantity;
        }
        var custMaxNet = agr.GetParameter<decimal>(TypeAgreementOptionCode.MAX_NETTO);
        if (custMaxNet > 0)
        {
            _log.Debug($"custMaxNet({idDebitor}, {idMaterial})={custMaxNet}");
            result.MaxNet = custMaxNet;
        }

        // Cumulative
        result.CumulativeFlag = agr.GetParameter<bool>(TypeAgreementOptionCode.KUMULIERT);

        _log.Debug($"GetLoadInfoByDelivery({idDelivery}): {result}");

        return result;
    }

    public async Task<LoadingInfo> GetLoadInfoByOrder(long idOrder, string vehicleNumber)
    {
        LoadingInfo result = new();

        var order = await _loadingDbService.FindOrder(idOrder) ??
            throw new Exception($"IdOrder not found ({idOrder})");
        long idLocation = order.IdPlantNavigation.IdLocation;
        long idPlant = order.IdPlantNavigation.Id;
        long idDebitor = await _loadingDbService.GetIdDebitorByNumber(
                        order.OrderDebitor.First().DebitorNumber);
        long idMaterial = await _loadingDbService.GetIdMaterialByCode(
                        order.OrderPosition.First().MaterialShortName);
        var agr = await _agreementsService.GetAgreementsByDebitorMaterial(idLocation, idDebitor, idMaterial);
        var vehicle = await _loadingDbService.GetVehicleByPlate(vehicleNumber ?? "0");

        result.MaxGross = await _locationParameterService.GetParameter<decimal>(idLocation,
                            ApplicationOption.WeighingMode.MaxGross, idPlant);
        var custMaxGross = agr.GetParameter<decimal>(TypeAgreementOptionCode.MAX_BRUTTO);
        if (custMaxGross > 0)
        {
            _log.Debug($"custMaxGross({idDebitor}, {idMaterial})={custMaxGross}");
            // Customer Agreement
            result.MaxGross = Math.Min(result.MaxGross, custMaxGross);
        }
        var vehicleMaxGross = vehicle?.MaxGross ?? 0;
        if (vehicleMaxGross > 0)
        {
            _log.Debug($"vehicleMaxGross({vehicle!.LicensePlate})={vehicleMaxGross}");
            // Vehicle
            result.MaxGross = Math.Min(result.MaxGross, vehicleMaxGross);
        }

        // MaxNet
        var orderQuantity = order.OrderPosition.First().OrderQuantity;
        var maxSingleQuantity = await _locationParameterService.GetParameter<decimal>(idLocation,
                                ApplicationOption.WeighingMode.MaxSingleQuantity, idPlant);
        if (orderQuantity <= maxSingleQuantity)
        {
            _log.Debug($"orderQuantity({order.OrderNumber})={orderQuantity}");
            result.MaxNet = orderQuantity;
        }
        var custMaxNet = agr.GetParameter<decimal>(TypeAgreementOptionCode.MAX_NETTO);
        if (custMaxNet > 0)
        {
            _log.Debug($"custMaxNet({idDebitor}, {idMaterial})={custMaxNet}");
            result.MaxNet = custMaxNet;
        }

        // Cumulative
        result.CumulativeFlag = agr.GetParameter<bool>(TypeAgreementOptionCode.KUMULIERT);

        _log.Debug($"GetLoadInfoByOrder({idOrder}): {result}");

        return result;
    }

}
