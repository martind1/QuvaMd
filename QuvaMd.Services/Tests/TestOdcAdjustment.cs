using Microsoft.IdentityModel.Tokens;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Interfaces.WeighingOperation;
using Serilog;

namespace QuvaMd.Services.Tests;

public class TestOdcAdjustment
{
    private readonly IOdcAdjustmentQuantityService _odcAdjustmentQuantityService;
    private readonly IDeliveryBasetypeService _deliveryBasetypeService;
    private readonly ILogger _log;

    public TestOdcAdjustment(IOdcAdjustmentQuantityService locationParameterService, IDeliveryBasetypeService deliveryBasetypeService)
    {
        _log = Log.ForContext(GetType());
        _odcAdjustmentQuantityService = locationParameterService;
        _deliveryBasetypeService = deliveryBasetypeService;
    }

    public long IdLocation = 100000009;  //HOH
    public long IdDelivery = 100003008;

    public async Task Menu()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("### Test OdcAdjustmentQuantityService ###");
                Console.WriteLine($"1 = set idLocation ({IdLocation})");
                Console.WriteLine($"2 = UpdateQuantities");
                Console.WriteLine($"3 = set idDelivery ({IdDelivery})");
                Console.WriteLine($"4 = Get Basetypes By Delivery");
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
                    await _odcAdjustmentQuantityService.UpdateQuantities(IdLocation);
                    Console.WriteLine($"OK");
                }
                if (key.KeyChar == '3')
                {
                    Console.WriteLine("IdDelivery:");
                    var s1 = Console.ReadLine();
                    IdDelivery = long.Parse(s1!.Trim() == "" ? IdDelivery.ToString() : s1);
                    Console.WriteLine(IdDelivery);
                }
                else if (key.KeyChar == '4')
                {
                    var rows = await _deliveryBasetypeService.GetListByDelivery(IdDelivery);
                    _log.Error($"Pos Verkaufssorte Grundsorte Menge ME Silo Gruppe");
                    decimal sumDispatch = rows.First().QuantityDispatch;
                    decimal sumBase = 0;
                    foreach (var row in rows)
                    {

                        _log.Error($"{row.PositionNumber} {row.MaterialCodeDispatch} {row.MaterialCodeBase} {row.Quantity} {row.UnitCode} {row.SiloNumber} {row.ProductgroupName}");
                        sumBase += row.Quantity;
                    }
                    _log.Error($"Quantity Dispatch:{sumDispatch} Base:{sumBase}");
                    Console.WriteLine($"OK");
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
