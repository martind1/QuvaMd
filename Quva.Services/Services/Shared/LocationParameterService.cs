using Microsoft.EntityFrameworkCore;
using Quva.Database.Models;
using Quva.Services.Enums;
using Quva.Services.Interfaces.Shared;
using Serilog;
using System.Globalization;

namespace SapTransfer.Services.Shared;

/// <summary>
/// Location Parameter, Enums for OptionGroups and Keys
/// </summary>
public class LocationParameterService : ILocationParameterService
{
    private readonly ILogger _log;
    private readonly QuvaContext _context;

    public LocationParameterService(QuvaContext context)
    {
        _log = Log.ForContext(GetType());
        _context = context;
    }

    private long _cachedIdLocation = 0;
    private List<LocationParameter> _cachedLocationParameters = new();

    private async Task<LocationParameter?> GetValue(long idLocation, string groupDotKey, long? idPlant)
    {
        // PseudoKey ohne Group: Cache entfernen:
        if (groupDotKey == "ClearCache")
        {
            _log.Information($"LocationParameter.ClearCache");
            _cachedIdLocation = 0;
            var dummy = new LocationParameter
            {
                Value = "OK",
                IdOptionKeyNavigation = new()
            };
            dummy.IdOptionKeyNavigation.Datatype = (int)DataTypeValues.t_string;
            return dummy;
        }

        // z.B. "DeliveryExport.CountOfDays"
        if (groupDotKey.Split('.').Length != 2)
        {
            _log.Error($"Syntaxerror. Must be Group.Key ({groupDotKey})");
            return null;
        }
        string groupName = groupDotKey.Split('.')[0];
        string keyName = groupDotKey.Split('.')[1];

        // Cache bzgl Location. Clear beim Wechsel der Location.
        if (_cachedIdLocation != idLocation)
        {
            _cachedIdLocation = idLocation;
            var query0 = from locpar in _context.LocationParameter
                         .Include(key => key.IdOptionKeyNavigation)
                            .ThenInclude(gru => gru.IdGroupNavigation)
                         where locpar.IdLocation == idLocation
                         select locpar;
            _cachedLocationParameters = await query0.ToListAsync();
        }

        var query = from locpar in _cachedLocationParameters
                    where locpar.IdOptionKeyNavigation.IdGroupNavigation.GroupName == groupName
                       && locpar.IdOptionKeyNavigation.KeyName == keyName
                       && locpar.IdPlant == idPlant
                    select locpar;
        var result = query.FirstOrDefault();
        // try with plant=null, so no error
        //if (result == null)
        //{
        //    _log.Error($"Unknown LocationOption {idLocation}.{groupDotKey}.{idPlant}");
        //}
        return result;
    }

    public async Task<T> GetParameter<T>(long idLocation, string groupDotKey, long? idPlant)
    {
        object result;
        LocationParameter? valueType = null;
        if (idPlant != null)
        {
            // named plant
            valueType = await GetValue(idLocation, groupDotKey, idPlant);
        }
        //other plants
        valueType ??= await GetValue(idLocation, groupDotKey, null);
        if (valueType == null)
        {
            throw new Exception($"Unknown LocationOption {idLocation}.{groupDotKey}.{idPlant}|null");
        }
        string value = valueType.Value;
        DataTypeValues datatype = (DataTypeValues)valueType.IdOptionKeyNavigation.Datatype;
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
                decimal doubleValue = decimal.Parse(value, CultureInfo.InvariantCulture);
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
        return (T)result;
    }

}

