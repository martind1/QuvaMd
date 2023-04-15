using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class DeviceDto : BaseDto<DeviceDto, Device>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ModuleCode { get; set; } = null!;

    public int PackagingType { get; set; }

    public int TransportType { get; set; }

    public string? Paramstring { get; set; }

    public int DeviceType { get; set; }

    public int Porttype { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public int Roles { get; set; }

    public virtual ICollection<DeviceParameterDto>? DeviceParameter { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<MappingWorkplaceDeviceDto>? MappingWorkplaceDevice { get; set; }
}
