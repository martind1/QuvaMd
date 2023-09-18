using Quva.Database.Models;
using Quva.Services.Loading.Helper;

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
            //AddError(bts, $"Contingent Material no True Basic Type; MaterialId:{idMaterial}");
            bts.AddError(TrCode.LoadingService.NoTrueBasicType, idMaterial);
            return null;
        }
        return basicTypes.FirstOrDefault();
    }

    private void AddContingent(BasetypeSilos bts, Contingent contingent, BasicType basicType)
    {
        int maxSiloset = contingent.ContingentSilo.Select(x => x.SiloSet).Max();
        for (int iSet = 0; iSet <= maxSiloset; iSet++)
        {
            var contingentSilos = contingent.ContingentSilo.Where(x => x.SiloSet == iSet).ToList();
            if (contingentSilos.Count <= 0)
            {
                //AddError(bts, $"Contingent Siloset keine Silos; Set:{iSet} Id:{contingent.Id}");
                bts.AddError(TrCode.LoadingService.NoContingentSilos, iSet, contingent.Id);
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
            foreach (var contingentSilo in contingentSilos)
            {
                SiloItem siloItem = new()
                {
                    Position = contingentSilo.Position,
                    TheContingentSilo = contingentSilo,
                    TheSilo = contingentSilo.IdSiloNavigation,
                    Percentage = contingentSilo.Percentage ?? 0,
                    PowerTh = contingentSilo.PowerTh ?? 0,
                };
                debugList.Add(new ErrorLine(contingentSilo.IdSiloNavigation.SiloNumber.ToString() + " " + siloItem.Percentage + '%'));
                siloSet.SiloItems.Add(siloItem);
            }
            var sl = DebugStringList(Enums.LanguageEnum.EN);
            if (!bts.AddSiloSet(siloSet))  // mit Checks, Priority
                _log.Warning("Cont: " + string.Join(", ", sl) + ": Siloset bereits vorhanden");
            else
                _log.Information("Cont: " + string.Join(", ", sl));

        }
    }


}
