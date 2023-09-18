using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Services.Devices.Sps;
using Serilog;
using System.Text;
using System.Text.Json;

namespace Quva.Services.Devices.ComPort;

public class SqlPort : IComPort
{
    private readonly SqlParameter _sqlParameter;
    private readonly ILogger _log;

    public SqlPort(ComDevice device) : this(device.Code, device.Device.Paramstring ?? string.Empty)
    {
    }

    public SqlPort(string deviceCode, string paramString)
    {
        _log = Log.ForContext(GetType());
        DeviceCode = deviceCode; //for Debug Output
        ComParameter = new ComParameter();
        _sqlParameter = new SqlParameter();
        SetParamString(paramString);
    }

    public string DeviceCode { get; }
    public PortType PortType { get; } = PortType.Sql;
    public ComParameter ComParameter { get; set; }
    public bool DirectMode { get; } = true;
    public uint Bcc { get; set; }

    public bool IsConnected()
    {
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        //nothing todo here
        await Task.Delay(0);
        GC.SuppressFinalize(this);  //CA1816: GC.SuppressFinalize korrekt aufrufen
    }

    // Setzt Parameter
    public void SetParamString(string paramstring)
    {
        _sqlParameter.LoadingPoint = int.Parse(paramstring);
    }

    // Object als Parameter
    public void SetParameter(object parameter)
    {
        _sqlParameter.ServiceScopeFactory = (IServiceScopeFactory)parameter;
    }


    public async Task OpenAsync()
    {
        //nothing todo here
        await Task.Delay(0);
    }

    public async Task CloseAsync()
    {
        //nothing todo here
        await Task.Delay(0);
    }

    #region input/output

    public async Task ResetAsync()
    {
        //nothing todo here
        await Task.Delay(0);
    }

    public async Task<int> InCountAsync(int WaitMs)
    {
        //0 important for ComProtocol.ReadDataAsync!
        return await Task.FromResult(0);
    }

    public async Task<int> ReadAsync(ByteBuff buffer)
    {
        ArgumentNullException.ThrowIfNull(_sqlParameter.ServiceScopeFactory);
        using var scope = _sqlParameter.ServiceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QuvaContext>();

        var query = from d in context.LoadorderHead
                .Include(p => p.LoadorderPart)
                .Include(n => n.IdLoadingPointNavigation)
                .AsNoTracking()
                    where d.IdLoadingPointNavigation.LoadingNumber == _sqlParameter.LoadingPoint
                      && d.IdLoadingPointNavigation.IdLoadorder == d.Id
                    select d;
        var value = await query.FirstOrDefaultAsync();
        buffer.Cnt = 0;
        if (value != null)
        {
            // to Transfer Object:
            SpsData data = new("SqlPort", "ReadAsync");
            data.FromLoadorder(value);
            buffer.Buff = JsonSerializer.SerializeToUtf8Bytes(data);
            buffer.Cnt = buffer.Buff.Length;
        }
        _log.Debug($"[{DeviceCode}] SqlPort.ReadAsync Cnt:{buffer.Cnt} Buff:{Encoding.UTF8.GetString(buffer.Buff, 0, buffer.Cnt)}");
        return await Task.FromResult(buffer.Cnt);
    }

    public async Task<bool> WriteAsync(ByteBuff buffer)
    {
        //nothing to write
        return await Task.FromResult(true);
    }

    public async Task FlushAsync()
    {
        //nothing to flush
        await Task.Delay(0);
    }

    #endregion input/output
}

public class SqlParameter
{
    public int LoadingPoint { get; set; }
    public IServiceScopeFactory? ServiceScopeFactory { get; set; }
}