using Microsoft.EntityFrameworkCore;
using Quva.Database.Models;
using Quva.Services.Enums;

namespace Quva.Services.Loading;

public static class LoadingDbService
{
    // plus Material
    public static async Task<List<BasicType>> GetBasicTypesFromMaterialId(BtsContext btsc, long? idMaterial)
    {
        btsc.log.Debug($"GetBasicTypesFromMaterialId {idMaterial}");
        var query = from bt in btsc.context.BasicType
                    .Include(bt => bt.IdMaterialNavigation)
                    where bt.IdLocation == btsc.idLocation &&
                          (idMaterial == null || bt.IdMaterial == idMaterial)
                    select bt;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<List<MappingBasicType>> GetMappedTypesFromBasicType(BtsContext btsc, long idBasicType)
    {
        // plus Other Material
        btsc.log.Debug($"GetMappedTypesFromBasicType {idBasicType}");
        var query = from mbt in btsc.context.MappingBasicType
                    .Include(mbt => mbt.IdOtherTypeNavigation)
                        .ThenInclude(otn => otn.IdMaterialNavigation)
                    where mbt.IdBasicType == idBasicType
                    orderby mbt.Position
                    select mbt;
        var result = await query.ToListAsync();
        return result;
    }

    public static async Task<List<Silo>> GetSilosFromBasictype(BtsContext btsc, long idBasicType)
    {
        // with Additional Basic Types as LEFT JOIN
        // https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators#left-join
        btsc.log.Debug($"GetSilosFromBasictype {idBasicType}");
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

    public static async Task<List<MappingSiloLoadingPoint>> GetLoadingpointSilosFromBasictype(BtsContext btsc, long idBasicType,
        long? idLoadingPoint)
    {
        //Loadingpoint + Silo
        btsc.log.Debug($"GetLoadingpointSilosFromBasictype {idBasicType}");
        var silos = await GetSilosFromBasictype(btsc, idBasicType);
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


    public static async Task<List<LoadingPoint>> GetLoadingPointsFromSilo(BtsContext btsc, long idSilo)
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
        btsc.log.Debug($"FindDelivery {idDelivery}");
        var query = from del in btsc.context.DeliveryHead
                    .Include(del => del.DeliveryOrder)
                        .ThenInclude(pla => pla!.IdPlantNavigation)
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


}
