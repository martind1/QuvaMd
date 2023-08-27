using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Services.Interfaces;
using Quva.Services.Interfaces.Shared;
using Serilog;

namespace Quva.Services.Loading;

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
}
