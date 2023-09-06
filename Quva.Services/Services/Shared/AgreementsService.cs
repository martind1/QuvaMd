using Microsoft.EntityFrameworkCore;
using Quva.Database.Models;
using Quva.Services.Enums;
using Quva.Services.Interfaces.Shared;
using Serilog;
using System.Globalization;
using PackagingTypeValues = Quva.Services.Enums.PackagingTypeValues;
using TransportTypeValues = Quva.Services.Enums.TransportTypeValues;

namespace SapTransfer.Services.Shared;

public class AgreementsService : IAgreementsService
{
    private readonly ILogger _log;
    private readonly QuvaContext _context;

    public AgreementsService(QuvaContext context)
    {
        _log = Log.ForContext(GetType());
        _context = context;
    }

    private readonly Dictionary<long, ICustomerAgreements> _cachedCustomerAgreements = new();

    public async Task<ICustomerAgreements> GetAgreementsByDebitorMaterial(long idLocation, long? idDebitor, long? idMaterial)
    {
        ICustomerAgreements result;
        var filter = new AgreementsFilter()
        {
            idMaterials = idMaterial == null ? new() :
                new List<long>() { idMaterial ?? 0 },
            idGoodsRecipient = idDebitor,
            idInvoiceRecipient = idDebitor,
            idCarrier = idDebitor,
            validDate = DateTime.Now.Date,
    };
        result = await GetAgreementsInternal(idLocation, filter);
        return result;
    }

    public async Task<ICustomerAgreements> GetAgreementsByDeliveryId(long idDeliveryHead)
    {
        ICustomerAgreements? cachedResult;
        if (idDeliveryHead == 0)
        {
            _log.Information($"CustomerAgreements.ClearCache");
            _cachedCustomerAgreements.Clear();
            cachedResult = new CustomerAgreements(_log);
            return cachedResult;
        }
        // Cache:
        if (_cachedCustomerAgreements.TryGetValue(idDeliveryHead, out cachedResult))
        {
            return cachedResult;
        }

        ICustomerAgreements result;
        var filter = new AgreementsFilter();

        //DeliveryHead + ~Order + Plant + SalesOrg + ~OrderDebitor + ~Position + ~OrderPosition + Unit
        var deliveryHead = await FindDelivery(idDeliveryHead);

        long idLocation = deliveryHead.DeliveryOrder!.IdPlantNavigation.IdLocation;
        filter.idPlant = deliveryHead.DeliveryOrder!.IdPlant;
        filter.packagingType = (PackagingTypeValues)deliveryHead.DeliveryOrder!.IdShippingMethodNavigation.PackagingType;
        filter.transportType = (TransportTypeValues)deliveryHead.DeliveryOrder!.IdShippingMethodNavigation.TransportType;
        filter.validDate = deliveryHead.DeliveryDate;

        foreach (var delPos in deliveryHead.DeliveryPosition)
        {
            var materialShort = delPos.DeliveryOrderPosition!.MaterialShortName;
            var material = await GetMaterialByCode(materialShort);
            if (material != null)
            {
                filter.idMaterials.Add(material.Id);
            }
            else
            {
                _log.Warning($"GetCustomerAgreementsByDelivery({idDeliveryHead}) Material not found ({materialShort})");
            }
        }

        foreach (var deb in deliveryHead.DeliveryOrder.DeliveryOrderDebitor)
        {
            switch ((OrderDebitorRole)deb.Role)
            {
                case OrderDebitorRole.GoodsRecipient:
                    filter.idCountry = deb.IdCountry;
                    filter.idGoodsRecipient = deb.Id;
                    break;
                case OrderDebitorRole.InvoiceRecipient:
                    filter.idInvoiceRecipient = deb.Id;
                    break;
                case OrderDebitorRole.ForwardingAgent:
                    filter.idCarrier = deb.Id;
                    break;
            }
        }
        result = await GetAgreementsInternal(idLocation, filter);
        _cachedCustomerAgreements[idDeliveryHead] = result;
        return result;
    }

    public async Task<ICustomerAgreements> GetAgreementsByFilter(long idLocation, AgreementsFilter filter)
    {
        ICustomerAgreements result;
        result = await GetAgreementsInternal(idLocation, filter);
        return result;
    }

