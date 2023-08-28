using Quva.Services.Loading;

namespace Quva.Services.Interfaces.Shared
{
    public interface ILoadingService
    {
        Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation);

        Task<BasetypeSilosView> GetBasetypeSilosAllView(long idLocation);

        Task<BasetypeSilos> GetBasetypeSilosFromDelivery(long idDelivery);
    }
}