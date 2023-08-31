using Quva.Database.Models;
using Quva.Services.Enums;
using Quva.Services.Loading.Interfaces;
using Serilog;
using System.Reflection.Emit;

namespace Quva.Services.Loading;

public class LoadOrderService : ILoadOrderService
{
    private readonly ILogger _log;
    private readonly ILoadingDbService _loadingDbService;
    private readonly IBasetypeService _basetypeService;

    public LoadOrderService(ILoadingDbService loadingDbService, IBasetypeService basetypeService)
    {
        _log = Log.ForContext(GetType());
        _loadingDbService = loadingDbService;
        _basetypeService = basetypeService;
    }

    public async Task<LoadingResult> CreateLoadorder(LoadingParameter parameter)
    {
        LoadingResult result = new();

        if (parameter.SiloSet != null)
        {
            //throw new NotImplementedException("LoadingParameter SiloSet is not implemented");
            AddError(result, "LoadingParameter SiloSet is not implemented");
            return result;
        }

        var delivery = await _loadingDbService.FindDelivery(parameter.IdDelivery);
        if (delivery == null)
        {
            AddError(result, $"IdDelivery not found ({parameter.IdDelivery})");
            return result;
        }

        var loadingPoints = await _loadingDbService.GetLoadingPointsByShippingMethod(parameter.IdLocation,
            delivery.DeliveryOrder!.IdShippingMethodNavigation);
        if (loadingPoints.Count == 0)
        {
            AddError(result, $"No Loading Points for IdDelivery:({parameter.IdDelivery})");
            return result;
        }

        if (delivery.DeliveryPosition.Count <= 0)
        {
            AddError(result, $"No Main Positions for IdDelivery:({parameter.IdDelivery})");
            return result;
        }

        foreach (var loadingPoint in loadingPoints)
        {
            // Check if active Loadorder with this Point already exists
            var activeOrder = await _loadingDbService.GetActiveLoadorder(parameter.IdDelivery, loadingPoint.Id);
            if (activeOrder != null)
            {
                AddError(result, $"Active Loadorder already exists. Point({loadingPoint.Name}) ID({activeOrder.Id})");
                continue;
            }

            LoadorderHead hdr = new()
            {
                // obligatory:
                Id = 0,
                IdDelivery = parameter.IdDelivery,
                PositionNumber = delivery.DeliveryPosition.First().PositionNumber,
                LoadorderState = (int)LoadorderStateValues.ToLoad,
                IdLoadingPoint = loadingPoint.Id,
                CreateUser = delivery.CreateUser,

                TargetQuantity = parameter.TargetQuantity,
                MaxGross = delivery.MaxGross,
                WeighingUnit = delivery.NetUnit
            };
            // TODO: ActivePartNumber
            // TODO: MoistLock (from CustAgree?)
            // TODO: set Flag* Fields = 0 or from CustAgree/LocParam

            // Teilmengen:
            if (parameter.PartQuantities == null || parameter.PartQuantities.Count <= 0)
            {
                LoadorderPart part = new()
                {
                    PartNumber = 1,
                    TargetQuantity = hdr.TargetQuantity,
                    CreateUser = delivery.CreateUser,
                };
                hdr.LoadorderPart.Add(part);
            }
            else
            {
                int partNumber = 1;
                foreach (var quant in parameter.PartQuantities)
                {
                    LoadorderPart part = new()
                    {
                        PartNumber = partNumber++,
                        TargetQuantity = quant,
                        CreateUser = delivery.CreateUser,
                    };
                    hdr.LoadorderPart.Add(part);
                }
            }

            // Silos:
            var silos = await _basetypeService.GetByDelivery(delivery, loadingPoint.Id);
            result.AddErrorLines(silos.ErrorLines);
            if (silos.SiloSets.Count == 0)
            {
                AddError(result, $"No Silos for IdDelivery:({parameter.IdDelivery}) Point({loadingPoint.LoadingNumber})");
                return result;
            }
            silos.SortByPrio();  // sorts silosets
            int idx = 0;
            foreach (var siloset in silos.SiloSets)
            {
                foreach (var siloitem in siloset.SiloItems)
                {
                    LoadorderSilo silo = new()
                    {
                        SiloSet = idx,
                        Position = siloitem.Position,
                        IdSilo = siloitem.TheSilo?.Id,
                        SiloNumber = siloitem.TheSilo?.SiloNumber,
                        Akz = siloitem.TheSilo?.Akz,
                        SpsCode = siloitem.TheSilo?.SpsCode,
                        IdBasicType = siloitem.TheSilo?.IdBasicType,
                        SiloLevelVolume = siloitem.TheSilo?.SiloLevelVolume,

                        Percentage = siloitem.Percentage,
                        PowerTh = siloitem.PowerTh,

                        CreateUser = delivery.CreateUser
                    };
                    hdr.LoadorderSilo.Add(silo);
                }
                idx++;
            }

            // persist:
            var idLoadorder = await _loadingDbService.SaveLoadorder(hdr);
            result.IdLoadorders.Add(idLoadorder);

        }  // loadingPoint

        await Task.Delay(0);
        return result;
    }

    private void AddError(LoadingResult loadingResult, string message)
    {
        _log.Error(message);
        loadingResult.ErrorLines.Add(message);
    }

}
