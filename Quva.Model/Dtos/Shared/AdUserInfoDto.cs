
using Quva.Model.Dtos.RootManagement.Plants;

namespace Quva.Model.Dtos.Shared
{
    public class AdUserInfoDto
    {
        public IEnumerable<string>? Roles { get; set; }
        public string? DisplayName { get; set; }
        public string? SamAccountName { get; set; }
        public string? Email { get; set; }

        public IList<LocationDto>? locations { get; set; }        
    }
}
