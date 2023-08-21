using Quva.Database.Models;
using Quva.Services.Services.Shared;

namespace Quva.Services.Interfaces.Shared;

public interface ICustomerAgreementService
{
    Task<ICustomerAgreements> GetAgreementsByDeliveryId(long idDeliveryHead);
    Task<ICustomerAgreements> GetAgreementsByFilter(long idLocation, AgreementsFilter filter);
}

public record AgreementsFilter
{
    public long? idPlant;
    public long? idCountry;
    public List<long> idMaterials = new();
    public long? idGoodsRecipient; 
    public long? idInvoiceRecipient; 
    public long? idCarrier;
    public PackagingType? packagingType; 
    public TransportType? transportType;
    public DateTime? validDate;
}

public interface ICustomerAgreements
{
    List<CustomerAgreement> Agreements { get; set; }
    List<TypeAgreementOption> DefaultValues { get; set; }
    List<VCustomerAgrParameter> Parameters { get; set; }

    object GetParameter(string code);
}