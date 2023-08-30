using Quva.Database.Models;

namespace Quva.Services.Loading;

public record SiloSet
{
    // Verwaltung einer Silokombination: Silos mit Anteilen

    public List<SiloItem> SiloItems { get; set; } = new List<SiloItem>();

    public int Priority { get; set; }  //Berechnung nach werksbezogenen Regeln (zB 99 wenn Silostand niedrig)

    public LoadingPoint? TheLoadingPoint { get; set; }
    public BasicType? TheBasicType { get; set; }  // also MIX
    public int MixIndex { get; set; } = 0;

    public Contingent? TheContingent { get; set; }
    public int ContingentSiloset { get; set; }  //0,1,2

    public bool LockRail { get => SiloItems.Where(si => si.TheSilo!.LockRail == true).FirstOrDefault() != null; }
    public bool LockTruck { get => SiloItems.Where(si => si.TheSilo!.LockTruck == true).FirstOrDefault() != null; }
    public bool LockLaboratory { get => SiloItems.Where(si => si.TheSilo!.LockLaboratory == true).FirstOrDefault() != null; }
    public bool LockForSensitiveCustomer { get => SiloItems.Where(si => si.TheSilo!.LockForSensitiveCustomer == true).FirstOrDefault() != null; }

    public void SortSiloNumber()
    {
        if (SiloItems.Count >= 2)
        {
            SiloItems = SiloItems.OrderBy(si => si.TheSilo?.SiloNumber).ToList();
            int pos = 1;
            foreach (var item in SiloItems)
            {
                item.Position = pos++;
            }
        }
    }

    public void ComputePriotity()
    {
        Priority = 0;
        if (SiloItems == null || SiloItems.Count == 0) return;
        // Standard: Niedrigste Prio (höchster Wert)
        Priority = SiloItems.Select(x => x.TheSilo!.Priority).Max();
        // TODO: Option: Wenn Silostand niedrig dann 99
        // TODO: Option: Wenn Mix dann +10
    }

    public virtual bool Equals(SiloSet? other)
    {
        if (other == null) return false;

        bool b1 = TheLoadingPoint?.LoadingNumber != null &&
                  other.TheLoadingPoint?.LoadingNumber != null &&
                  TheLoadingPoint?.LoadingNumber == other.TheLoadingPoint?.LoadingNumber;
        if (!b1) return false;

        bool b2 = TheBasicType?.IdMaterialNavigation.Code != null &&
                  other.TheBasicType?.IdMaterialNavigation.Code != null &&
                  TheBasicType?.IdMaterialNavigation.Code ==
                    other.TheBasicType?.IdMaterialNavigation.Code;
        if (!b2) return false;

        if (SiloItems.Count != other.SiloItems.Count) return false;

        int equalCount = 0;
        foreach (SiloItem silo in SiloItems)
        {
            foreach (SiloItem otherSilo in other.SiloItems)
            {
                if (silo.Equals(otherSilo))
                {
                    equalCount++;
                }
            }
        }
        if (equalCount != SiloItems.Count) return false;

        return true;
    }

    public override int GetHashCode() => TheLoadingPoint!.GetHashCode() + SiloItems.GetHashCode();
}


