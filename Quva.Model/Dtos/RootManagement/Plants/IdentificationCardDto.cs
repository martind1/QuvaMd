using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement.Plants;

public partial class IdentificationCardDto : BaseDto<IdentificationCardDto, IdentificationCard>
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public long IdDebitor { get; set; }

    public long IdVehicles { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public bool Locked { get; set; }

    public bool FlagDispencer { get; set; }

    public virtual DebitorDto? IdDebitorNavigation { get; set; } = null!;

    public virtual VehicleDto? IdVehiclesNavigation { get; set; } = null!;
}