    private async Task<ICustomerAgreements> GetAgreementsInternal(
        long idLocation,
        AgreementsFilter filter)
    {
        // valid Agreements
        var allAgreements = await GetAllCustomerAgreements(idLocation, true);  //only active
        var agreements = new List<CustomerAgreement>();
        foreach (var agr in allAgreements)
        {
            bool b1 = agr.IdPlant == null || filter.idPlant == null || agr.IdPlant == filter.idPlant;
            bool b2 = agr.IdMaterial == null || filter.idMaterials == null || filter.idMaterials.Contains(agr.IdMaterial.Value);
            bool b3 = agr.IdCountry == null || filter.idCountry == null || agr.IdCountry == filter.idCountry;

            bool b4 = agr.IdDebitor == null || (agr.FlagGoodsRecipient && agr.IdDebitor == filter.idGoodsRecipient);
            bool b5 = agr.IdDebitor == null || (agr.FlagInvoiceRecipient && agr.IdDebitor == filter.idInvoiceRecipient);
            bool b6 = agr.IdDebitor == null || (agr.FlagCarrier && agr.IdDebitor == filter.idCarrier);

            bool b7 = agr.PackagingType == (int)PackagingTypeValues.All || filter.packagingType == null ||
                agr.PackagingType == (int)filter.packagingType;
            bool b8 = agr.TransportType == (int)TransportTypeValues.All || filter.transportType == null ||
                agr.TransportType == (int)filter.transportType;

            bool b9 = agr.ValidFrom == null || filter.validDate == null ||
                agr.ValidFrom.Value.Date <= filter.validDate.Value.Date;
            bool b10 = agr.ValidTo == null || filter.validDate == null ||
                agr.ValidTo.Value.Date >= filter.validDate.Value.Date;

            if (b1 && b2 && b3 && (b4 || b5 || b6) && b7 && b8 && b9 && b10)
            {
                agreements.Add(agr);
            }
        }

        // unique Parameters of valid Agreements
        var optionParameters = new Dictionary<long, VCustomerAgrParameter>();
        foreach (var a in agreements)
        {
            long idAgreement = a.Id;
            var agrParameters = await GetVCustomerAgrParameters(idAgreement);
            foreach (var agrParameter in agrParameters)
            {
                bool setFlag = true;
                if (optionParameters.TryGetValue(agrParameter.IdOption, out var agrParameterValue))
                {
                    setFlag = agrParameterValue.Value != agrParameterValue.DefaultValue;
                }
                if (setFlag)
                {
                    optionParameters[agrParameter.IdOption] = agrParameter;
                }
            }
        }
        List<VCustomerAgrParameter> parameters = optionParameters.Values.ToList();

        var defaultValues = await GetTypeAgreementOptions();

        var result = new CustomerAgreements(_log, agreements, parameters, defaultValues);
        return result;
    }

    private async Task<DeliveryHead> FindDelivery(long idDeliveryHead)
    {
        _log.Debug($"FindDelivery {idDeliveryHead}");
        //DeliveryHead + ~Order + Plant + SalesOrg + ShippingMethod + ~OrderDebitor + ~Position + ~OrderPosition + Unit
        var query = from delhdr in _context.DeliveryHead
                    .Include(delhdr => delhdr.DeliveryOrder)
                        .ThenInclude(delord => delord!.IdPlantNavigation)  //(1)
                    .Include(delhdr => delhdr.DeliveryOrder)
                        .ThenInclude(delord => delord!.IdSalesOrganizationNavigation)  //(1)
                    .Include(delhdr => delhdr.DeliveryOrder)
                        .ThenInclude(delord => delord!.IdShippingMethodNavigation)  //(1)
                    .Include(delhdr => delhdr.DeliveryOrder)
                        .ThenInclude(delorddeb => delorddeb!.DeliveryOrderDebitor)  //(1)
                            .ThenInclude(delorddeb => delorddeb.IdCountryNavigation)
                    .Include(delhdr => delhdr.DeliveryPosition)
                        .ThenInclude(delpos => delpos.DeliveryOrderPosition)
                            .ThenInclude(delordpos => delordpos!.IdUnitNavigation)
                    where delhdr.Id == idDeliveryHead
                    select delhdr;
        //(1) https://github.com/dotnet/efcore/issues/17212 wg !

        DeliveryHead? value = await query.AsSplitQuery().FirstOrDefaultAsync();
        ArgumentNullException.ThrowIfNull(value, "Find DeliveryHead Id:" + idDeliveryHead);
        return await Task.FromResult(value);
    }

