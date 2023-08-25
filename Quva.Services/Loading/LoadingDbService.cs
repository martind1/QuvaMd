using Microsoft.EntityFrameworkCore;
using Quva.Database.Models;

namespace Quva.Services.Loading;

public static class LoadingDbService
{
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
        btsc.log.Debug($"GetMappedTypesFromBasicType {idBasicType}");
        var query = from mbt in btsc.context.MappingBasicType
                    where mbt.IdBasicType == idBasicType
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

    public static async Task<List<MappingSiloLoadingPoint>> GetLoadingpointSilosFromBasictype(BtsContext btsc, long idBasicType)
    {
        //Loadingpoint + Silo
        var silos = await GetSilosFromBasictype(btsc, idBasicType);
        var idSilos = (from s in silos select s.Id).ToList();
        btsc.log.Debug($"GetLoadingpointSilosFromBasictype {idBasicType}");
        var query = from lp in btsc.context.MappingSiloLoadingPoint
                    .Include(lp => lp.IdSiloNavigation)
                    .Include(lp => lp.IdLoadingPointNavigation)
                    where idSilos.Contains(lp.IdSiloNavigation.Id)
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

}
