using Quva.Database.Models;
using Quva.Services.Enums;
using Serilog;
using System.Reflection.Emit;

namespace Quva.Services.Loading;

internal class LoadOrderService
{
    public BtsContext Btsc;
    private readonly LoadingResult _loadingResult;

    public LoadOrderService(BtsContext btsc, LoadingResult loadingResult)
    {
        Btsc = btsc;
        _loadingResult = loadingResult;
    }

    public static async Task<LoadingResult> CreateLoadorder(BtsContext btsc, LoadingParameter parameter)
    {
        LoadingResult result = new();
        LoadOrderService loService = new(btsc, result);

        if (parameter.SiloSet != null)
        {
            //throw new NotImplementedException("LoadingParameter SiloSet is not implemented");
            loService.AddError("LoadingParameter SiloSet is not implemented");
            return result;
        }

        var delivery = await LoadingDbService.FindDelivery(btsc, parameter.IdDelivery);
        if (delivery == null)
        {
            loService.AddError($"IdDelivery not found ({parameter.IdDelivery})");
            return result;
        }

        var loadingPoints = await LoadingDbService.GetLoadingPointsByShippingMethod(btsc,
            delivery.DeliveryOrder!.IdShippingMethodNavigation);
        if (loadingPoints.Count == 0)
        {
            loService.AddError($"No Loading Points for IdDelivery:({parameter.IdDelivery})");
            return result;
        }
        foreach (var loadingPoint in loadingPoints)
        {
            // Check if active Loadorder with this Point already exists
            var activeOrder = await LoadingDbService.GetActiveLoadorder(btsc, parameter.IdDelivery, loadingPoint.Id);
            if (activeOrder != null)
            {
                loService.AddError($"Active Loadorder already exists. Point({loadingPoint.Name}) ID({activeOrder.Id})");
                continue;
            }

            LoadorderHead hdr = new()
            {
                // obligatory:
                Id = 0,
                IdDelivery = parameter.IdDelivery,
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
            var silos = await BasetypeSilos.CreateByDelivery(btsc, delivery, loadingPoint.Id);
            result.AddErrorLines(silos.ErrorLines);
            if (silos.SiloSets.Count == 0)
            {
                loService.AddError($"No Silos for IdDelivery:({parameter.IdDelivery}) Point({loadingPoint.LoadingNumber})");
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
            var idLoadorder = await LoadingDbService.SaveLoadorder(btsc, hdr);
            result.IdLoadorders.Add(idLoadorder);

        }  // loadingPoint

        await Task.Delay(0);
        return result;
    }

    private void AddError(string message)
    {
        Btsc.log.Error(message);
        _loadingResult.ErrorLines.Add(message);
    }

}
