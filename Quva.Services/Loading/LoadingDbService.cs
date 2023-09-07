using Microsoft.EntityFrameworkCore;
using Quva.Database.Models;
using Quva.Services.Enums;
using Quva.Services.Loading.Interfaces;
using Serilog;

namespace Quva.Services.Loading;

public class LoadingDbService : ILoadingDbService
{
    private readonly QuvaContext _context;
    private readonly ILogger _log;

    public LoadingDbService(QuvaContext context)
    {
        _log = Log.ForContext(GetType());
        _context = context;
    }



    // plus Material
    public async Task<List<BasicType>> GetBasicTypesByMaterialId(long idLocation, long? idMaterial, bool onlyTrue)
    {
        // onlyTrue: true = only True Basic Types (no Mix Types)
        _log.Debug($"GetBasicTypesByMaterialId {idMaterial}");
        var query = from bt in _context.BasicType
                    .Include(bt => bt.IdMaterialNavigation)
                    where bt.IdLocation == idLocation
                       && (idMaterial == null || bt.IdMaterial == idMaterial)
                       && (!onlyTrue || bt.MixIndex == 0)
                    select bt;
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MappingBasicType>> GetMappedTypesByBasicType(long idBasicType)
    {
        // plus Other Material
        _log.Debug($"GetMappedTypesByBasicType {idBasicType}");
        var query = from mbt in _context.MappingBasicType
                    .Include(mbt => mbt.IdOtherTypeNavigation)
                        .ThenInclude(otn => otn.IdMaterialNavigation)
                    where mbt.IdBasicType == idBasicType
                    orderby mbt.Position
                    select mbt;
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<Silo>> GetSilosByBasictype(long idBasicType)
    {
        // with Additional Basic Types as LEFT JOIN
        // https://learn.microsoft.com/en-us/ef/core/querying/complex-query-operators#left-join
        _log.Debug($"GetSilosByBasictype {idBasicType}");
        var query = from sil in _context.Silo
                    join ads in _context.AdditionalBasicType
                        on sil.Id equals ads.IdSilo
                        into grouping
                    from ads in grouping.DefaultIfEmpty()
                    where sil.IdBasicType == idBasicType || ads.IdBasicType == idBasicType
                    select sil;
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<MappingSiloLoadingPoint>> GetLoadingpointSilosByBasictype(long idBasicType,
        long? idLoadingPoint)
    {
        //Loadingpoint + Silo
        _log.Debug($"GetLoadingpointSilosByBasictype {idBasicType}");
        var silos = await GetSilosByBasictype(idBasicType);
        var idSilos = (from s in silos select s.Id).ToList();

        var query = from lp in _context.MappingSiloLoadingPoint
                    .Include(lp => lp.IdSiloNavigation)
                    .Include(lp => lp.IdLoadingPointNavigation)
                    where idSilos.Contains(lp.IdSiloNavigation.Id)
                       && (idLoadingPoint == null || lp.IdLoadingPointNavigation.Id == idLoadingPoint)
                    select lp;
        var result = await query.ToListAsync();
        return result;
    }


    public async Task<List<LoadingPoint>> GetLoadingPointsBySilo(long idLocation, long idSilo)
    {
        _log.Debug($"GetIdLoadingPointsFromSilo {idSilo}");
        var query = from mlp in _context.MappingSiloLoadingPoint
                    .Include(mlp => mlp.IdLoadingPointNavigation)
                    where mlp.IdSilo == idSilo
                    select mlp.IdLoadingPointNavigation;
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<LoadingPoint>> GetLoadingPoints(long idLocation)
    {
        _log.Debug($"GetLoadingPoints {idLocation}");
        var query = from lp in _context.LoadingPoint
                    where lp.IdLocation == idLocation
                    select lp;
        var result = await query.ToListAsync();
        return result;
    }

    public async Task<DeliveryHead?> FindDelivery(long idDelivery)
    {
        // mit DeliverOrder, Plant, DeliverOrderDebitor, DeliveryOrderPosition (only mainPosition)
        //   ShippingMethod
        _log.Debug($"FindDelivery {idDelivery}");
        var query = from del in _context.DeliveryHead
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
                        .ThenInclude(dop => dop.IdDeliveryPositionNavigation)
                    .AsSplitQuery()
                    where del.Id == idDelivery
                    select del;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<OrderHead?> FindOrder(long idOrder)
    {
        // mit DeliverOrder, Plant, DeliverOrderDebitor, DeliveryOrderPosition (only mainPosition)
        //   ShippingMethod
        _log.Debug($"FindOrder {idOrder}");
        var query = from ord in _context.OrderHead
                        .Include(pla => pla!.IdPlantNavigation)
                        .Include(shi => shi!.IdShippingMethodNavigation)
                        .Include(ord => ord!.OrderDebitor
                            .Where(dob => dob.Role == (int)OrderDebitorRole.GoodsRecipient))
                        .Include(op => op!.OrderPosition
                            .Where(dop => dop.MainPosition == true))
                    .AsSplitQuery()
                    where ord.Id == idOrder
                    select ord;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<long> GetIdDebitorByNumber(long debitorNumber)
    {
        _log.Debug($"GetIdDebitorByNumber {debitorNumber}");
        var query = from lp in _context.Debitor
                    where lp.DebitorNumber == debitorNumber
                    select lp.Id;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<long> GetIdMaterialByCode(string code)
    {
        _log.Debug($"GetIdMaterialByCode {code}");
        var query = from lp in _context.Material
                    where lp.Code == code
                    select lp.Id;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<Vehicle?> GetVehicleByPlate(string plate)
    {
        _log.Debug($"GetVehicleByPlate {plate}");
        var query = from ve in _context.Vehicle
                    where ve.LicensePlate == plate
                    select ve;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }


    public async Task<List<LoadingPoint>> GetLoadingPointsByShippingMethod(long idLocation, ShippingMethod shippingMethod)
    {
        _log.Debug($"GetLoadingPointsByShippingMethod {shippingMethod.Name}");
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

        var query = from lp in _context.LoadingPoint
                    where allowedTransportTypes.Contains((TransportTypeValues)lp.TransportType)
                       && allowedPackagingTypes.Contains((PackagingTypeValues)lp.PackagingType)
                       && lp.IdLocation == idLocation
                    select lp;
        var result = await query.ToListAsync();
        return result;
    }


    public async Task<long> SaveLoadorder(LoadorderHead loadorder)
    {
        _log.Debug($"SaveLoadorder {loadorder.IdDelivery} Id:{loadorder.Id}");
        if (loadorder.Id == 0)
        {
            loadorder.CreateUser = "LoadOrderService";  //is notnull and no trigger logic
            _context.LoadorderHead.Add(loadorder);
        }
        else
        {
            loadorder.ChangeUser = "LoadOrderService";
            loadorder.ChangeDate = DateTime.Now;
            loadorder.ChangeNumber++;
            _context.LoadorderHead.Update(loadorder);
        }
        await _context.SaveChangesAsync();
        _log.Debug($"Saved ID:{loadorder.Id}");
        return await Task.FromResult(loadorder.Id);
    }


    public async Task<LoadorderHead?> GetActiveLoadorder(long idDelivery, long idLoadingPoint, int[] activeStates)
    {
        _log.Debug($"GetActiveLoadorder {idDelivery}, {idLoadingPoint}");
        var query = from lo in _context.LoadorderHead
                    where lo.IdDelivery == idDelivery
                       && lo.IdLoadingPoint == idLoadingPoint
                       && activeStates.Contains(lo.LoadorderState)
                    select lo;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task<LoadingPoint?> GetLoadingPointByLoadorder(long idLoadorder)
    {
        _log.Debug($"GetLoadingPointByLoadorder {idLoadorder}");
        var query1 = from lo in _context.LoadorderHead
                    where lo.Id == idLoadorder
                    select lo.IdLoadingPoint;
        var idLoadingPoint = await query1.FirstOrDefaultAsync();
        if (idLoadingPoint == default) return null;

        var query = from lo in _context.LoadingPoint
                    where lo.Id == idLoadingPoint
                    select lo;
        var result = await query.FirstOrDefaultAsync();
        return result;
    }

    public async Task SetLoadingPointLoadorder(LoadingPoint loadingPoint, long idLoadorder)
    {
        _log.Debug($"SetLoadingPointLoadorder Point:{loadingPoint.Id} Order:{idLoadorder}");
        _context.Attach(loadingPoint);
        loadingPoint.IdLoadorder = idLoadorder;
        await _context.SaveChangesAsync();
    }


    public async Task<List<Contingent>> GetActiveContingents(long idLocation,
        long? idDebitor, long idMaterial, DateTime? date)
    {
        // with ContingentSilo, Silo, LoadingPoint
        string day = date != null ? ((DateTime)date).ToString("yyyy-MM-dd") : "null";
        _log.Debug($"GetActiveContingents IdDeb:{idDebitor ?? 0} IdMat:{idMaterial} Day:{day}");
        var query = from cs in _context.Contingent
                    .Include(csi => csi.ContingentSilo)
                        .ThenInclude(csi => csi.IdSiloNavigation)
                    .Include(cs => cs.IdLoadingPointNavigation)
                    where cs.IdLocation == idLocation
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
