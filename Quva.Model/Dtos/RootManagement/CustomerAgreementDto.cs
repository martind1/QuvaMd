using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class CustomerAgreementDto : BaseDto<CustomerAgreementDto, CustomerAgreement>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long? IdPlant { get; set; }

    public long? IdCountry { get; set; }

    public long? IdMaterial { get; set; }

    public int PackagingType { get; set; }

    public int TransportType { get; set; }

    public bool FlagGoodsRecipient { get; set; }

    public bool FlagInvoiceRecipient { get; set; }

    public bool FlagCarrier { get; set; }

    public bool Active { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public long IdCategory { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long? IdDebitor { get; set; }

    public virtual ICollection<CustomerAgrParameterDto>? CustomerAgrParameter { get; set; }

    public virtual TypeAgreementCategoryDto? IdCategoryNavigation { get; set; }
    
    public virtual CountryDto? IdCountryNavigation { get; set; }

    public virtual DebitorDto? IdDebitorNavigation { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual MaterialDto? IdMaterialNavigation { get; set; }

    public virtual PlantDto? IdPlantNavigation { get; set; }
}
