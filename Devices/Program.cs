using Quva.Devices.Card;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Quva.Devices.Display;
using Quva.Devices.Scale;
using Quva.Devices.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Quva.Devices.Cam;
using Microsoft.AspNetCore.Mvc;

namespace Quva.Devices
{
    internal class Program
    {
        public static IHost? host;

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
            Log.Error("\r\n");
            Log.Error("Initializing Serilog....");

            //Service dependency injection:
            //using 
                host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                    {
                        services.AddSingleton<IDeviceService, DeviceService>();
                        services.AddSingleton<IDataService, DataService>();
                    })
                .Build();
            Log.Information("added Service");

            // Service aufrufen:
            var testsvc = new TestDeviceService(host.Services);

            Task T = testsvc.Test1();
            //Task T1 = testsvc.Test5();
            //Task T = testsvc.Test6();
            T.Wait();  //warten bis Task beendet
            Console.WriteLine("waiting for key press");
            Console.ReadKey();  //warten auf Taste

            //testsvc.Test2();

        }
    }

    internal class TestDeviceService
    {
        public IDeviceService svc { get; set; }

        public TestDeviceService(IServiceProvider hostProvider)
        {
            using IServiceScope serviceScope = hostProvider.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            svc = provider.GetRequiredService<IDeviceService>();
        }

        /// <summary>
        /// klassische async Befehle
        /// </summary>
        /// <returns></returns>
        public async Task Test1()
        {
            Log.Information($"testsvc.Test1");

            //var data1 = await svc.ScaleStatus("HOH.FW1");
            //Log.Information($"Status Err:{data1.ErrorNr} Display:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");

            //var data2 = await svc.ScaleStatus("HOH.FW2");
            //Log.Information($"Status Err:{data2.ErrorNr} Display:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");

            //var data12 = await svc.ScaleRegister("HOH.FW1");
            //Log.Information($"Register Err:{data12.ErrorNr} Display:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");

            //var data3 = await svc.CardRead("HOH.TRANSP1");
            //Log.Information($"Read Err:{data3.ErrorNr} {data3.ErrorText} Card:{data3.CardNumber}");

            //var message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
            //var data4 = await svc.DisplayShow("HOH.DISP1", message);
            //Log.Information($"Display Err:{data4.ErrorNr} {data4.ErrorText} Msg:{data4.Message}");

            //var data5 = await svc.CamLoad("HOH.CAMS", 0);
            var T1 = svc.CamLoad("HOH.CAMS", 0);
            var T2 = svc.CamLoad("HOH.CAMS", 1);
            var T3 = svc.CamLoad("HOH.CAMS", 2);
            //var T4 = svc.CamLoad("HOH.CAMS", 3);
            await Task.WhenAll(T1, T2, T3);
            var data1 = await T1;
            var data2 = await T2;
            var data3 = await T3;
            //var data4 = await T4;
            Log.Information($"CamLoad0 Err:{data1.ErrorNr} {data1.ErrorText} Size:{data1.ImageSize}");
            ArgumentNullException.ThrowIfNull(data1.ImageBytes);
            File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM0.{data1.ImageFormat}", data1.ImageBytes);

            Log.Information($"CamLoad1 Err:{data2.ErrorNr} {data2.ErrorText} Size:{data2.ImageSize}");
            ArgumentNullException.ThrowIfNull(data2.ImageBytes);
            File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM1.{data2.ImageFormat}", data2.ImageBytes); 

            Log.Information($"CamLoad2 Err:{data3.ErrorNr} {data3.ErrorText} Size:{data3.ImageSize}");
            ArgumentNullException.ThrowIfNull(data3.ImageBytes);
            File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM2.{data3.ImageFormat}", data3.ImageBytes); 

            //Log.Information($"CamLoad3 Err:{data4.ErrorNr} {data4.ErrorText} Size:{data4.ImageSize}");
            //ArgumentNullException.ThrowIfNull(data4.ImageBytes);
            //File.WriteAllBytes(@$"C:\Temp\angular\logs\CAM3.{data4.ImageFormat}", data4.ImageBytes); 
        }

        /// <summary>
        /// Dauerschleife: Status, wenn 10++t dann Register
        /// </summary>
        public void Test2()
        {
            Log.Information($"testsvc.Test2");
            ScaleData d1;
            ScaleData d2;
            int registerTrigger = 10;
            do
            {
                var T1 = svc.ScaleStatus("HOH.FW1");
                d1 = T1.Result;
                Console.Write($"\r{d1.Display}          ");
                if (d1.Weight == registerTrigger)
                {
                    registerTrigger++;
                    var T2 = svc.ScaleRegister("HOH.FW1");
                    d2 = T2.Result;
                    Console.Write($"\r\n{d2.Display}\r\n");
                }
            } while (d1.Weight != 99);
        }

        /// <summary>
        /// mehrere async Tasks starten und später beenden
        /// </summary>
        public async Task Test3()
        {
            Log.Information($"testsvc.Test3");

            var T1 = svc.ScaleStatus("HOH.FW1");

            var T2 = svc.ScaleStatus("HOH.FW2");

            var T12 = svc.ScaleRegister("HOH.FW1");


            await Task.WhenAll(T1, T2, T12);

            var data1 = await T1;
            Log.Information($"Status 1 Err:{data1.ErrorNr} Display:{data1.Display} Weight:{data1.Weight} Unit:{data1.Unit}");
            var data2 = await T2;
            Log.Information($"Status 2 Err:{data2.ErrorNr} Display:{data2.Display} Weight:{data2.Weight} Unit:{data2.Unit}");
            var data12 = await T12;
            Log.Information($"Register 1 Err:{data12.ErrorNr} Display:{data12.Display} Eichnr:{data12.CalibrationNumber} Weight:{data12.Weight} Unit:{data12.Unit}");
            await svc.DisposeAsync();
        }

        /// <summary>
        /// Callback Card async Test
        /// </summary>
        public async Task Test4()
        {
            Log.Information($"testsvc.Test4");
            IResult result = await svc.CardReadStart("HOH.TRANSP1", MyCardRead);
            Log.Information($"testsvc.Test4 Started");
        }

        private void MyCardRead(CardData cardData)
        {
            Log.Information($"### CardRead {cardData.CardNumber} ###");
            if (cardData.CardNumber == "q")
            {
                _ = svc.CloseDevice(cardData.DeviceCode);
            }

        }

        /// <summary>
        /// Callback Scale async Test
        /// </summary>
        public async Task Test5()
        {
            Log.Information($"testsvc.Test5");
            IResult result = await svc.ScaleStatusStart("HOH.FW2", MyScaleStatus);
            Log.Information($"testsvc.Test5 Started");
        }

        private async void MyScaleStatus(ScaleData scaleData)
        {
            Log.Error($"### ScaleStatus {scaleData.Display} ###");
            if (scaleData.Weight == 5)
            {
                Log.Error($"### ScaleStatus 5 close {scaleData.DeviceCode} ###");
                _ = svc.CloseDevice(scaleData.DeviceCode);
                await Task.Delay(100);

                Log.Error($"### ScaleStatus 5 close HOH.DISP1 ###");
                _ = svc.CloseDevice("HOH.DISP1");
            }

        }

        /// <summary>
        /// Callback Display Test
        /// </summary>
        public async Task Test6()
        {
            Log.Information($"testsvc.Test6");
            //IResult result = await svc.DisplayShowStart("HOH.DISP1", MyShow);
            //ScaleDisplay:
            IResult result = await svc.DisplayShowScale("HOH.DISP1", "HOH.FW2");
            if (result == null) { }

            Log.Information($"testsvc.Test6 Started");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static void MyShow(DisplayData displayData)
        {
            displayData.Message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
        }
        //Log.Error($"### DisplayShow {ShowData.Message} ###");
        //aktuelle Uhrzeit

    }
}