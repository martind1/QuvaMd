using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class LoadingPointDto : BaseDto<LoadingPointDto, LoadingPoint>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public int LoadingNumber { get; set; }

    public string Name { get; set; } = null!;

    public int PackagingType { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public int TransportType { get; set; }

    public virtual ICollection<ContingentDto>? Contingent { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<MappingSiloLoadingPointDto>? MappingSiloLoadingPoint { get; set; }
}
