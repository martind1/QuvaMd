using Quva.Database.Models;
using Quva.Model.Dtos.Configuration.Plant;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class LocationParameterDto : BaseDto<LocationParameterDto, LocationParameter>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public string Value { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdOptionKey { get; set; }

    public long? IdPlant { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual TypeApplicationOptionKeyDto? IdOptionKeyNavigation { get; set; } = null!;

    public virtual PlantDto? IdPlantNavigation { get; set; }
}
