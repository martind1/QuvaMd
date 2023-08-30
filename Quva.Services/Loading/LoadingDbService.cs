using Microsoft.EntityFrameworkCore;
using Quva.Database.Models;
using Quva.Services.Enums;
using Serilog;

namespace Quva.Services.Loading;

public static class LoadingDbService
{
    // plus Material
    public static async Task<List<BasicType>> GetBasicTypesByMaterialId(BtsContext btsc, long? idMaterial, bool onlyTrue)
    {
        // onlyTrue: true = only True Basic Types (no Mix Types)
        btsc.log.Debug($"GetBasicTypesByMaterialId {idMaterial}");
        var query = from bt in btsc.context.BasicType
                    .Include(bt => bt.IdMaterialNavigation)
                    where bt.IdLocation == btsc.idLocation 
                       && (idMaterial == null || bt.IdMaterial == idMaterial)
                       && (!onlyTrue || bt.MixIndex == 0)
                    select bt;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<List<MappingBasicType>> GetMappedTypesByBasicType(BtsContext btsc, long idBasicType)
    {
        // plus Other Material
        btsc.log.Debug($"GetMappedTypesByBasicType {idBasicType}");
        var query = from mbt in btsc.context.MappingBasicType
                    .Include(mbt => mbt.IdOtherTypeNavigation)
                        .ThenInclude(otn => otn.IdMaterialNavigation)
                    where mbt.IdBasicType == idBasicType
                    orderby mbt.Position
                    select mbt;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<List<Silo>> GetSilosByBasictype(BtsContext btsc, long idBasicType)
    {
        // with Additional Basic Types as LEFT JOIN
        // https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators#left-join
        btsc.log.Debug($"GetSilosByBasictype {idBasicType}");
        var query = from sil in btsc.context.Silo
                    join ads in btsc.context.AdditionalBasicType
                        on sil.Id equals ads.IdSilo
                        into grouping
                    from ads in grouping.DefaultIfEmpty()
                    where sil.IdBasicType == idBasicType || ads.IdBasicType == idBasicType
                    select sil;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<List<MappingSiloLoadingPoint>> GetLoadingpointSilosByBasictype(BtsContext btsc, long idBasicType,
        long? idLoadingPoint)
    {
        //Loadingpoint + Silo
        btsc.log.Debug($"GetLoadingpointSilosByBasictype {idBasicType}");
        var silos = await GetSilosByBasictype(btsc, idBasicType);
        var idSilos = (from s in silos select s.Id).ToList();

        var query = from lp in btsc.context.MappingSiloLoadingPoint
                    .Include(lp => lp.IdSiloNavigation)
                    .Include(lp => lp.IdLoadingPointNavigation)
                    where idSilos.Contains(lp.IdSiloNavigation.Id)
                       && (idLoadingPoint == null || lp.IdLoadingPointNavigation.Id == idLoadingPoint)
                    select lp;
        var result = await query.ToListAsync();
        return result;
    }


    public static async Task<List<LoadingPoint>> GetLoadingPointsBySilo(BtsContext btsc, long idSilo)
    {
        btsc.log.Debug($"GetIdLoadingPointsFromSilo {idSilo}");
        var query = from mlp in btsc.context.MappingSiloLoadingPoint
                    .Include(mlp => mlp.IdLoadingPointNavigation)
                    where mlp.IdSilo == idSilo
                    select mlp.IdLoadingPointNavigation;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<List<LoadingPoint>> GetLoadingPoints(BtsContext btsc)
    {
        btsc.log.Debug($"GetLoadingPoints {btsc.idLocation}");
        var query = from lp in btsc.context.LoadingPoint
                    where lp.IdLocation == btsc.idLocation
                    select lp;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<DeliveryHead?> FindDelivery(BtsContext btsc, long idDelivery)
    {
        // mit DeliverOrder, Plant, DeliverOrderDebitor, DeliveryOrderPosition
        //   ShippingMethod
        btsc.log.Debug($"FindDelivery {idDelivery}");
        var query = from del in btsc.context.DeliveryHead
                    .Include(del => del.DeliveryOrder)
                        .ThenInclude(pla => pla!.IdPlantNavigation)
                    .Include(del => del.DeliveryOrder)
                        .ThenInclude(shi => shi!.IdShippingMethodNavigation)
                    .Include(del => del.DeliveryOrder)
                        .ThenInclude(ord => ord!.DeliveryOrderDebitor
                            .Where(dob => dob.Role == (int)OrderDebitorRole.GoodsRecipient))
                    .Include(del => del.DeliveryOrder)
                        .ThenInclude(op => op!.DeliveryOrderPosition
                            .Where(dop => dop.MainPosition == true))
                    .AsSplitQuery()
                    where del.Id == idDelivery
                    select del;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public static async Task<long> GetIdDebitorByNumber(BtsContext btsc, long debitorNumber)
    {
        btsc.log.Debug($"GetIdDebitorByNumber {debitorNumber}");
        var query = from lp in btsc.context.Debitor
                    where lp.DebitorNumber == debitorNumber
                    select lp.Id;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public static async Task<long> GetIdMaterialByCode(BtsContext btsc, string code)
    {
        btsc.log.Debug($"GetIdMaterialByCode {code}");
        var query = from lp in btsc.context.Material
                    where lp.Code == code
                    select lp.Id;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }


    public static async Task<List<LoadingPoint>> GetLoadingPointsByShippingMethod(BtsContext btsc, ShippingMethod shippingMethod)
    {
        btsc.log.Debug($"GetLoadingPointsByShippingMethod {shippingMethod.Name}");
        TransportTypeValues[] allowedTransportTypes = (TransportTypeValues)shippingMethod.TransportType switch
        {
            TransportTypeValues.Rail => new TransportTypeValues[] { TransportTypeValues.All,
                                                                    TransportTypeValues.Rail },
            TransportTypeValues.Truck => new TransportTypeValues[] { TransportTypeValues.All,
                                                                    TransportTypeValues.Truck },
            _ => new TransportTypeValues[] { TransportTypeValues.All,
                                             TransportTypeValues.Rail,
                                             TransportTypeValues.Truck },
        };

        PackagingTypeValues[] allowedPackagingTypes = (PackagingTypeValues)shippingMethod.PackagingType switch
        {
            PackagingTypeValues.Packaged => new PackagingTypeValues[] { PackagingTypeValues.All,
                                                                        PackagingTypeValues.Packaged },
            PackagingTypeValues.Bulk => new PackagingTypeValues[] { PackagingTypeValues.All,
                                                                    PackagingTypeValues.Bulk },
            _ => new PackagingTypeValues[] { PackagingTypeValues.All,
                                             PackagingTypeValues.Packaged,
                                             PackagingTypeValues.Bulk },
        };

        var query = from lp in btsc.context.LoadingPoint
                    where allowedTransportTypes.Contains((TransportTypeValues)lp.TransportType)
                       && allowedPackagingTypes.Contains((PackagingTypeValues)lp.PackagingType)
                    select lp;
        var result = await query.ToListAsync();
        return result;
    }


    public static async Task<long> SaveLoadorder(BtsContext btsc, LoadorderHead loadorder)
    {
        btsc.log.Debug($"SaveLoadorder {loadorder.IdDelivery} Id:{loadorder.Id}");
        if (loadorder.Id == 0)
        {
            loadorder.CreateUser = "LoadOrderService";  //is notnull and no trigger logic
            btsc.context.LoadorderHead.Add(loadorder);
        }
        else
        {
            loadorder.ChangeUser = "LoadOrderService";
            loadorder.ChangeDate = DateTime.Now;
            loadorder.ChangeNumber++;
            btsc.context.LoadorderHead.Update(loadorder);
        }
        await btsc.context.SaveChangesAsync();
        btsc.log.Debug($"Saved ID:{loadorder.Id}");
        return await Task.FromResult(loadorder.Id);
    }

    
    public static async Task<LoadorderHead?> GetActiveLoadorder(BtsContext btsc, long idDelivery, long idLoadingPoint)
    {
        btsc.log.Debug($"GetActiveLoadorder {idDelivery}, {idLoadingPoint}");
        var query = from lo in btsc.context.LoadorderHead
                    where lo.IdDelivery == idDelivery
                       && lo.IdLoadingPoint == idLoadingPoint
                       && lo.LoadorderState < (int)LoadorderStateValues.Inactive
                    select lo;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public static async Task<List<Contingent>> GetActiveContingents(BtsContext btsc, 
        long? idDebitor, long idMaterial, DateTime? date)
    {
        // with ContingentSilo, Silo, LoadingPoint
        string day = date != null ? ((DateTime)date).ToString("yyyy-MM-dd") : "null";
        btsc.log.Debug($"GetActiveContingents IdDeb:{idDebitor ?? 0} IdMat:{idMaterial} Day:{day}");
        var query = from cs in btsc.context.Contingent
                    .Include(csi => csi.ContingentSilo)
                        .ThenInclude(csi => csi.IdSiloNavigation)
                    .Include(cs => cs.IdLoadingPointNavigation)
                    where cs.IdLocation == btsc.idLocation
                       && ((idDebitor == null && cs.IdDebitor == null) || cs.IdDebitor == (idDebitor ?? 0))
                       && cs.IdMaterial == idMaterial
                       && cs.Active == true
                       && (date == null || cs.ValidFrom == null || cs.ValidFrom <= date)
                       && (date == null || cs.ValidTo == null || cs.ValidTo >= date)
                    select cs;
        var result = await query.ToListAsync();
        return result;
    }



}
