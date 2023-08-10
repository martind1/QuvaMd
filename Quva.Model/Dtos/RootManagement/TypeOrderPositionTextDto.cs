using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class TypeOrderPositionTextDto : BaseDto<TypeOrderPositionTextDto, TypeOrderPositionText>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<DeliveryOrderPositionTextDto>? DeliveryOrderPositionText { get; set; }

    public virtual ICollection<OrderPositionTextDto>? OrderPositionText { get; set; }
}
