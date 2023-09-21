using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quva.Database.Models;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Mapping;
using Quva.Services.Services.Shared;
using Serilog;
using Serilog.Debugging;
using System;
using System.Windows;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? host { get; private set; }
    public static IConfiguration? configuration { get; private set; }
    private static string? User;
    private static string? Pw;
    private static string? Datasource;

    public App()
    {
        SelfLog.Enable(
            msg => Console.WriteLine("Error from Serilog: {0}", msg));
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        Log.Information("\r\n");
        Log.Information("Initializing Serilog....");

        //Database:
        User = configuration.GetSection("database").GetSection("user").Value;
        Pw = configuration.GetSection("database").GetSection("pw").Value;
        Datasource = configuration.GetSection("database").GetSection("datasource").Value;

        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => { ConfigureServices(context.Configuration, services); })
            .Build();
    }

    private void ConfigureServices(IConfiguration configuration,
        IServiceCollection services)
    {
        //Database:
        var conn = new SqlConnectionStringBuilder
        {
            Password = Pw,
            DataSource = Datasource,
            UserID = User,
            PersistSecurityInfo = false
        };
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddDbContextPool<QuvaContext>(opt =>
        {
            opt.EnableSensitiveDataLogging();  //Serilog
            opt.UseOracle(conn.ConnectionString, opt =>
            {
                opt.UseOracleSQLCompatibility("11");
            });
        });
        services.AddSingleton(Log.Logger);
        services.AddMapster();


        services.AddSingleton<IDeviceService, DeviceService>();

        services.AddSingleton<MainWindow>();
        services.AddTransient<DeviceTestWindow>();
        services.AddTransient<ScaleIT9000Window>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(host);
        await host.StartAsync();

        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(host);
        using (host)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}