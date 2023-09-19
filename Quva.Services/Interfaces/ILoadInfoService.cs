using Quva.Services.Loading.Helper;

namespace Quva.Services.Interfaces
{
    public interface ILoadInfoService
    {
        Task<LoadingInfo> GetLoadInfoByDelivery(long idDelivery);

        Task<LoadingInfo> GetLoadInfoByOrder(long idOrder, string vehicleNumber);

        Task<decimal> GetReducedMaxGross(long idLocation, decimal maxGross, long idPlant);
    }
}