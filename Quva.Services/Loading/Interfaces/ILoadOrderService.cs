using Quva.Services.Loading.Helper;

namespace Quva.Services.Loading.Interfaces
{
    public interface ILoadOrderService
    {
        Task<LoadingResult> CreateLoadorder(LoadingParameter parameter);

        Task ActivateLoadorder(long idLoadorder);
    }
}