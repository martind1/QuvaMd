using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading;
using Serilog;

namespace Quva.Services.Services.Shared;

public class LoadingService : ILoadingService
{
    private readonly ILogger _log;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICustomerAgreementService _customerAgreementService;

    public LoadingService(IServiceScopeFactory scopeFactory, ICustomerAgreementService customerAgreementService)
    {
        _log = Log.ForContext(GetType());
        _scopeFactory = scopeFactory;
        _customerAgreementService = customerAgreementService;
    }

    public async Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuvaContext>();

        return await BasetypeSilos.CreateAll(new BtsContext(context, _customerAgreementService, _log, idLocation));

    }

    public async Task<BasetypeSilosView> GetBasetypeSilosAllView(long idLocation)
    {
        var baseTypeSilos = await GetBasetypeSilosAll(idLocation);

        return BasetypeSilosView.FromBasetypeSilos(baseTypeSilos);
    }

    public async Task<BasetypeSilos> GetBasetypeSilosByDelivery(long idDelivery)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuvaContext>();

        var btsc = new BtsContext(context, _customerAgreementService, _log, 0);
        return await BasetypeSilos.CreateByDelivery(btsc, idDelivery, null);

    }


    public async Task<LoadingResult> CreateLoadorder(LoadingParameter parameter)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuvaContext>();

        var btsc = new BtsContext(context, _customerAgreementService, _log, parameter.IdLocation);
        return await LoadOrderService.CreateLoadorder(btsc, parameter);
    }

}
