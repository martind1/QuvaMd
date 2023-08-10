using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class DebitorDto : BaseDto<DebitorDto, Debitor>
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name1 { get; set; } = null!;

    public string? Name2 { get; set; }

    public string? Name3 { get; set; }

    public string Street { get; set; } = null!;

    public string? Housenumber { get; set; }

    public string? Postcode { get; set; }

    public string Location { get; set; } = null!;

    public string? Phone { get; set; }    

    public string? Language { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public bool? Active { get; set; }

    public long IdCountry { get; set; }

    public virtual CountryDto? IdCountryNavigation { get; set; } = null!;

    public virtual ICollection<ContingentDto>? Contingent { get; set; }

    public virtual ICollection<CustomerAgreementDto>? CustomerAgreements { get; set; }

    public virtual ICollection<IdentificationCardDto>? IdentificationCard { get; set; }

    public virtual ICollection<MappingSoDebitorDto>? MappingSoDebitor { get; set; }
}
