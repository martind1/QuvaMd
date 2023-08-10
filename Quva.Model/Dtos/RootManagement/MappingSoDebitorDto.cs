using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement
{
    public class MappingSoDebitorDto : BaseDto<MappingSoDebitorDto, MappingSoDebitor>
    {
        public long Id { get; set; }

        public long IdDebitor { get; set; }

        public long IdSalesOrganization { get; set; }

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public int? CountDeliveryPrint { get; set; }

        public virtual DebitorDto IdDebitorNavigation { get; set; } = null!;

        public virtual SalesOrganizationDto IdSalesOrganizationNavigation { get; set; } = null!;
    }
}
