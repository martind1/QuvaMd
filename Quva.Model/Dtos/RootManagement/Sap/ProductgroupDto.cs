using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class ProductgroupDto : BaseDto<ProductgroupDto, Productgroup>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ConfigPlantMaterialDto>? ConfigPlantMaterial { get; set; }

    public virtual ICollection<ConfigProductgroupDto>? ConfigProductgroup { get; set; }

    public virtual ICollection<DeliveryOrderPositionDto>? DeliveryOrderPosition { get; set; }

    public virtual ICollection<OrderPositionDto>? OrderPosition { get; set; }
}
