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
using Quva.Services.Interfaces;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading;
using Quva.Services.Loading.Interfaces;
using Quva.Services.Mapping;
using Quva.Services.Services.Shared;
using Quva.Services.Tests;
using SapTransfer.Services.Shared;
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
        if (args.Length > 0)
        {
            Log.Debug("args: " + string.Join(',', args));
        }
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

        //My Services:
        builder.Services.AddSingleton<IDeviceService, DeviceService>();
        builder.Services.AddScoped<ILocationParameterService, LocationParameterService>();
        builder.Services.AddScoped<IAgreementsService, AgreementsService>();
        builder.Services.AddScoped<ILoadingDbService, LoadingDbService>();
        builder.Services.AddScoped<IBasetypeService, BasetypeService>();
        builder.Services.AddScoped<ILoadOrderService, LoadOrderService>();
        builder.Services.AddScoped<ILoadingService, LoadingService>();
        builder.Services.AddScoped<ILoadInfoService, LoadInfoService>();


        var app = builder.Build();
        Log.Information("added Service");

        // Testmenü:
        while (true) 
        {
            Console.WriteLine("### Startmenü ###");
            Console.WriteLine("1 = Test DeviceService");
            Console.WriteLine("2 = Test Locationparameter");
            Console.WriteLine("3 = Test CustomerAgreements");
            Console.WriteLine("4 = Test LoadingService");
            Console.WriteLine("sonst = Ende");
            ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
            Console.WriteLine("");
            if (key.KeyChar == '1')
            {
                var svc = app.Services.GetRequiredService<IDeviceService>();
                var testsvc = new TestDeviceService(svc);
                testsvc.Menu();
            }
            else if (key.KeyChar == '2')
            {
                var svc = app.Services.GetRequiredService<ILocationParameterService>();
                var testobj = new TestLocationParameter(svc);
                testobj.Menu().GetAwaiter().GetResult(); 
            }
            else if (key.KeyChar == '3')
            {
                var svc = app.Services.GetRequiredService<IAgreementsService>();
                var testobj = new TestCustomerAgreement(svc);
                testobj.Menu().GetAwaiter().GetResult();
            }
            else if (key.KeyChar == '4')
            {
                var svc = app.Services.GetRequiredService<ILoadingService>();
                var testobj = new TestLoadingService(svc);
                testobj.Menu().GetAwaiter().GetResult();
            }
            else
            {
                break;
            }
        }

    }
}

