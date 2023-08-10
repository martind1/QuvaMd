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

        // Testmenü:
        while (true) 
        {
            Console.WriteLine("### Startmenü ###");
            Console.WriteLine("1 = TestDeviceService");
            Console.WriteLine("2 = TestLoadorder");
            Console.WriteLine("sonst = Ende");
            ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
            if (key.KeyChar == '1')
            {
                var testsvc = new TestDeviceService(app.Services);
                testsvc.Menu();
            }
            else if (key.KeyChar == '2')
            {
                Console.WriteLine("TestLoadorder coming soon ..");
            }
            else
            {
                break;
            }
        }

    }
}

