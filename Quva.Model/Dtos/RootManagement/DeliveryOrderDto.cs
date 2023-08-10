using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class DeliveryOrderDto : BaseDto<DeliveryOrderDto, DeliveryOrder>
{
    public long Id { get; set; }

    public string? UnloadingPoint { get; set; }

    public bool ChangedOrder { get; set; }

    public int DeliveryNotePrint { get; set; }

    public string Art { get; set; } = null!;

    public long OrderNumber { get; set; }

    public string? FieldService { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? CustomerOrderNumber { get; set; }

    public DateTime? DesiredDeliveryDate { get; set; }

    public bool DeliveryLock { get; set; }

    public string? DeliveryCondition { get; set; }

    public string? DeliveryInfo { get; set; }

    public bool NonSapOrder { get; set; }

    public bool NewOrder { get; set; }

    public string? Clerk { get; set; }

    public string? PrintLanguage { get; set; }

    public bool Active { get; set; }

    public long IdShippingMethod { get; set; }

    public long IdPlant { get; set; }

    public int? RetrievalCode { get; set; }

    public bool Locked { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdSalesOrganization { get; set; }

    public long IdDeliveryHead { get; set; }

    public virtual ICollection<DeliveryOrderDebitorDto>? DeliveryOrderDebitor { get; set; }

    public virtual ICollection<DeliveryOrderPositionDto>? DeliveryOrderPosition { get; set; }

    public virtual ICollection<DeliveryOrderTextDto>? DeliveryOrderText { get; set; }

    public virtual DeliveryHeadDto? IdDeliveryHeadNavigation { get; set; }

    public virtual PlantDto? IdPlantNavigation { get; set; } = null!;

    public virtual SalesOrganizationDto? IdSalesOrganizationNavigation { get; set; } = null!;

    public virtual ShippingMethodDto? IdShippingMethodNavigation { get; set; } = null!;
}
