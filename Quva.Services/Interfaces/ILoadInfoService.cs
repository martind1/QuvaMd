using Quva.Services.Loading;

namespace Quva.Services.Interfaces
{
    public interface ILoadInfoService
    {
        Task<LoadingInfo> GetLoadInfo(long idDelivery);
    }
}