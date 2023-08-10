using Quva.Database.Models;
using Quva.Model.Dtos.Shared;


namespace Quva.Model.Dtos.RootManagement
{
    public class OrderheadviewDto : BaseDto<OrderheadviewDto, Orderheadview>
    {
        public long Id { get; set; }

        public string? UnloadingPoint { get; set; }

        public bool ChangedOrder { get; set; }

        public string Art { get; set; } = null!;

        public long OrderNumber { get; set; }

        public string? FieldService { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? CustomerOrderNumber { get; set; }

        public DateTime? DesiredDeliveryDate { get; set; }

        public bool DeliveryLock { get; set; }

        public string? DeliveryCondition { get; set; }

        public string? DeliveryInfo { get; set; }

        public bool NonSapOrder { get; set; }

        public bool NewOrder { get; set; }

        public string? Clerk { get; set; }

        public string? PrintLanguage { get; set; }

        public bool Active { get; set; }

        public long IdShippingMethod { get; set; }

        public long IdPlant { get; set; }

        public int? RetrievalCode { get; set; }

        public bool Locked { get; set; }

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public long? IdSalesOrganization { get; set; }

        public string? CodeGoodRecipient { get; set; }

        public string? Name1GoodRecipient { get; set; }

        public string? LocationGoodRecipient { get; set; }

        public string? StreetGoodRecipient { get; set; }

        public string? PostcodeGoodRecipient { get; set; }

        public string? PhoneGoodRecipient { get; set; }

        public string? CodeInvoiceRecipient { get; set; }

        public string? Name1InvoiceRecipient { get; set; }

        public string? LocationInvoiceRecipient { get; set; }

        public string? StreetInvoiceRecipient { get; set; }

        public string? PostcodeInvoiceRecipient { get; set; }

        public string? PhoneInvoiceRecipient { get; set; }

        public string? CodeForwardingAgent { get; set; }

        public string? Name1ForwardingAgent { get; set; }

        public string? LocationForwardingAgent { get; set; }

        public string? StreetForwardingAgent { get; set; }

        public string? PostcodeForwardingAgent { get; set; }

        public string? PhoneForwardingAgent { get; set; }

        public long IdLocation { get; set; }

        public int PackagingType { get; set; }

        public int TransportType { get; set; }

        public decimal? SumDeliveredQuantity { get; set; }
    }
}
