using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Mapping;
using Quva.Services.Services.Shared;
using Serilog;
using Serilog.Debugging;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Quva.Services;

internal class Program
{
    //public static IHost? host;
    private static string? User;
    private static string? Pw;
    private static string? Datasource;

    private static void Main(string[] args)
    {
        SelfLog.Enable(
            msg => Console.WriteLine("Error from Serilog: {0}", msg));
        IConfiguration configSerilog = new ConfigurationBuilder()
            //.AddJsonFile(@"c:\Keyfiles\quva-config.json", true, true)
            .AddJsonFile(@"Devices.json", true, true)
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configSerilog)
            .CreateLogger();
        Log.Error("\r\n");
        Log.Error("Initializing Serilog....");
        //var cs = Crypt.EncryptString("b14ca5898a4e4133bbce2ea2315a1916", "QUVATESTNEU");
        //Log.Error($"cs:{cs}");

        //Database:
        User = configSerilog.GetSection("database").GetSection("user").Value;
        Pw = configSerilog.GetSection("database").GetSection("pw").Value;
        Datasource = configSerilog.GetSection("database").GetSection("datasource").Value;
        var conn = new SqlConnectionStringBuilder
        {
            Password = Pw,
            DataSource = Datasource,
            UserID = User,
            PersistSecurityInfo = false
        };

        //Service dependency injection:
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IDeviceService, DeviceService>();
        builder.Services.AddDbContextPool<QuvaContext>(opt =>
                {
                    opt.EnableSensitiveDataLogging();  //Serilog
                    opt.UseOracle(conn.ConnectionString, opt =>
                    {
                        opt.UseOracleSQLCompatibility("11");
                    });
                });
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddMapster();

        builder.Host.UseSerilog();  //log sql
        var app = builder.Build();
        Log.Information("added Service");

        // Service aufrufen:
        //var testsvc = new TestDeviceService(host.Services);
        var testsvc = new TestDeviceService(app.Services);

        //var T = testsvc.TestPosition();
        var T = testsvc.TestModbus();
        //Task T1 = testsvc.Test5();
        //Task T2 = testsvc.Test6();
        //T.Wait(); //warten bis Task beendet
        Console.WriteLine("waiting for key press");
        Console.ReadKey(); //warten auf Taste
        //testsvc.Test2();
        testsvc.svc.DisposeAsync();
    }
}

internal class TestDeviceService
{
    public TestDeviceService(IServiceProvider hostProvider)
    {
        using var serviceScope = hostProvider.CreateScope();
        var provider = serviceScope.ServiceProvider;
        svc = provider.GetRequiredService<IDeviceService>();
    }

    public IDeviceService svc { get; set; }

    private string _oldDisplay = string.Empty;
    private ScaleStatus _oldScaleStatus;
    private long _oldSecond;

    public async Task TestModbus()
    {
        Log.Information("testsvc.TestModbus");

        var T1 = svc.ModbusWrite("HOH.WAGO", "BulbEntryGreen", "1");
        var T2 = svc.ModbusWrite("HOH.WAGO", "BulbExitGreen", "1");
        var T3 = svc.ModbusWrite("HOH.WAGO", "BulbExitRed", "1");
        await Task.WhenAll(T1, T2, T3);
        var data1 = await T1;
        var data2 = await T2;
        var data3 = await T3;
        Log.Information($"ModbusWrite Err1:{data1.ErrorNr} {data1.ErrorText} Err2:{data2.ErrorNr} {data2.ErrorText} Err3:{data3.ErrorNr} {data3.ErrorText}");


        //var T1 = svc.ModbusCommand("HOH.WAGO", ModbusCommands.ReadBlock.ToString(), "Inputs", "1");
        //var data1 = await T1;
        //Log.Information($"ModbusWrite Err1:{data1.ErrorNr} {data1.ErrorText}");
        //string v1 = svc.GetModbusValue("HOH.WAGO", "EntryIsOccupied");
        //string v2 = svc.GetModbusValue("HOH.WAGO", "ExitIsOccupied");
        //Log.Information($"ModbusRead V1.3:{v1} V2.4:{v2}");

        //var T1 = svc.ModbusReadStart("HOH.WAGO", MyModbusRead);
        //var T2 = svc.ModbusReadStart("HOH.WAGO", MyModbusRead);  //test doppelter Aufruf
        //var data1 = await T1;
        //var data2 = await T2;
        //Log.Error($"ModbusReadStart1:{data1}");
        //Log.Error($"ModbusReadStart2:{data2}");
    }

