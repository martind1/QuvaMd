using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Loading;

public class BasetypeSilosView
{
    public List<BasetypeSilosItem> SiloSets { get; set; } = new();

    public static BasetypeSilosView FromBasetypeSilos(BasetypeSilos basetypeSilos)
    {
        BasetypeSilosView result = new();
        foreach (var siloset in basetypeSilos.SiloSets)
        {
            BasetypeSilosItem item = new()
            {
                BaseTypeCode = siloset.TheBasicType!.IdMaterialNavigation.Code,
                MaterialName = siloset.TheBasicType!.IdMaterialNavigation.Name,
                MixIndex = siloset.TheBasicType.MixIndex,
                Contingent = siloset.TheContingent != null ? siloset.ContingentSiloset : null,
                LoadingPoint = siloset.TheLoadingPoint!.LoadingNumber,
                Priority = siloset.Priority,
                LockRail = siloset.LockRail,
                LockTruck = siloset.LockTruck,
                LockLaboratory = siloset.LockLaboratory,
                LockForSensitiveCustomer = siloset.LockForSensitiveCustomer,
            };
            foreach (var siloItem in siloset.SiloItems)
            {
                switch (siloItem.Position)
                {
                    case 1:
                        item.SiloNumber1 = siloItem.TheSilo!.SiloNumber;
                        item.Percentage1 = siloItem.Percentage;
                        break;
                    case 2:
                        item.SiloNumber2 = siloItem.TheSilo!.SiloNumber;
                        item.Percentage2 = siloItem.Percentage;
                        break;
                    case 3:
                        item.SiloNumber3 = siloItem.TheSilo!.SiloNumber;
                        item.Percentage3 = siloItem.Percentage;
                        break;
                }
            }
            result.SiloSets.Add(item);
        }
        return result;
    }
}

public record BasetypeSilosItem
{
    // record wg ToString()
    public string BaseTypeCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public int MixIndex { get; set; }
    public int? Contingent { get; set; }
    public int LoadingPoint { get; set; } = 0;
    public int Priority { get; set; } = 0;
    public bool LockRail { get; set; } = false;
    public bool LockTruck { get; set; } = false;
    public bool LockLaboratory { get; set; } = false;
    public bool LockForSensitiveCustomer { get; set; } = false;
    public int? SiloNumber1 { get; set; }
    public int? SiloNumber2 { get; set; }
    public int? SiloNumber3 { get; set; }
    public decimal? Percentage1 { get; set; }
    public decimal? Percentage2 { get; set; }
    public decimal? Percentage3 { get; set; }
}
