using Quva.Database.Models;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.Configuration.Plant;

public partial class SealDto : BaseDto<SealDto, Seal>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public string? Extension { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ConfigSealDto>? ConfigSeal { get; set; }
}
