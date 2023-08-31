namespace Quva.Services.Loading.Interfaces
{
    public interface ILoadOrderService
    {
        Task<LoadingResult> CreateLoadorder(LoadingParameter parameter);
    }
}