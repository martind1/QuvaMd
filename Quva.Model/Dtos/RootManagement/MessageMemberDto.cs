using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class MessageMemberDto : BaseDto<MessageMemberDto, MessageMember>
{
    public long Id { get; set; }

    public long IdMessageDispatcher { get; set; }

    public int MemberType { get; set; }

    public string Adress { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual MessageDispatcherDto? IdMessageDispatcherNavigation { get; set; } = null!;
}
