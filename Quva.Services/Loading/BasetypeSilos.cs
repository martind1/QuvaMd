namespace Quva.Services.Loading;



/// <summary>
/// Verwaltung der möglichen Silokombinationen. Analog View V_GRSO_SILOS
/// </summary>
public class BasetypeSilos
{
    public List<SiloSet> SiloSets { get; set; } = new();
    public List<string> ErrorLines { get; set; } = new();
    public BaseTypeSiloFilter filter = new();


    public void SortByPrio()
    {
        // Sorts and deletes 0-Priorities
        SiloSets = SiloSets
            .Where(o => o.Priority > 0)
            .OrderBy(o => o.Priority)
            .ToList();
    }

    public bool AddSiloSet(SiloSet siloSet)
    {
        // 1. sort Silonumber
        // 2. check if already exists
        // 3. compute priority when mix

        siloSet.SortSiloNumber();

        foreach (var otherSiloSet in SiloSets)
        {
            if (otherSiloSet.Equals(siloSet)) return false;
        }

        siloSet.ComputePriotity();

        SiloSets.Add(siloSet);

        return true;
    }

}

