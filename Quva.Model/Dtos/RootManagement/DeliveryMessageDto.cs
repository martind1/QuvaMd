using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class DeliveryMessageDto : BaseDto<DeliveryMessageDto, DeliveryMessage>
{
    public long Id { get; set; }

    public long IdDeliveryHead { get; set; }

    public long IdConfigMessage { get; set; }

    public int MessageType { get; set; }

    public string? Subject { get; set; }

    public int State { get; set; }

    public DateTime? ToSendDate { get; set; }

    public DateTime? SendDate { get; set; }

    public string? SendError { get; set; }

    public string? SenderEmail { get; set; }

    public string? Recipient { get; set; }

    public string? Content { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<DeliveryDocumentDto>? DeliveryDocument { get; set; }

    public virtual ConfigMessageDto? IdConfigMessageNavigation { get; set; } = null!;

    public virtual DeliveryHeadDto? IdDeliveryHeadNavigation { get; set; } = null!;
}
