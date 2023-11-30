using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Quva.Model.Enums.Loading;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Interfaces.WeighingOperation;
using Quva.Services.Loading.Helper;
using Serilog;

namespace QuvaMd.Services.Tests;

public class TestLoadingService
{
    private readonly ILoadingService _loadingService;
    private readonly IDeliveryBasetypeService _deliveryBasetypeService;
    private readonly ILogger _log;

    public TestLoadingService(ILoadingService loadingService, IDeliveryBasetypeService deliveryBasetypeService)
    {
        _log = Log.ForContext(GetType());
        _loadingService = loadingService;
        _deliveryBasetypeService = deliveryBasetypeService;
    }

    public long IdLocation = 100000009;  //HOH
    public long IdDelivery = 100002870; //
    public long IdOrder = 100002586;
    public string VehicleNumber = "K-FC 300";

    public async Task Menu()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("### Test LoadingService ###");
                Console.WriteLine($"1 = set idLocation (HOH = {IdLocation})");
                Console.WriteLine($"2 = GetBasetypeSilosAll");
                Console.WriteLine($"3 = set idDelivery ({IdDelivery})");
                Console.WriteLine($"4 = GetBasetypeSilosByDelivery");
                Console.WriteLine($"5 = CreateLoadorder (10+10+10t)");
                Console.WriteLine($"6 = UpdateLoadorder+Activate (5+6t)");
                Console.WriteLine($"7 = FinishDeliveryLoadorder ({IdDelivery})");
                Console.WriteLine($"8 = GetLoadingInfoByDelivery({IdDelivery})");
                Console.WriteLine($"9 = GetLoadingInfoByOrder({IdOrder}, {VehicleNumber})");
                Console.WriteLine($"A = GetBasetypesByDelivery({IdDelivery})");
                Console.WriteLine("sonst = Ende");
                ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
                Console.WriteLine("");
                if (key.KeyChar == '1')
                {
                    Console.WriteLine("IdLocation:");
                    var s1 = Console.ReadLine();
                    IdLocation = long.Parse(s1!.Trim() == "" ? IdLocation.ToString() : s1);
                    Console.WriteLine(IdLocation);
                }
                else if (key.KeyChar == '2')
                {
                    var agr = await _loadingService.GetBasetypeSilosAll(IdLocation);
                    //foreach (var siloset in agr.SiloSets)
                    //{
                    //    _log.Error(siloset.ToString());
                    //}
                    BasetypeSilosView view = BasetypeSilosView.FromBasetypeSilos(agr);
                    _log.Error("BaseTypeSilos All:" + Environment.NewLine + view.ToCsv());
                    Console.WriteLine($"OK");
                }
                else if (key.KeyChar == '3')
                {
                    Console.WriteLine("IdDelivery:");
                    var s1 = Console.ReadLine();
                    IdDelivery = long.Parse(s1!.Trim() == "" ? IdDelivery.ToString() : s1);
                    Console.WriteLine(IdLocation);
                }
                else if (key.KeyChar == '4')
                {
                    var baseTypeSilos = await _loadingService.GetBasetypeSilosByDelivery(IdDelivery);

                    var view = BasetypeSilosView.FromBasetypeSilos(baseTypeSilos);

                    _log.Error($"BaseTypeSilos for Delivery {IdDelivery}:{Environment.NewLine}{view.ToCsv()}");
                    //_log.Error($"{string.Join(Environment.NewLine, baseTypeSilos.ErrorLines)}");
                    List<string> sl = baseTypeSilos.ErrorStringList(LanguageEnum.DE);
                    _log.Error($"Errors: {string.Join(Environment.NewLine, sl)}");
                    Console.WriteLine($"done");
                }
                else if (key.KeyChar == '5')
                {
                    LoadingParameter parameter = new()
                    {
                        IdLocation = IdLocation,
                        IdDelivery = IdDelivery,
                        TargetQuantity = 30,
                        PartQuantities = new List<decimal> { 10, 10, 10 },
                    };
                    //var loadingResult = await _loadingService.CreateLoadorder(parameter);
                    var loadingResult = await _loadingService.UpdateLoadorder(parameter);
                    _log.Error($"Created Loadorders IdDel:{IdDelivery}: IdLoas:{string.Join(", ", loadingResult.IdLoadorders)}");
                    _log.Error($"Loadingpoints: {string.Join(", ", loadingResult.LoadingPoints)}");

                    List<string> sl = loadingResult.ErrorStringList(LanguageEnum.DE);
                    _log.Error($"Errors: {string.Join(Environment.NewLine, sl)}");
                }
                else if (key.KeyChar == '6')
                {
                    LoadingParameter parameter = new()
                    {
                        IdLocation = IdLocation,
                        IdDelivery = IdDelivery,
                        TargetQuantity = 11,
                        PartQuantities = new List<decimal> { 5, 6 },
                    };
                    var loadingResult = await _loadingService.UpdateLoadorder(parameter);
                    _log.Error($"Updated Loadorders {IdDelivery}: {string.Join(", ", loadingResult.IdLoadorders)}");
                    _log.Error($"Loadingpoints: {string.Join(", ", loadingResult.LoadingPoints)}");

                    List<string> sl = loadingResult.ErrorStringList(LanguageEnum.DE);
                    _log.Error($"Errors: {string.Join(Environment.NewLine, sl)}");

                    long idLoadorder = loadingResult.IdLoadorders.FirstOrDefault();
                    await _loadingService.StartLoadorder(idLoadorder);
                    _log.Error($"Started");
                }
                else if (key.KeyChar == '7')
                {
                    await _loadingService.FinishDeliveryLoadorder(IdDelivery);
                    _log.Error($"Finished Loadorders {IdDelivery}");

                }
                else if (key.KeyChar == '8')
                {
                    LoadingInfo info = await _loadingService.GetLoadInfoByDelivery(IdDelivery);

                    _log.Error($"{IdDelivery}:{info}");
                    Console.WriteLine($"done");
                }
                else if (key.KeyChar == '9')
                {
                    LoadingInfo info = await _loadingService.GetLoadInfoByOrder(IdOrder, VehicleNumber);

                    _log.Error($"{IdOrder},{VehicleNumber}:{info}");
                    Console.WriteLine($"done");
                }
                else if (char.ToUpper(key.KeyChar) == 'A')
                {
                    var rows = await _deliveryBasetypeService.GetListByDelivery(IdDelivery);
                    var view = DeliveryBasetypeView.FromDeliveryBasetype(rows);

                    _log.Error($"BaseTypes for Delivery {IdDelivery}:{Environment.NewLine}{view.ToCsv()}");
                    Console.WriteLine($"done");
                }
                else
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message.ToString());
                _log.Error(ex, "");
            }
        }

    }
}
