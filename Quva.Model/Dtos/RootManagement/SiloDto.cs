using Quva.Database.Models;
using Quva.Model.Dtos.Configuration.Plant;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class SiloDto : BaseDto<SiloDto, Silo>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int SiloNumber { get; set; }

    public string? Akz { get; set; }

    public long IdLocation { get; set; }

    public long? IdBasicType { get; set; }

    public string? SpsCode { get; set; }

    public decimal? MaxPercentage { get; set; }

    public string? SpsBasicType { get; set; }

    public bool LockRail { get; set; }

    public bool LockTruck { get; set; }

    public bool LockDrying { get; set; }

    public bool LockBigbag { get; set; }

    public int Priority { get; set; }

    public decimal? SiloVolume { get; set; }

    public decimal? SiloLevelVolume { get; set; }

    public decimal? SiloLevelPercentage { get; set; }

    public decimal? MinSiloLevelVolume { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ContingentDto>? ContingentIdSilo1Navigation { get; set; }

    public virtual ICollection<ContingentDto>? ContingentIdSilo2Navigation { get; set; }

    public virtual ICollection<ContingentDto>? ContingentIdSilo3Navigation { get; set; }

    public virtual BasicTypeDto? IdBasicTypeNavigation { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; }

    public virtual ICollection<MappingSiloLoadingPointDto>? MappingSiloLoadingPoint { get; set; }
}
