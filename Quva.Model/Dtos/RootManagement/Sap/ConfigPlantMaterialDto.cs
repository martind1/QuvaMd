using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class ConfigPlantMaterialDto : BaseDto<ConfigPlantMaterialDto, ConfigPlantMaterial>
{
    public long Id { get; set; }

    public long IdPlant { get; set; }

    public long IdMaterial { get; set; }

    public decimal BatchObligation { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdProductgroup { get; set; }

    public virtual ICollection<BatchDto>? Batch { get; set; }

    public virtual ICollection<ContingentDto>? Contingent { get; set; }

    public virtual MaterialDto? IdMaterialNavigation { get; set; } = null!;

    public virtual PlantDto? IdPlantNavigation { get; set; } = null!;
    public virtual ProductgroupDto? IdProductgroupNavigation { get; set; } = null!;
    public virtual ICollection<OdcAdjustmentQuantityDto>? OdcAdjustmentQuantity { get; set; }
}
