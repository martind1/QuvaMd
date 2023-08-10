using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class OdcAdjustmentDayDto : BaseDto<OdcAdjustmentDayDto, OdcAdjustmentDay>
{
    public long Id { get; set; }

    public long IdPlant { get; set; }

    public DateTime LoadingDay { get; set; }

    public int State { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual PlantDto? IdPlantNavigation { get; set; } = null!;

    public virtual ICollection<OdcAdjustmentQuantityDto>? OdcAdjustmentQuantity { get; set; }
}
