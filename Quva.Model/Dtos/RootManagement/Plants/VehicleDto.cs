using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Plants;

public partial class VehicleDto : BaseDto<VehicleDto, Vehicle>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long IdCarrier { get; set; }

    public string LicensePlate { get; set; } = null!;

    public decimal? MaxGross { get; set; }

    public decimal? TareWeight { get; set; }

    public DateTime? TareDate { get; set; }

    public int? TareCounter { get; set; }

    public decimal? Subset1 { get; set; }

    public decimal? Subset2 { get; set; }

    public decimal? Subset3 { get; set; }

    public decimal? Subset4 { get; set; }

    public decimal? Subset5 { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public bool FixedTareFlag { get; set; }

    public bool RawMaterialFlag { get; set; }

    public bool OpenLoadingFlag { get; set; }

    public virtual CarrierDto? IdCarrierNavigation { get; set; } = null!;

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<IdentificationCardDto>? IdentificationCard { get; set; }
}
