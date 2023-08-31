using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading;
using Quva.Services.Loading.Interfaces;
using Serilog;

namespace Quva.Services.Services.Shared;

public class LoadingService : ILoadingService
{
    private readonly ILogger _log;
    private readonly IBasetypeService _basetypeService;
    private readonly ILoadOrderService _loadOrderService;

    public LoadingService(IBasetypeService basetypeService, ILoadOrderService loadOrderService)
    {
        _log = Log.ForContext(GetType());
        _basetypeService = basetypeService;
        _loadOrderService = loadOrderService;
    }

    public async Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation)
    {
        return await _basetypeService.GetAll(idLocation);

    }

    public async Task<BasetypeSilos> GetBasetypeSilosByDelivery(long idDelivery)
    {
        return await _basetypeService.GetByDelivery(idDelivery, null);
    }


    public async Task<LoadingResult> CreateLoadorder(LoadingParameter parameter)
    {
        _log.Information($"CreateLoadorder IdDel:{parameter.IdDelivery}, {parameter.TargetQuantity} t");
        return await _loadOrderService.CreateLoadorder(parameter);
    }

}
