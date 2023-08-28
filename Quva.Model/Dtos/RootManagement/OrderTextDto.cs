using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class OrderTextDto : BaseDto<OrderTextDto, OrderText>
{
    public long Id { get; set; }

    public long IdOrder { get; set; }

    public long IdOrderTextTypes { get; set; }

    public string? Content { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual OrderHeadDto? IdOrderNavigation { get; set; } = null!;

    public virtual TypeOrderTextDto? IdOrderTextTypesNavigation { get; set; } = null!;
}