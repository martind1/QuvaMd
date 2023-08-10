using Quva.Database.Models;
using Quva.Model.Dtos.Configuration.Plant;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement.Sap;

public partial class MaterialDto : BaseDto<MaterialDto, Material>
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? TradeName { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdUnit { get; set; }

    public string Language { get; set; } = null!;

    public bool? PrintMark { get; set; }

    public string MaterialKind { get; set; } = null!;

    public string GroupKey { get; set; } = null!;

    public virtual ICollection<BasicTypeDto>? BasicType { get; set; }

    public virtual ICollection<ConfigPlantMaterialDto>? ConfigPlantMaterial { get; set; }    

    public virtual ICollection<CustomerAgreementDto>? CustomerAgreements { get; set; }

    public virtual UnitDto? IdUnitNavigation { get; set; } = null!;

    public virtual ICollection<QuantityUnitDto>? QuantityUnit { get; set; }
}
