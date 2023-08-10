using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement;

public partial class UnitDto :  BaseDto<UnitDto,Unit>
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public string Display { get; set; } = null!;

    public virtual ICollection<BatchDto>? Batch { get; set; }

    public virtual ICollection<DeliveryOrderPositionDto>? DeliveryOrderPosition { get; set; }

    public virtual ICollection<MaterialDto>? Material { get; set; }

    public virtual ICollection<OrderPositionDto>? OrderPosition { get; set; }

    public virtual ICollection<QuantityUnitDto>? QuantityUnit { get; set; }
}
