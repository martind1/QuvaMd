using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement
{
    public class ConfigPlantSalesOrgDto : BaseDto<ConfigPlantSalesOrgDto,ConfigPlantSalesOrg>
    {
        public long Id { get; set; }

        public long IdPlant { get; set; }

        public long IdLogo { get; set; }

        public string ReferenzDeliveryForm { get; set; } = null!;

        public string ReferenzLoadingForm { get; set; } = null!;

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public long? IdSalesOrganization { get; set; }

        public virtual LogoDto? IdLogoNavigation { get; set; } = null!;

        public virtual PlantDto? IdPlantNavigation { get; set; } = null!;

        public virtual SalesOrganizationDto? IdSalesOrganizationNavigation { get; set; }
    }
}
