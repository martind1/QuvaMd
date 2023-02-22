using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;

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

            //Service dependency injection:
            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                    {
                        services.AddSingleton<DeviceService>();
                    })
                .Build();
            Log.Information("added Service");

            // Service aufrufen:
            var testsvc = new TestDeviceService(host.Services);
            Task T = testsvc.Test();
            T.Wait();  //warten bis Task beendet
            //Console.ReadLine();  //alt: warten bis Tasks beendet
        }


    }

    internal class TestDeviceService
    {
        public DeviceService svc { get; set; }

        public TestDeviceService(IServiceProvider hostProvider)
        {
            using IServiceScope serviceScope = hostProvider.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            svc = provider.GetRequiredService<DeviceService>();
        }

        //public async Task Test(CancellationToken arg)
        public async Task Test()
        {
            Log.Information($"testsvc.Test");
            var data1 = await svc.ScaleStatus("W1");
            Log.Information($"Status:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");
            var data21 = await svc.ScaleStatus("W2");
            Log.Information($"Status:{data21.Display} Weight:{data21.Weight} Unit:{data21.Unit}");
            var data2 = await svc.ScaleRegister("W2");
            Log.Information($"Register:{data2.Display} Eichnr:{data2.CalibrationNumber} Weight:{data2.Weight} Unit:{data2.Unit}");
            await svc.DisposeAsync(); 
        }
    }
}