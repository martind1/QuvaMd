using Quva.Database.Models;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement.Plants;

public partial class CarrierDto : BaseDto<CarrierDto, Carrier>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }
    public long IdLocation { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<VehicleDto>? Vehicle { get; set; }
}
