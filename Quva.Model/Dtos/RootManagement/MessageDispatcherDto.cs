using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class MessageDispatcherDto : BaseDto<MessageDispatcherDto, MessageDispatcher>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ConfigMessageDto>? ConfigMessage { get; set; }

    public virtual ICollection<MessageMemberDto>? MessageMember { get; set; }
}
