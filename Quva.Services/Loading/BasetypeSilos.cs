using Quva.Services.Enums;
using Quva.Services.Loading.Helper;
using Serilog;

namespace Quva.Services.Loading;

// Verwaltung der möglichen Silokombinationen. Analog View V_GRSO_SILOS
public class BasetypeSilos
{
    private readonly ILogger _log;
    public List<SiloSet> SiloSets { get; set; } = new();
    public List<ErrorLine> ErrorLines { get; set; } = new();
    public BaseTypeSiloFilter filter = new();

    public BasetypeSilos()
    {
        _log = Log.ForContext(GetType());
    }

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

    public void AddError(string code, params object[] parameter)
    {
        var line = new ErrorLine(code, parameter);
        _log.Warning(line.ToString(LanguageEnum.EN));
        ErrorLines.Add(line);
    }


    public List<string> ErrorStringList(LanguageEnum language)
    {
        List<string> result = new();
        foreach (var err in ErrorLines)
        {
            result.Add(err.ToString(language));
        }
        return result;
    }

    // entfernt Silosets die nicht der Regel entsprechen:
    public void ApplyRule(RuleToApply rule)
    {
        List<SiloSet> NewList = new();
        foreach (var siloset in SiloSets)
        {
            if (siloset.ApplyRule(rule, AddError))
            {
                NewList.Add(siloset);
            }
            else
            {
                _log.Information($"Siloset deleted: {siloset.SiloNumbers}");
            }
        }
        if (NewList.Count < SiloSets.Count)
        {
            SiloSets = NewList;
        }
    }

    // alle Regeln aufstellen und anwenden
    public void ApplyRules(RuleToApply ruleData)
    {
        RuleToApply rule;
        rule = ruleData with { CheckSiloLock = true, 
                               CheckLaboratory = true, 
                               CheckProduction = true,
                               CheckSilolevel = true };
        ApplyRule(rule);

        if (ruleData.SensitiveCustomer)
        {
            rule = ruleData with { CheckForSensitiveCustomer = true };
            ApplyRule(rule);
        }

    }

}

