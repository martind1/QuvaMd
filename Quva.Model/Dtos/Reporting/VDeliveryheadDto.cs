using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Model.Dtos.Reporting
{
    public class VDeliveryheadDto : BaseDto<VDeliveryheadDto, VDeliveryhead>
    {
        public string? PlantShortName { get; set; }

        public string LocationName1 { get; set; } = null!;

        public string LocationName2 { get; set; } = null!;

        public string LocationPostcode { get; set; } = null!;

        public string LocationLocation { get; set; } = null!;

        public string LocationStreet { get; set; } = null!;

        public string LocationHousenumber { get; set; } = null!;

        public string LocationPhone { get; set; } = null!;

        public long Id { get; set; }

        public long? DeliveryNumber { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string? RegistrationDate { get; set; }

        public string? FinalWeighingDate { get; set; }

        public decimal? InitialWeighingWeight { get; set; }

        public string? InitialWeighingCalibNumber { get; set; }

        public int? InitialWeighingMode { get; set; }

        public string? InitialScaleCode { get; set; }

        public string? InitialWeighingUnitText { get; set; }

        public decimal? FinalWeighingWeight { get; set; }

        public string? FinalWeighingCalibNumber { get; set; }

        public int? FinalWeighingMode { get; set; }

        public string? FinalScaleCode { get; set; }

        public string? FinalWeighingUnitText { get; set; }

        public string? ContainerNumber { get; set; }

        public string? UnloadingPoint { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? CustomerOrderNumber { get; set; }

        public string? VehicleNumber { get; set; }

        public decimal? MoistPercentage { get; set; }

        public long? OrderNumber { get; set; }

        public string? GoodRecipienCode { get; set; }

        public string? GoodRecipienName1 { get; set; }

        public string? GoodRecipienName2 { get; set; }

        public string? GoodRecipienStreet { get; set; }

        public string? GoodRecipienPostcode { get; set; }

        public string? GoodRecipienLocation { get; set; }

        public string GoodRecipienCountyCode { get; set; } = null!;

        public string? InvoiceRecipienCode { get; set; }

        public string? InvoiceRecipienName1 { get; set; }

        public string? InvoiceRecipienName2 { get; set; }

        public string? InvoiceRecipienStreet { get; set; }

        public string? InvoiceRecipienPostcode { get; set; }

        public string? InvoiceRecipienLocation { get; set; }

        public string InvoiceRecipienCountyCode { get; set; } = null!;

        public string ShippingMethod { get; set; } = null!;

        public decimal? NetWeight { get; set; }

        public string? NetUnitText { get; set; }
    }
}
