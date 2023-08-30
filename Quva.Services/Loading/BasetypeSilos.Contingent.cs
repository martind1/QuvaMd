using Quva.Database.Models;

namespace Quva.Services.Loading;

public partial class BasetypeSilos
{
    private async Task AddSpecificContingents()
    {
        var basicType = await CheckBasicType();
        if (basicType == null) return;
        List<Contingent> contingents = await LoadingDbService.GetActiveContingents(Btsc,
            filter.idDebitor ?? 0, basicType.IdMaterial, DateTime.Now.Date);
        foreach (var contingent in contingents)
        {
            AddContingent(contingent, basicType);
        }
    }

    private async Task AddGeneralContingents()
    {
        var basicType = await CheckBasicType();
        if (basicType == null) return;
        List<Contingent> contingents = await LoadingDbService.GetActiveContingents(Btsc,
            null, basicType.IdMaterial, DateTime.Now.Date);
        foreach (var contingent in contingents)
        {
            AddContingent(contingent, basicType);
        }
    }

    private async Task<BasicType?> CheckBasicType()
    {
        long idMaterial = filter.idMaterial ?? 0;
        // only True Basictype (Mix=0):
        var basicTypes = await LoadingDbService.GetBasicTypesByMaterialId(Btsc, idMaterial, true);
        if (basicTypes.Count == 0)
        {
            AddError($"Contingent Material no True Basic Type; MaterialId:{idMaterial}");
            return null;
        }
        return basicTypes.FirstOrDefault();
    }

    private void AddContingent(Contingent contingent, BasicType basicType)
    {
        int maxSiloset = contingent.ContingentSilo.Select(x => x.SiloSet).Max();
        for (int iSet = 0; iSet <= maxSiloset; iSet++)
        {
            var silos = contingent.ContingentSilo.Where(x => x.SiloSet == iSet).ToList();
            if (silos.Count <= 0)
            {
                AddError($"Contingent Siloset keine Silos; Set:{iSet} Id:{contingent.Id}");
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
            if (!AddSiloSet(siloSet))  // mit Checks, Priority
                Btsc.log.Warning("Cont: " + string.Join(", ", debugList) + ": Siloset bereits vorhanden");
            else
                Btsc.log.Information("Cont: " + string.Join(", ", debugList));

        }
    }
}
