using Quva.Database.Models;

namespace Quva.Services.Loading;

public partial class BasetypeSilos
{
    private async Task AddTrueBasicTypes()
    {
        // BaseTypes mit Material ..
        List<BasicType> basicTypes = await LoadingDbService.GetBasicTypesByMaterialId(Btsc, filter.idMaterial, false);
        foreach (var basicType in basicTypes)
        {
            // .. plus MapingBasicType mit Other Material
            var mappedBasicTypes = await LoadingDbService.GetMappedTypesByBasicType(Btsc, basicType.Id);
            if (mappedBasicTypes.Count <= 0)  //if (basicType.MixIndex <= 0)
            {
                if (basicType.MixIndex > 0)
                {
                    AddError($"Falscher MixIndex {basicType.MixIndex}. Muss 0 sein wenn kein Mix. BasicType:{basicType.IdMaterialNavigation.Code}");
                    continue;  //produktiv
                }
                var lpSilos = await LoadingDbService.GetLoadingpointSilosByBasictype(Btsc, basicType.Id, null);
                foreach (var lpSilo in lpSilos)
                {
                    Silo silo = lpSilo.IdSiloNavigation;
                    if (filter.idLoadingPoints.Count > 0 && !filter.idLoadingPoints.Contains(lpSilo.IdLoadingPoint))
                    {
                        continue;
                    }
                    debugList.Clear();
                    SiloSet siloSet = new()
                    {
                        TheBasicType = basicType,
                        Priority = silo.Priority,
                        TheLoadingPoint = lpSilo.IdLoadingPointNavigation,
                        MixIndex = basicType.MixIndex
                    };
                    SiloItem siloItem = new()
                    {
                        Position = 1,
                        TheSilo = silo,
                        Percentage = 999,
                    };
                    debugList.Add(silo.SiloNumber.ToString() + " " + siloItem.Percentage + '%');
                    siloSet.SiloItems.Add(siloItem);
                    if (!AddSiloSet(siloSet))  // mit Checks, Priority
                        Btsc.log.Warning(string.Join(", ", debugList) + ": Siloset bereits vorhanden");
                    else
                        Btsc.log.Information(string.Join(", ", debugList));
                }

            }
        }
    }

    private async Task AddMixedBasicTypes()
    {
        // BaseTypes mit Material ..
        List<BasicType> basicTypes = await LoadingDbService.GetBasicTypesByMaterialId(Btsc, filter.idMaterial, false);
        foreach (var basicType in basicTypes)
        {
            // .. plus MapingBasicType mit Other Material
            var mappedBasicTypes = await LoadingDbService.GetMappedTypesByBasicType(Btsc, basicType.Id);
            if (mappedBasicTypes.Count > 0)  //if (basicType.MixIndex <= 0)
            {
                // Mischsorten:
                if (basicType.MixIndex == 0)
                {
                    AddError($"Falscher MixIndex 0. Muss >0 sein bei Mix. BasicType:{basicType.IdMaterialNavigation.Code}");
                    continue;  //produktiv
                }
                var loadingPoints = await LoadingDbService.GetLoadingPoints(Btsc);
                foreach (var loadingPoint in loadingPoints)
                {
                    if (filter.idLoadingPoints.Count > 0 && !filter.idLoadingPoints.Contains(loadingPoint.Id))
                    {
                        continue;
                    }
                    List<List<MappingSiloLoadingPoint>> siloLists = new();  //Silo+LoadingPoint
                    Dictionary<int, decimal> percentageList = new();

                    debugList.Clear();
                    foreach (var mappedbasicType in mappedBasicTypes)
                    {
                        var lpSilos = await LoadingDbService.GetLoadingpointSilosByBasictype(Btsc,
                            mappedbasicType.IdOtherType, loadingPoint.Id);
                        if (lpSilos.Count > 0)
                        {
                            siloLists.Add(lpSilos);
                            var i = mappedBasicTypes.IndexOf(mappedbasicType);
                            percentageList[i] = mappedbasicType.Percentage;
                        }
                        if (lpSilos.Count == 0)
                        {
                            debugList.Add(string.Format("kein Silo für Material:{0} BasicType:{1}.Mix{2} Point:{3}",
                                basicType.IdMaterialNavigation.Code, basicType.Id, basicType.MixIndex, loadingPoint.Name));
                        }
                        var idsilos = lpSilos.Select(x => x.IdSiloNavigation.SiloNumber).ToList();
                        Btsc.log.Information("{0} Silos.Pt{1} für IdOtherType.{2}: {3}",
                            basicType.IdMaterialNavigation.Code, loadingPoint.LoadingNumber, mappedbasicType.IdOtherTypeNavigation.IdMaterialNavigation.Code,
                              string.Join(", ", idsilos));
                    }
                    if (siloLists.Count == 0)
                    {
                        Btsc.log.Information("Keine Silo Kombinationen Basetype:{0}.{1} Point:{2}",
                            basicType.IdMaterialNavigation.Code, basicType.MixIndex, loadingPoint.Name);
                        continue;
                    }
                    if (siloLists.Count < mappedBasicTypes.Count)
                    {
                        AddError(string.Join(Environment.NewLine, debugList));
                        continue;  //produktiv
                    }

                    // Varianten aller Silo Kombinationen
                    Btsc.log.Information("Silo Kombinationen Basetype:{0}.{1} Point:{2}",
                        basicType.IdMaterialNavigation.Code, basicType.MixIndex, loadingPoint.Name);
                    foreach (IEnumerable<Object> siloList in CartesianProductContainer.Cartesian(siloLists))
                    {
                        debugList.Clear();
                        SiloSet siloSet = new()
                        {
                            TheBasicType = basicType,
                            Priority = 99,  // see AddSiloSet
                            TheLoadingPoint = loadingPoint,
                            MixIndex = basicType.MixIndex
                        };

                        int pos = 1;
                        foreach (var silo in siloList.Cast<MappingSiloLoadingPoint>())
                        {
                            SiloItem siloItem = new()
                            {
                                Position = pos,
                                TheSilo = silo.IdSiloNavigation,
                                Percentage = percentageList[pos - 1],
                            };
                            debugList.Add(silo.IdSiloNavigation.SiloNumber.ToString() + " " + siloItem.Percentage + '%');

                            pos++;
                            siloSet.SiloItems.Add(siloItem);
                        }
                        if (!AddSiloSet(siloSet))  // mit Checks, Priority
                            Btsc.log.Warning(string.Join(", ", debugList) + ": Siloset bereits vorhanden");
                        else
                            Btsc.log.Information(string.Join(", ", debugList));
                    }

                }
            }
        }
    }

}
