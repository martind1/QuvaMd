using Quva.Database.Models;

namespace Quva.Services.Loading;

public delegate void OnAddError(string code, params object[] parameter);

public record SiloSet
{
    // Verwaltung einer Silokombination: Silos mit Anteilen

    public List<SiloItem> SiloItems { get; set; } = new List<SiloItem>();

    public int Priority { get; set; }  //Berechnung nach werksbezogenen Regeln (zB 99 wenn Silostand niedrig)

    public LoadingPoint? TheLoadingPoint { get; set; }
    public BasicType? TheBasicType { get; set; }  // MIX or TrueType
    public int MixIndex { get; set; } = 0;

    public Contingent? TheContingent { get; set; }
    public int ContingentSiloset { get; set; }  //0,1,2

    public bool LockRail { get => SiloItems.Where(si => si.TheSilo!.LockRail == true).FirstOrDefault() != null; }
    public bool LockTruck { get => SiloItems.Where(si => si.TheSilo!.LockTruck == true).FirstOrDefault() != null; }
    public bool LockTruck2 { get => SiloItems.Where(si => si.TheSilo!.LockTruck2 == true).FirstOrDefault() != null; }
    public bool LockBigbag { get => SiloItems.Where(si => si.TheSilo!.LockBigbag == true).FirstOrDefault() != null; }
    public bool LockLaboratory { get => SiloItems.Where(si => si.TheSilo!.LockLaboratory == true).FirstOrDefault() != null; }
    public bool LockProduction { get => SiloItems.Where(si => si.TheSilo!.LockForProduction == true).FirstOrDefault() != null; }
    public bool LockForSensitiveCustomer { get => SiloItems.Where(si => si.TheSilo!.LockForSensitiveCustomer == true).FirstOrDefault() != null; }

    public string SiloNumbers { get => string.Join(", ", SiloItems.Select(si => si.TheSilo!.SiloNumber).ToList()); }

    
    // ergibt false wenn nicht der Regel entsprochen:
    public bool ApplyRule(RuleToApply rule, OnAddError addError)
    {
        bool result = true;

        if (rule.CheckSiloLock)
        {
            if (TheContingent != null && !TheContingent.CheckSilolock)
            {
                // no CheckSilolock
            }
            else if (rule.LockRole != Enums.LockRoleValues.None)
            {
                if (rule.LockRole == Enums.LockRoleValues.BigBag && LockBigbag)
                {
                    addError(TrCode.LoadingService.LockBigbag, SiloNumbers);
                    result = false;
                }
                if (rule.LockRole == Enums.LockRoleValues.Truck2 && LockTruck2)
                {
                    addError(TrCode.LoadingService.LockTruck2, SiloNumbers);
                    result = false;
                }
            }
            else
            {
                if (rule.TransportType == Enums.TransportTypeValues.Truck && LockTruck)
                {
                    addError(TrCode.LoadingService.LockTruck, SiloNumbers);
                    return false;
                }
                if (rule.TransportType == Enums.TransportTypeValues.Rail && LockRail)
                {
                    addError(TrCode.LoadingService.LockRail, SiloNumbers);
                    return false;
                }
            }
        }
        if (rule.CheckLaboratory && LockLaboratory)
        {
            if (TheContingent == null || !TheContingent.CheckSilolock)
            {
                addError(TrCode.LoadingService.LockLaboratory, SiloNumbers);
                return false;
            }
        }
        if (rule.CheckProduction && LockProduction)
        {
            if (TheContingent == null || !TheContingent.CheckSilolock)
            {
                addError(TrCode.LoadingService.LockProduction, SiloNumbers);
                return false;
            }
        }
        if (rule.CheckForSensitiveCustomer && LockForSensitiveCustomer)
        {
            if (TheContingent == null || !TheContingent.CheckSilolock)
            {
                addError(TrCode.LoadingService.LockForSensitiveCustomer, SiloNumbers);
                return false;
            }
        }

        if (rule.CheckSilolevel && SiloItems.Count > 1)
        {
            if (TheContingent == null || TheContingent.CheckSilolevel)
            {
                // 1 = SiloLevel - LoadQuantity > MinFuellung
                foreach (var silo in SiloItems)
                {
                    var siloQuantity = rule.LoadingQuantity * silo.Percentage / 100;
                    if (silo.TheSilo!.SiloLevelVolume - siloQuantity <= silo.TheSilo.MinSiloLevelVolume)
                    {
                        addError(TrCode.LoadingService.CheckSilolevel, silo.TheSilo.SiloNumber);
                        return false;
                    }
                }
            }
        }

        return result;
    }

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
        
        // Option: Wenn Mix dann +10
        if (SiloItems.Count > 1)
        {
            Priority += 10;
        }

        // TODO: Option: Wenn Silostand niedrig dann 99
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


