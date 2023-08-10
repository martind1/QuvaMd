using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement;
using Quva.Model.Dtos.RootManagement.Plants;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.Configuration.Plant;

public partial class BasicTypeDto : BaseDto<BasicTypeDto, BasicType>
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long IdMaterial { get; set; }

    public string? SpsCode { get; set; }

    public long? IdBasicType1 { get; set; }

    public int? IdBasicType1Percentage { get; set; }

    public long? IdBasicType2 { get; set; }

    public int? IdBasicType2Percentage { get; set; }

    public long? IdBasicType3 { get; set; }

    public int? IdBasicType3Percentage { get; set; }

    public long? IdBasicType4 { get; set; }

    public int? IdBasicType4Percentage { get; set; }

    public long? IdBasicType5 { get; set; }

    public int? IdBasicType5Percentage { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual BasicTypeDto? IdBasicType1Navigation { get; set; }

    public virtual BasicTypeDto? IdBasicType2Navigation { get; set; }

    public virtual BasicTypeDto? IdBasicType3Navigation { get; set; }

    public virtual BasicTypeDto? IdBasicType4Navigation { get; set; }

    public virtual BasicTypeDto? IdBasicType5Navigation { get; set; }

    public virtual LocationDto? IdLocationNavigation { get; set; } = null!;

    public virtual MaterialDto? IdMaterialNavigation { get; set; } = null!;

    public virtual ICollection<BasicTypeDto>? InverseIdBasicType1Navigation { get; set; }

    public virtual ICollection<BasicTypeDto>? InverseIdBasicType2Navigation { get; set; }

    public virtual ICollection<BasicTypeDto>? InverseIdBasicType3Navigation { get; set; }

    public virtual ICollection<BasicTypeDto>? InverseIdBasicType4Navigation { get; set; }

    public virtual ICollection<BasicTypeDto>? InverseIdBasicType5Navigation { get; set; }

    public virtual ICollection<SiloDto>? Silo { get; set; }
}
