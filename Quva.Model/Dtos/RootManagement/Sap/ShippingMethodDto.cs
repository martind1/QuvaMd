using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class ShippingMethodDto : BaseDto<ShippingMethodDto, ShippingMethod>
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int PackagingType { get; set; }

    public int TransportType { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<DeliveryOrderDto>? DeliveryOrder { get; set; }

    public virtual ICollection<OrderHeadDto>? OrderHead { get; set; }
}
