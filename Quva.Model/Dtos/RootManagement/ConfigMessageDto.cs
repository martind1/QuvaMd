using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class ConfigMessageDto : BaseDto<ConfigMessageDto, ConfigMessage>
{
    public long Id { get; set; }

    public long? IdMessageDispatcher { get; set; }

    public long IdLocation { get; set; }

    public string Name { get; set; } = null!;

    public int Cause { get; set; }

    public int DelayMinutes { get; set; }

    public string? Subject { get; set; }

    public string? Content { get; set; }

    public string? Condition { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<ConfigMessagesDocumentDto>? ConfigMessagesDocument { get; set; }

    public virtual ICollection<DeliveryMessageDto>? DeliveryMessage { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual MessageDispatcherDto? IdMessageDispatcherNavigation { get; set; }
}
