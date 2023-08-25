using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Services.Interfaces;
using Quva.Services.Services.Shared;
using Serilog;

namespace Quva.Services.Loading;

public class LoadingService : ILoadingService
{
    private readonly ILogger _log;
    private readonly IServiceScopeFactory _scopeFactory;

    public LoadingService(IServiceScopeFactory scopeFactory)
    {
        _log = Log.ForContext(GetType());
        _scopeFactory = scopeFactory;
    }

    public async Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuvaContext>();

        return await BasetypeSilos.CreateAll(new BtsContext(context, _log, idLocation));

    }
}
