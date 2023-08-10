using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class MappingWorkplaceDeviceDto : BaseDto<MappingWorkplaceDeviceDto, MappingWorkplaceDevice>
{
    public long Id { get; set; }

    public long IdWorkplace { get; set; }

    public long IdDevice { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual DeviceDto? IdDeviceNavigation { get; set; } = null!;

    public virtual WorkplaceDto? IdWorkplaceNavigation { get; set; } = null!;
}
