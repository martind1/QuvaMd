using Quva.Database.Models;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.Reporting
{
    public class VDeliverypositionDto : BaseDto<VDeliverypositionDto,VDeliveryposition>
    {
        public long DeliveryNumber { get; set; }

        public int? PositionNumber { get; set; }

        public decimal LoadedQuantity { get; set; }

        public string? MaterialShortName { get; set; }

        public string? MaterialLongName { get; set; }

        public string? CustomerProductName { get; set; }

        public string? Unit { get; set; }
    }
}
