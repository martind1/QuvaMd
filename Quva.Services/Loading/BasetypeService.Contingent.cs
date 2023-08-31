using Quva.Database.Models;

namespace Quva.Services.Loading;

public partial class BasetypeService
{
    private async Task AddSpecificContingents(BasetypeSilos bts)
    {
        var basicType = await CheckBasicType(bts);
        if (basicType == null) return;
        List<Contingent> contingents = await _loadingDbService.GetActiveContingents(bts.filter.IdLocation,
            bts.filter.idDebitor ?? 0, basicType.IdMaterial, DateTime.Now.Date);
        foreach (var contingent in contingents)
        {
            AddContingent(bts, contingent, basicType);
        }
    }

    private async Task AddGeneralContingents(BasetypeSilos bts)
    {
        var basicType = await CheckBasicType(bts);
        if (basicType == null) return;
        List<Contingent> contingents = await _loadingDbService.GetActiveContingents(bts.filter.IdLocation,
            null, basicType.IdMaterial, DateTime.Now.Date);
        foreach (var contingent in contingents)
        {
            AddContingent(bts, contingent, basicType);
        }
    }

    private async Task<BasicType?> CheckBasicType(BasetypeSilos bts)
    {
        long idMaterial = bts.filter.idMaterial ?? 0;
        // only True Basictype (Mix=0):
        var basicTypes = await _loadingDbService.GetBasicTypesByMaterialId(bts.filter.IdLocation, idMaterial, true);
        if (basicTypes.Count == 0)
        {
            AddError(bts, $"Contingent Material no True Basic Type; MaterialId:{idMaterial}");
            return null;
        }
        return basicTypes.FirstOrDefault();
    }

    private void AddContingent(BasetypeSilos bts, Contingent contingent, BasicType basicType)
    {
        int maxSiloset = contingent.ContingentSilo.Select(x => x.SiloSet).Max();
        for (int iSet = 0; iSet <= maxSiloset; iSet++)
        {
            var silos = contingent.ContingentSilo.Where(x => x.SiloSet == iSet).ToList();
            if (silos.Count <= 0)
            {
                AddError(bts, $"Contingent Siloset keine Silos; Set:{iSet} Id:{contingent.Id}");
                continue;  //produktiv

            }
            debugList.Clear();
            SiloSet siloSet = new()
            {
                TheBasicType = basicType,
                Priority = 99,  // see AddSiloSet
                TheLoadingPoint = contingent.IdLoadingPointNavigation,
                TheContingent = contingent,
                ContingentSiloset = iSet,
            };
            foreach (var silo in silos)
            {
                SiloItem siloItem = new()
                {
                    Position = silo.Position,
                    TheSilo = silo.IdSiloNavigation,
                    Percentage = silo.Percentage ?? 0,
                    PowerTh = silo.PowerTh ?? 0,
                };
                debugList.Add(silo.IdSiloNavigation.SiloNumber.ToString() + " " + siloItem.Percentage + '%');
                siloSet.SiloItems.Add(siloItem);
            }
            if (!bts.AddSiloSet(siloSet))  // mit Checks, Priority
                _log.Warning("Cont: " + string.Join(", ", debugList) + ": Siloset bereits vorhanden");
            else
                _log.Information("Cont: " + string.Join(", ", debugList));

        }
    }
}
