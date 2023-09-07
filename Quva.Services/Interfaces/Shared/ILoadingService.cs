using Quva.Services.Loading;

namespace Quva.Services.Interfaces.Shared
{
    public interface ILoadingService
    {
        Task ActivateLoadorder(long idLoadorder);
        Task<LoadingResult> CreateLoadorder(LoadingParameter parameter);
        Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation);
        Task<BasetypeSilos> GetBasetypeSilosByDelivery(long idDelivery);
        Task<LoadingInfo> GetLoadInfoByDelivery(long idDelivery);
        Task<LoadingInfo> GetLoadInfoByOrder(long idOrder, string vehicleNumber);
    }
}