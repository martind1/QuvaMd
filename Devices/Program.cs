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
            var data1 = await svc.ScaleStatus("HOH.FW1");
            Log.Information($"Err:{data1.ErrorNr} Status:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");

            var data2 = await svc.ScaleStatus("HOH.FW2");
            Log.Information($"Err:{data2.ErrorNr} Status:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");

            var data12 = await svc.ScaleRegister("HOH.FW1");
            Log.Information($"Err:{data12.ErrorNr} Register:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");

            await svc.DisposeAsync(); 
        }
    }
}