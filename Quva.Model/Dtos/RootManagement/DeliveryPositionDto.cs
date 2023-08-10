using Quva.Database.Models;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement
{
    public class DeliveryPositionDto : BaseDto<DeliveryPositionDto, DeliveryPosition>
    {
        public long Id { get; set; }

        public long IdDeliveryHead { get; set; }

        public bool Active { get; set; }

        public decimal LoadedQuantity { get; set; }

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public int PositionNumber { get; set; }

        public virtual DeliveryOrderPositionDto? DeliveryOrderPosition { get; set; }

        public virtual DeliveryHeadDto? IdDeliveryHeadNavigation { get; set; } = null!;
    }
}
