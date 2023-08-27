using Quva.Database.Models;
using Quva.Services.Interfaces;
using Quva.Services.Interfaces.Shared;
using SapTransfer.Services.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public async Task Menu()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("### Test LoadingService ###");
                Console.WriteLine($"1 = set idLocation (default = HOH = {IdLocation})");
                Console.WriteLine($"2 = GetBasetypeSilosAll");
                Console.WriteLine("sonst = Ende");
                ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
                Console.WriteLine("");
                if (key.KeyChar == '1')
                {
                    Console.WriteLine("IdLocation:");
                    var s1 = Console.ReadLine();
                    IdLocation = long.Parse(s1 ?? "100000009");
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
