using Quva.Database.Models;
using Quva.Services.Enums;

namespace Quva.Services.Interfaces.Shared;

public interface IAgreementsService
{
    Task<ICustomerAgreements> GetAgreementsByDebitorMaterial(long idLocation, long? idDebitor, long? idMaterial);
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
    public PackagingTypeValues? packagingType; 
    public TransportTypeValues? transportType;
    public DateTime? validDate;
}

public interface ICustomerAgreements
{
    List<CustomerAgreement> Agreements { get; set; }
    List<TypeAgreementOption> DefaultValues { get; set; }
    List<VCustomerAgrParameter> Parameters { get; set; }

    object GetParameter(string code);
}