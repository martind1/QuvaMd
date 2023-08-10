using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class ConfigProductgroupDto : BaseDto<ConfigProductgroupDto, ConfigProductgroup>
{
    public long Id { get; set; }

    public long IdPlant { get; set; }

    public long IdProductgroup { get; set; }

    public string GroupKey { get; set; } = null!;

    public string MaterialKind { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual PlantDto? IdPlantNavigation { get; set; } = null!;

    public virtual ProductgroupDto? IdProductgroupNavigation { get; set; } = null!;
}
