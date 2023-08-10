using Quva.Database.Models;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement;

public partial class UsersDto : BaseDto<UsersDto, Users>
{
    public long Id { get; set; }

    public string AdAccount { get; set; } = null!;

    public DateTime? LastLoginDate { get; set; }

    public string Name { get; set; } = null!;

    public bool Active { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<MappingUserWorkplaceDto>? MappingUserWorkplace { get; set; }
}
