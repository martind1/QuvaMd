using Quva.Services.Interfaces.Loading;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading;
using Quva.Services.Loading.Helper;
using Serilog;

namespace QuvaMd.Services.Services.Shared;

public class LoadingService : ILoadingService
{
    private readonly ILogger _log;
    private readonly IBasetypeService _basetypeService;
    private readonly ILoadOrderService _loadOrderService;
    private readonly ILoadInfoService _loadInfoService;

    public LoadingService(IBasetypeService basetypeService, ILoadOrderService loadOrderService, ILoadInfoService loadInfoService)
    {
        _log = Log.ForContext(GetType());
        _basetypeService = basetypeService;
        _loadOrderService = loadOrderService;
        _loadInfoService = loadInfoService;
    }

    public async Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation)
    {
        return await _basetypeService.GetAll(idLocation);

    }

    public async Task<BasetypeSilos> GetBasetypeSilosByDelivery(long idDelivery)
    {
        return await _basetypeService.GetByDelivery(idDelivery);
    }


    public async Task<LoadingResult> CreateLoadorder(LoadingParameter parameter)
    {
        _log.Information($"CreateLoadorder IdDel:{parameter.IdDelivery}, {parameter.TargetQuantity} t");
        return await _loadOrderService.CreateLoadorder(parameter);
    }
    public async Task<LoadingResult> UpdateLoadorder(LoadingParameter parameter)
    {
        _log.Information($"UpdateLoadorder IdDel:{parameter.IdDelivery}, {parameter.TargetQuantity} t");
        return await _loadOrderService.UpdateLoadorder(parameter);
    }

    public async Task StartLoadorder(long idLoadorder)
    {
        await _loadOrderService.StartLoadorder(idLoadorder);
    }
    public async Task FinishDeliveryLoadorder(long idDelivery)
    {
        await _loadOrderService.FinishDeliveryLoadorder(idDelivery);
    }
    public async Task ActivateLoadorder(long idLoadorder)
    {
        await _loadOrderService.ActivateLoadorder(idLoadorder);
    }

    public async Task<LoadingInfo> GetLoadInfoByDelivery(long idDelivery)
    {
        LoadingInfo info = await _loadInfoService.GetLoadInfoByDelivery(idDelivery);
        return info;
        // Exception when error
    }

    public async Task<LoadingInfo> GetLoadInfoByOrder(long idOrder, string vehicleNumber)
    {
        LoadingInfo info = await _loadInfoService.GetLoadInfoByOrder(idOrder, vehicleNumber);
        return info;
        // Exception when error
    }
}
