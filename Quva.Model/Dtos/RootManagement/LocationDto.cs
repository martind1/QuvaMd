using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
namespace Quva.Model.Dtos.RootManagement.Plants;

public partial class LocationDto : BaseDto<LocationDto, Location>
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Shortname { get; set; } = null!;

    public string Name1 { get; set; } = null!;

    public string Name2 { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string Housenumber { get; set; } = null!;

    public string Postcode { get; set; } = null!;

    public string Location1 { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<DeviceDto>? Device { get; set; }

}
