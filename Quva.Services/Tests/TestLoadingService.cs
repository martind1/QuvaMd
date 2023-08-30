﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Quva.Services.Interfaces.Shared;
using Quva.Services.Loading;
using Serilog;

namespace Quva.Services.Tests;

public class TestLoadingService
{
    private readonly ILoadingService _loadingService;
    private readonly ILogger _log;

    public TestLoadingService(ILoadingService loadingService)
    {
        _log = Log.ForContext(GetType());
        _loadingService = loadingService;
    }

    public long IdLocation = 100000009;  //HOH
    public long IdDelivery = 100002695; //Kunw=700002, Mara=V3004S-L

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
                Console.WriteLine($"5 = CreateLoadorder (10t)");
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
                    var agr = await _loadingService.GetBasetypeSilosAllView(IdLocation);
                    //foreach (var siloset in agr.SiloSets)
                    //{
                    //    _log.Information(siloset.ToString());
                    //}
                    _log.Information("BaseTypeSilos All:" + Environment.NewLine + agr.ToCsv());
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

                    _log.Information($"BaseTypeSilos for Delivery {IdDelivery}:{Environment.NewLine}{view.ToCsv()}");
                    _log.Information($"{string.Join(Environment.NewLine, baseTypeSilos.ErrorLines)}");
                    Console.WriteLine($"done");
                }
                else if (key.KeyChar == '5')
                {
                    LoadingParameter parameter = new()
                    {
                        IdLocation = IdLocation,
                        IdDelivery = IdDelivery,
                        TargetQuantity = 10
                    };
                    var loadingResult = await _loadingService.CreateLoadorder(parameter);
                    _log.Information($"Created Loadorders {IdDelivery}: {string.Join(", ", loadingResult.IdLoadorders)}");

                    _log.Information($"Errors: {string.Join(Environment.NewLine, loadingResult.ErrorLines)}");
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
