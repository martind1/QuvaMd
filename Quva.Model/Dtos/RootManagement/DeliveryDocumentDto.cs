using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class DeliveryDocumentDto : BaseDto<DeliveryDocumentDto, DeliveryDocument>
{
    public long Id { get; set; }

    public long IdDeliveryHead { get; set; }

    public int DocuType { get; set; }

    public byte[]? Content { get; set; }

    public string? Extension { get; set; }

    public DateTime? Timestamp { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long? IdDeliveryMessage { get; set; }

    public string? DocuCode { get; set; }

    public virtual DeliveryHeadDto? IdDeliveryHeadNavigation { get; set; } = null!;

    public virtual DeliveryMessageDto? IdDeliveryMessageNavigation { get; set; }
}