    private void MyModbusRead(ModbusData Data)
    {
        if (Data.changedBlocks.Contains("Inputs"))
        {
            Data.changedBlocks.Remove("Inputs");
            string v1 = svc.GetModbusValue("HOH.WAGO", "EntryIsOccupied");
            string v2 = svc.GetModbusValue("HOH.WAGO", "ExitIsOccupied");
            Log.Error($"### MyModbus 3:{v1} 4:{v2}");
        }
    }

    public async Task TestPosition() 
    {
        Log.Information("Test IT9000 + Modbus Wago Positioning");
        var T1 = await svc.ScaleStatusStart("HOH.FW1", MyScalePosition);

    }
    private void MyScalePosition(ScaleData scaleData)
    {
        if (_oldDisplay != scaleData.Display || _oldScaleStatus != scaleData.Status)
        {
            Log.Error($"### Scale {scaleData.Display} * Status {scaleData.Status:F} ###");
            _oldDisplay = scaleData.Display;
            _oldScaleStatus = scaleData.Status;
        }
        if (_oldSecond != DateTime.Now.ToFileTime() / 100000000)
        {
            _oldSecond = DateTime.Now.ToFileTime() / 100000000;
            var T2 = svc.ScaleRegister("HOH.FW1");
            ScaleData data = T2.GetAwaiter().GetResult();
            Log.Information($"### Register:{data.ErrorNr} {data.Display} C:{data.CalibrationNumber} S:{data.Status:F}");
        }
    }


    public async Task Test1()
    {
        Log.Information("testsvc.Test1");

        //var data1 = await svc.ScaleStatus("HOH.FW1");
        //Log.Information($"Status Err:{data1.ErrorNr} Display:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");

        //var data2 = await svc.ScaleStatus("HOH.FW2");
        //Log.Information($"Status Err:{data2.ErrorNr} Display:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");

        //var data12 = await svc.ScaleRegister("HOH.FW1");
        //Log.Information($"Register Err:{data12.ErrorNr} Display:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");

        //var data3 = await svc.CardRead("HOH.TRANSP1");
        //Log.Information($"Read Err:{data3.ErrorNr} {data3.ErrorText} Card:{data3.CardNumber}");

        //var message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
        ////var data4 = await svc.DisplayShow("HOH.DISP1", message);
        //var data4 = await svc.DisplayShow("HOH.DISP2", message);
        //Log.Information($"Display Err:{data4.ErrorNr} {data4.ErrorText} Msg:{data4.Message}");

        //var data5 = await svc.CamLoad("HOH.CAMS", 0);
        
        var T1 = svc.CamLoad("HOH.CAMS", 0);
        var T2 = svc.CamLoad("HOH.CAMS", 1);
        var T3 = svc.CamLoad("HOH.CAMS", 2);
        var T4 = svc.CamLoad("HOH.CAMS", 3);  //nicht vorhanden
        await Task.WhenAll(T1, T2, T3, T4);
        var data1 = await T1;
        var data2 = await T2;
        var data3 = await T3;
        var data4 = await T4;
        //var data4 = await T4;
        Log.Information($"CamLoad0 Err:{data1.ErrorNr} {data1.ErrorText} Size:{data1.ImageSize}");
        ArgumentNullException.ThrowIfNull(data1.ImageBytes);
        File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM0.{data1.ImageFormat}", data1.ImageBytes);

        Log.Information($"CamLoad1 Err:{data2.ErrorNr} {data2.ErrorText} Size:{data2.ImageSize}");
        ArgumentNullException.ThrowIfNull(data2.ImageBytes);
        File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM1.{data2.ImageFormat}", data2.ImageBytes);

        Log.Information($"CamLoad2 Err:{data3.ErrorNr} {data3.ErrorText} Size:{data3.ImageSize}");
        ArgumentNullException.ThrowIfNull(data3.ImageBytes);
        File.WriteAllBytes($@"C:\Temp\angular\logs\CAM2.{data3.ImageFormat}", data3.ImageBytes);

        //Log.Information($"CamLoad3 Err:{data4.ErrorNr} {data4.ErrorText} Size:{data4.ImageSize}");
        //ArgumentNullException.ThrowIfNull(data4.ImageBytes);
        //File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM3.{data4.ImageFormat}", data4.ImageBytes); 
        
    }

