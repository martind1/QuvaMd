using Quva.Database.Models;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement;

public partial class WorkplaceParameterDto : BaseDto<WorkplaceParameterDto, WorkplaceParameter>
{
    public long Id { get; set; }

    public long IdWorkplace { get; set; }

    public string Name { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string? Value { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual WorkplaceDto? IdWorkplaceNavigation { get; set; } = null!;
}
