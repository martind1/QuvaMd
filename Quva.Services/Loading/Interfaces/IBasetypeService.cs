using Quva.Database.Models;

namespace Quva.Services.Loading.Interfaces
{
    public interface IBasetypeService
    {
        Task<BasetypeSilos> GetAll(long idLocation);
        Task<BasetypeSilos> GetByDelivery(DeliveryHead delivery, long? idLoadingPoint);
        Task<BasetypeSilos> GetByDelivery(long idDelivery, long? idLoadingPoint);
    }
}