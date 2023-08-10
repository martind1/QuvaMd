using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class OdcAdjustmentQuantityDto : BaseDto<OdcAdjustmentQuantityDto, OdcAdjustmentQuantity>
{
    public long Id { get; set; }

    public long IdOdcAdjustmentDay { get; set; }

    public long IdConfigPlantMaterial { get; set; }

    public int SpMark { get; set; }

    public int Transporttype { get; set; }

    public int Packagingtype { get; set; }

    public decimal Quantity { get; set; }

    public bool? DelFlag { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ConfigPlantMaterialDto? IdConfigPlantMaterialNavigation { get; set; } = null!;

    public virtual OdcAdjustmentDayDto? IdOdcAdjustmentDayNavigation { get; set; } = null!;
}
