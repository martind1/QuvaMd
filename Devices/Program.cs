using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Quva.Devices
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(
                msg => Console.WriteLine("Error from Serilog: {0}", msg));
            IConfiguration configSerilog = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configSerilog)
                .CreateLogger();
            Log.Information("Initializing Serilog....");
            //Console.ReadLine();

            //Service DI:
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                    {
                        services.AddSingleton<DeviceService>();
                    })
                .Build();
            Log.Information("added Service");
            //await host.RunAsync();

            // Service aufrufen:
            var svc = new TestService(host.Services);
        }

    }

    internal class TestService
    {
        public TestService(IServiceProvider hostProvider)
        {
            using IServiceScope serviceScope = hostProvider.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            DeviceService svc = provider.GetRequiredService<DeviceService>();

            var scale = svc.OpenScale("WA1");
            var data = scale.Status();
            Log.Information($"Status:{data.Display} Weight:{data.Weight} Unit:{data.Unit}");

            svc.CloseScale("WA1");
        }
    }
}