using Quva.Services.Loading;

namespace Quva.Services.Interfaces
{
    public interface ILoadingService
    {
        Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation);
    }
}