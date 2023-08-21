using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class LogoDto : BaseDto<LogoDto, Logo>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Extension { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ConfigPlantSalesOrg>? ConfigPlantSalesOrg { get; set; }
}
