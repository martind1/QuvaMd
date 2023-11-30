using Quva.Services.Loading;
using Quva.Services.Loading.Helper;

namespace Quva.Services.Interfaces.Shared
{
    public interface ILoadingService
    {
        Task<LoadingResult> CreateLoadorder(LoadingParameter parameter);
        Task<LoadingResult> UpdateLoadorder(LoadingParameter parameter);
        Task StartLoadorder(long idLoadorder);
        Task ActivateLoadorder(long idLoadorder);
        Task FinishDeliveryLoadorder(long idDelivery);
        Task<BasetypeSilos> GetBasetypeSilosAll(long idLocation);
        Task<BasetypeSilos> GetBasetypeSilosByDelivery(long idDelivery);
        Task<LoadingInfo> GetLoadInfoByDelivery(long idDelivery);
        Task<LoadingInfo> GetLoadInfoByOrder(long idOrder, string vehicleNumber);
    }
}