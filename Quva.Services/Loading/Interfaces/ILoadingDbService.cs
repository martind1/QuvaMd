using Quva.Database.Models;

namespace Quva.Services.Loading.Interfaces
{
    public interface ILoadingDbService
    {
        Task<DeliveryHead?> FindDelivery(long idDelivery);
        Task<OrderHead?> FindOrder(long idOrder);
        Task<List<Contingent>> GetActiveContingents(long idLocation, long? idDebitor, long idMaterial, DateTime? date);
        Task<LoadorderHead?> GetActiveLoadorder(long idDelivery, long idLoadingPoint, int[] activeStates);
        Task<List<BasicType>> GetBasicTypesByMaterialId(long idLocation, long? idMaterial, bool onlyTrue);
        Task<long> GetIdDebitorByNumber(long debitorNumber);
        Task<long> GetIdMaterialByCode(string code);
        Task<Vehicle?> GetVehicleByPlate(string plate);
        Task<List<LoadingPoint>> GetLoadingPoints(long idLocation);
        Task<List<LoadingPoint>> GetLoadingPointsByShippingMethod(long idLocation, ShippingMethod shippingMethod);
        Task<List<LoadingPoint>> GetLoadingPointsBySilo(long idLocation, long idSilo);
        Task<List<MappingSiloLoadingPoint>> GetLoadingpointSilosByBasictype(long idBasicType, long? idLoadingPoint);
        Task<List<MappingBasicType>> GetMappedTypesByBasicType(long idBasicType);
        Task<List<Silo>> GetSilosByBasictype(long idBasicType);
        Task<long> SaveLoadorder(LoadorderHead loadorder);
    }
}