    /// <summary>
    ///     Dauerschleife: Status, wenn 10++t dann Register
    /// </summary>
    public void Test2()
    {
        Log.Information("testsvc.Test2");
        ScaleData d1;
        var registerTrigger = 10;
        do
        {
            var T1 = svc.ScaleStatus("HOH.FW1");
            d1 = T1.Result;
            Console.Write($"\r{d1.Display}          ");
            if (d1.Weight == registerTrigger)
            {
                registerTrigger++;
                var T2 = svc.ScaleRegister("HOH.FW1");
                var d2 = T2.Result;
                Console.Write($"\r\n{d2.Display}\r\n");
            }
        } while (d1.Weight != 99);
    }

    /// <summary>
    ///     mehrere async Tasks starten und später beenden
    /// </summary>
    public async Task Test3()
    {
        Log.Information("testsvc.Test3");

        var T1 = svc.ScaleStatus("HOH.FW1");

        var T2 = svc.ScaleStatus("HOH.FW2");

        var T12 = svc.ScaleRegister("HOH.FW1");


        await Task.WhenAll(T1, T2, T12);

        var data1 = await T1;
        Log.Information(
            $"Status 1 Err:{data1.ErrorNr} Display:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");
        var data2 = await T2;
        Log.Information(
            $"Status 2 Err:{data2.ErrorNr} Display:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");
        var data12 = await T12;
        Log.Information(
            $"Register 1 Err:{data12.ErrorNr} Display:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");
        await svc.DisposeAsync();
    }

    /// <summary>
    ///     Callback Card async Test
    /// </summary>
    public async Task Test4()
    {
        Log.Information("testsvc.Test4");
        var result = await svc.CardReadStart("HOH.TRANSP1", MyCardRead);
        Log.Information("testsvc.Test4 Started");
    }

    private void MyCardRead(CardData cardData)
    {
        Log.Information($"### CardRead {cardData.CardNumber} ###");
        if (cardData.CardNumber == "q") _ = svc.CloseDevice(cardData.DeviceCode);
    }

    /// <summary>
    ///     Callback Scale async Test
    /// </summary>
    public async Task Test5()
    {
        Log.Information("testsvc.Test5");
        var result = await svc.ScaleStatusStart("HOH.FW1", MyScaleStatus);
        Log.Information("testsvc.Test5 Started");
    }

    private async void MyScaleStatus(ScaleData scaleData)
    {
        if (_oldDisplay != scaleData.Display)
        {
            if (scaleData.ErrorNr == 0)
                _oldDisplay = scaleData.Display;
            Log.Error($"### ScaleStatus {scaleData.Display} ###");
        }
        if (scaleData.Weight == 5)
        {
            Log.Error($"### ScaleStatus 5 close {scaleData.DeviceCode} ###");
            _ = svc.CloseDevice(scaleData.DeviceCode);
            await Task.Delay(100);

            Log.Error("### ScaleStatus 5 close HOH.DISP1 ###");
            _ = svc.CloseDevice("HOH.DISP1");
        }
    }

    /// <summary>
    ///     Callback Display Test
    /// </summary>
    public async Task Test6()
    {
        Log.Information("testsvc.Test6");
        //Microsoft.AspNetCore.Http.IResult result = await svc.DisplayShowStart("HOH.DISP2", MyShow);
        //ScaleDisplay:
        var result = await svc.DisplayShowScale("HOH.DISP2", "HOH.FW1");
        if (result == null)
        {
        }

        Log.Information("testsvc.Test6 Started");
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private static void MyShow(DisplayData displayData)
    {
        displayData.Message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
    }
    //Log.Error($"### DisplayShow {ShowData.Message} ###");
    //aktuelle Uhrzeit
}