    private async Task<Material?> GetMaterialByCode(string code)
    {
        _log.Debug($"GetMaterialByCode");
        var query = from d in _context.Material
                    where d.Code == code
                    select d;
        Material? value = await query.FirstOrDefaultAsync();
        //long result = value == null ? 0 : value.Id;
        return await Task.FromResult(value);
    }

    private async Task<List<CustomerAgreement>> GetAllCustomerAgreements(long idLocation, bool? active)
    {
        _log.Debug($"GetCustomerAgreements {idLocation}");
        var query = from agr in _context.CustomerAgreement
                    where agr.IdLocation == idLocation && (active == null || agr.Active == active)
                    select agr;
        var result = await query.ToListAsync();
        return result;
    }

    private async Task<List<VCustomerAgrParameter>> GetVCustomerAgrParameters(long idAgreement)
    {
        _log.Debug($"GetVCustomerAgrParameters {idAgreement}");
        var query = from par in _context.VCustomerAgrParameter
                    where par.IdAgreement == idAgreement
                    select par;
        var result = await query.ToListAsync();
        return result;
    }

    private async Task<List<TypeAgreementOption>> GetTypeAgreementOptions()
    {
        _log.Debug($"GetTypeAgreementOptions");
        var query = from par in _context.TypeAgreementOption
                    select par;
        var result = await query.ToListAsync();
        return result;
    }
}

public record CustomerAgreements : ICustomerAgreements
{
    private readonly ILogger _log;
    public List<CustomerAgreement> Agreements { get; set; }
    public List<VCustomerAgrParameter> Parameters { get; set; }
    public List<TypeAgreementOption> DefaultValues { get; set; }

    public CustomerAgreements(ILogger log)
    {
        _log = log;
        Agreements = new();
        Parameters = new();
        DefaultValues = new();
    }

    public CustomerAgreements(ILogger log,
        List<CustomerAgreement> agreements,
        List<VCustomerAgrParameter> parameters,
        List<TypeAgreementOption> defaultValues)
    {
        _log = log;
        Agreements = agreements;
        Parameters = parameters;
        DefaultValues = defaultValues;
    }

    private void GetValueType(string code, out string? value, out DataTypeValues datatype)
    {
        // z.B. "MAX_BRUTTO"
        value = null;
        datatype = DataTypeValues.t_string;
        var agrParameter = Parameters.Where(par => par.OptionCode == code).FirstOrDefault();
        if (agrParameter == null)
        {
            var agrOption = DefaultValues.Where(par => par.Code == code).FirstOrDefault();
            if (agrOption != null)
            {
                value = agrOption.DefaultValue.Trim();
                datatype = (DataTypeValues)agrOption.Datatype;
            }
            else
            {
                _log.Error($"Unknown Agreement Option {code}");
            }
        }
        else
        {
            value = agrParameter.Value.Trim();  // " " -> ""
            datatype = (DataTypeValues)agrParameter.Datatype;
        }
    }

    public T GetParameter<T>(string code)
    {
        object result;
        GetValueType(code, out string? value, out DataTypeValues datatype);
        if (value == null)
        {
            throw new Exception($"Unknown Agreement Option {code}");
        }
        switch (datatype)
        {
            case DataTypeValues.t_string:
                result = value;
                break;
            case DataTypeValues.t_int:
                int intValue = int.Parse(value);
                result = intValue;
                break;
            case DataTypeValues.t_bool:
                bool boolValue = int.Parse(value) != 0;
                result = boolValue;
                break;
            case DataTypeValues.t_float:
                double doubleValue = double.Parse(value, CultureInfo.InvariantCulture);
                result = doubleValue;
                break;
            case DataTypeValues.t_date:
                DateTime dateValue;
                if (value.Length <= 10)
                {
                    dateValue = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else
                {
                    dateValue = DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                }
                result = dateValue;
                break;
            default:
                result = value;
                break;
        }
        if (typeof(T) == typeof(decimal))
        {
            result = Convert.ToDecimal(result);
        }
        return (T)result;
    }

}
