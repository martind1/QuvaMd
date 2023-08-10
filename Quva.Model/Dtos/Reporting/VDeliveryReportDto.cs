using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
using System.ComponentModel.DataAnnotations;

namespace Quva.Model.Dtos.Reporting
{
    public class VDeliveryReportDto : BaseDto<VDeliveryReportDto, VDeliveryReport>
    {
        [Key]
        public long DeliveryNumber { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string? RegistrationDate { get; set; }

        public DateTime? ClosingDate { get; set; }

        public string? InitialWeighingMode { get; set; }

        public long OrderNumber { get; set; }

        public string DebitorGrCode { get; set; } = null!;

        public string DebitorGrName1 { get; set; } = null!;

        public string MaterialShortName { get; set; } = null!;

        public string MaterialLongName { get; set; } = null!;

        public string? VehicleNumber { get; set; }

        public string? Carrier { get; set; }

        public string ShippingMethod { get; set; } = null!;

        public decimal? InitialWeighingWeight { get; set; }

        public decimal? FinalWeighingWeight { get; set; }

        public decimal LoadedQuantity { get; set; }

        public string Unit { get; set; } = null!;

        public int DeliveryState { get; set; }

        public string DebitorIrCode { get; set; } = null!;

        public string DebitorIrName1 { get; set; } = null!;

        public long IdPlant { get; set; }
    }
}
