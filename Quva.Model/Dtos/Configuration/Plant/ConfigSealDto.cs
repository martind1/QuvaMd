using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.Configuration.Plant;

public partial class ConfigSealDto : BaseDto<ConfigSealDto, ConfigSeal>
{
    public long Id { get; set; }

    public long IdPlant { get; set; }

    public long? IdSeal { get; set; }

    public string FilterField { get; set; } = null!;

    public string FilterExpression { get; set; } = null!;

    public int Kind { get; set; }

    public int Position { get; set; }

    public int Width { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual PlantDto? IdPlantNavigation { get; set; } = null!;

    public virtual SealDto? IdSealNavigation { get; set; } = null!;
}
