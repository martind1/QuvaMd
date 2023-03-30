using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quva.Services.Devices.Data;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Services.Shared;
using Serilog;
using Serilog.Debugging;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? host { get; private set; }
    public static IConfiguration? configuration { get; private set; }

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

        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => { ConfigureServices(context.Configuration, services); })
            .Build();
    }

    private void ConfigureServices(IConfiguration configuration,
        IServiceCollection services)
    {
        services.AddSingleton<IDataService, DataService>();
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