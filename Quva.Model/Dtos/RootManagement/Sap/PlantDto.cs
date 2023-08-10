using Quva.Model.Dtos.Shared;
using Quva.Database.Models;
using Quva.Model.Dtos.Configuration.Plant;
using Quva.Model.Dtos.RootManagement.Plants;

namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class PlantDto : BaseDto<PlantDto, Plant>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long DeliveryNumberMin { get; set; }

    public long DeliveryNumberMax { get; set; }

    public long DeliveryNumberCurrent { get; set; }

    public long NonSapOrderMin { get; set; }

    public long NonSapOrderMax { get; set; }

    public long NonSapOrderCurrent { get; set; }

    public virtual ICollection<ConfigPlantMaterialDto>? ConfigPlantMaterial { get; set; }

    public virtual ICollection<ConfigPlantSalesOrgDto>? ConfigPlantSalesOrg { get; set; }

    public virtual ICollection<ConfigProductgroupDto>? ConfigProductgroup { get; set; }

    public virtual ICollection<ConfigSealDto>? ConfigSeal { get; set; }

    public virtual ICollection<CustomerAgreementDto>? CustomerAgreements { get; set; }

    public virtual ICollection<DeliveryOrderDto>? DeliveryOrder { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<LocationParameterDto>? LocationParameter { get; }

    public virtual ICollection<OdcAdjustmentDayDto>? OdcAdjustmentDay { get; set; }

    public virtual ICollection<OrderHeadDto>? OrderHead { get; set; }
}
