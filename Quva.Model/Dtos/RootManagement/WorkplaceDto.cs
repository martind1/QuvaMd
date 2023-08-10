using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement;

public partial class WorkplaceDto : BaseDto<WorkplaceDto, Workplace>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;    

    public long IdLocation { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<MappingUserWorkplaceDto>? MappingUserWorkplace { get; set; }

    public virtual ICollection<MappingWorkplaceDeviceDto>? MappingWorkplaceDevice { get; set; }

    public virtual ICollection<PrinterDto>? Printer { get; set; }

    public virtual ICollection<WorkplaceParameterDto>? WorkplaceParameter { get; set; }
}
