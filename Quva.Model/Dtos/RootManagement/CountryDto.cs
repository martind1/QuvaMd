using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class CountryDto : BaseDto<CountryDto, Country>
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool Eu { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<CustomerAgreementDto>? CustomerAgreement { get; set; }

    public virtual ICollection<DeliveryOrderDebitorDto>? DeliveryOrderDebitor { get; set; }

    public virtual ICollection<OrderDebitorDto>? OrderDebitor { get; set; }

    public virtual ICollection<DebitorDto> Debitor { get; set; }
}
