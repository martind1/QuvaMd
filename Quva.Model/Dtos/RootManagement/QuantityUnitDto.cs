using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement.Sap;
using Quva.Model.Dtos.Shared;

namespace Quva.Model.Dtos.RootManagement;

public partial class QuantityUnitDto : BaseDto<QuantityUnitDto, QuantityUnit>
{
    public long Id { get; set; }

    public long IdAlternativeUnit { get; set; }

    public long IdMaterial { get; set; }

    public long EanNumber { get; set; }

    public int FactorBasicUnit { get; set; }

    public int FactorAlternativeUnit { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual UnitDto? IdAlternativeUnitNavigation { get; set; } = null!;

    public virtual MaterialDto? IdMaterialNavigation { get; set; } = null!;
}
