using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class ContingentDto : BaseDto<ContingentDto, Contingent>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long IdLoadingPoint { get; set; }

    public long? IdDebitor { get; set; }

    public bool Active { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public long IdSilo1 { get; set; }

    public decimal SiloPart1 { get; set; }

    public long? IdSilo2 { get; set; }

    public decimal? SiloPart2 { get; set; }

    public long? IdSilo3 { get; set; }

    public decimal? SiloPart3 { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdConfigPlantMaterial { get; set; }

    public virtual ConfigPlantMaterialDto? IdConfigPlantMaterialNavigation { get; set; } = null!;

    public virtual DebitorDto? IdDebitorNavigation { get; set; }

    public virtual LoadingPointDto? IdLoadingPointNavigation { get; set; } = null!;

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;    

    public virtual SiloDto? IdSilo1Navigation { get; set; } = null!;

    public virtual SiloDto? IdSilo2Navigation { get; set; }

    public virtual SiloDto? IdSilo3Navigation { get; set; }
}
