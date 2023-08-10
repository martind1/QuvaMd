using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement;

public partial class DeliveryOrderPositionClassDto : BaseDto<DeliveryOrderPositionClassDto, DeliveryOrderPositionClass>
{
    public long Id { get; set; }

    public long IdDeliveryOrderPosition { get; set; }

    public string? Classe { get; set; }

    public int? Position { get; set; }

    public string? Value { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual DeliveryOrderPositionDto? IdDeliveryOrderPositionNavigation { get; set; } = null!;
}
