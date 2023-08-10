using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class MappingSiloLoadingPointDto : BaseDto<MappingSiloLoadingPointDto, MappingSiloLoadingPoint>
{
    public long Id { get; set; }

    public long IdSilo { get; set; }

    public long IdLoadingPoint { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual LoadingPointDto? IdLoadingPointNavigation { get; set; } = null!;

    public virtual SiloDto? IdSiloNavigation { get; set; } = null!;
}
