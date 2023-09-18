using Quva.Database.Models;

namespace Quva.Services.Loading;

public partial class BasetypeService
{
    private async Task AddTrueBasicTypes(BasetypeSilos bts)
    {
        // BaseTypes mit Material ..
        var basicTypes = await _loadingDbService.GetBasicTypesByMaterialId(bts.filter.IdLocation, bts.filter.idMaterial, false);
        foreach (var basicType in basicTypes)
        {
            // .. plus MapingBasicType mit Other Material
            var mappedBasicTypes = await _loadingDbService.GetMappedTypesByBasicType(basicType.Id);
            if (mappedBasicTypes.Count <= 0)  //if (basicType.MixIndex <= 0)
            {
                if (basicType.MixIndex > 0)
                {
                    //AddError(bts, $"Falscher MixIndex {basicType.MixIndex}. Muss 0 sein wenn kein Mix. BasicType:{basicType.IdMaterialNavigation.Code}");
                    bts.AddError(TrCode.LoadingService.WrongMixindex, basicType.MixIndex, basicType.IdMaterialNavigation.Code);
                    continue;  //produktiv
                }
                var lpSilos = await _loadingDbService.GetLoadingpointSilosByBasictype(basicType.Id, null);
                foreach (var lpSilo in lpSilos)
                {
                    Silo silo = lpSilo.IdSiloNavigation;
                    if (bts.filter.idLoadingPoints.Count > 0 && !bts.filter.idLoadingPoints.Contains(lpSilo.IdLoadingPoint))
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
                    debugList.Add(new ErrorLine(silo.SiloNumber.ToString() + " " + siloItem.Percentage + '%'));
                    siloSet.SiloItems.Add(siloItem);
                    var sl = DebugStringList(Enums.LanguageEnum.EN);
                    if (!bts.AddSiloSet(siloSet))  // mit Checks, Priority
                        _log.Warning(string.Join(", ", sl) + ": Siloset bereits vorhanden");
                    else
                        _log.Information(string.Join(", ", sl));
                }

            }
        }
    }

    private async Task AddMixedBasicTypes(BasetypeSilos bts)
    {
        // BaseTypes mit Material ..
        var basicTypes = await _loadingDbService.GetBasicTypesByMaterialId(bts.filter.IdLocation, bts.filter.idMaterial, false);
        foreach (var basicType in basicTypes)
        {
            // .. plus MapingBasicType mit Other Material
            var mappedBasicTypes = await _loadingDbService.GetMappedTypesByBasicType(basicType.Id);
            if (mappedBasicTypes.Count > 0)  //if (basicType.MixIndex <= 0)
            {
                // Mischsorten:
                if (basicType.MixIndex == 0)
                {
                    //AddError(bts, $"Falscher MixIndex 0. Muss >0 sein bei Mix. BasicType:{basicType.IdMaterialNavigation.Code}");
                    bts.AddError(TrCode.LoadingService.WrongMixindex0, basicType.IdMaterialNavigation.Code);
                    continue;  //produktiv
                }
                var loadingPoints = await _loadingDbService.GetLoadingPoints(bts.filter.IdLocation);
                foreach (var loadingPoint in loadingPoints)
                {
                    if (bts.filter.idLoadingPoints.Count > 0 && !bts.filter.idLoadingPoints.Contains(loadingPoint.Id))
                    {
                        continue;
                    }
                    List<List<MappingSiloLoadingPoint>> siloLists = new();  //Silo+LoadingPoint
                    Dictionary<int, decimal> percentageList = new();

                    debugList.Clear();
                    foreach (var mappedbasicType in mappedBasicTypes)
                    {
                        var lpSilos = await _loadingDbService.GetLoadingpointSilosByBasictype(mappedbasicType.IdOtherType,
                            loadingPoint.Id);
                        if (lpSilos.Count > 0)
                        {
                            siloLists.Add(lpSilos);
                            var i = mappedBasicTypes.IndexOf(mappedbasicType);
                            percentageList[i] = mappedbasicType.Percentage;
                        }
                        if (lpSilos.Count == 0)
                        {
                            //debugList.Add(string.Format("No Silo for Material:{0} BasicType:{1}.Mix{2} Point:{3}",
                            //    basicType.IdMaterialNavigation.Code, basicType.Id, basicType.MixIndex, loadingPoint.Name));
                            debugList.Add(new ErrorLine(TrCode.LoadingService.NoSiloMaterial, 
                                basicType.IdMaterialNavigation.Code, basicType.Id, basicType.MixIndex, loadingPoint.Name));
                        }
                        var idsilos = lpSilos.Select(x => x.IdSiloNavigation.SiloNumber).ToList();
                        _log.Information("{0} Silos.Pt{1} für IdOtherType.{2}: {3}",
                            basicType.IdMaterialNavigation.Code, loadingPoint.LoadingNumber, mappedbasicType.IdOtherTypeNavigation.IdMaterialNavigation.Code,
                              string.Join(", ", idsilos));
                    }
                    if (siloLists.Count == 0)
                    {
                        _log.Information("Keine Silo Kombinationen Basetype:{0}.{1} Point:{2}",
                            basicType.IdMaterialNavigation.Code, basicType.MixIndex, loadingPoint.Name);
                        continue;
                    }
                    if (siloLists.Count < mappedBasicTypes.Count)
                    {
                        //AddError(bts, string.Join(Environment.NewLine, debugList));
                        foreach (var debug in debugList)
                        {
                            bts.AddError(debug.Code, debug.Parameter);
                        }
                        continue;  //produktiv
                    }

                    // Varianten aller Silo Kombinationen
                    _log.Information("Silo Kombinationen Basetype:{0}.{1} Point:{2}",
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
                            debugList.Add(new ErrorLine(
                                silo.IdSiloNavigation.SiloNumber.ToString() + " " + siloItem.Percentage + '%'));

                            pos++;
                            siloSet.SiloItems.Add(siloItem);
                        }
                        var sl = DebugStringList(Enums.LanguageEnum.EN);
                        if (!bts.AddSiloSet(siloSet))  // mit Checks, Priority
                            _log.Warning(string.Join(", ", sl) + ": Siloset bereits vorhanden");
                        else
                            _log.Information(string.Join(", ", sl));
                    }

                }
            }
        }
    }

}
