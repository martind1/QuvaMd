using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class ConfigMessagesDocumentDto : BaseDto<ConfigMessagesDocumentDto, ConfigMessagesDocument>
{
    public long Id { get; set; }

    public long IdConfigMessages { get; set; }

    public int DokumentType { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ConfigMessageDto? IdConfigMessagesNavigation { get; set; } = null!;
}
