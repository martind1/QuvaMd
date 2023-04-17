using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class DeviceParameterDto : BaseDto<DeviceParameterDto, DeviceParameter>
{
    public long Id { get; set; }

    public long IdDevice { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public string? Description { get; set; }

    public virtual DeviceDto? IdDeviceNavigation { get; set; } = null!;
}