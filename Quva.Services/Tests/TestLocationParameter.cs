using Microsoft.IdentityModel.Tokens;
using Quva.Services.Interfaces.Shared;

namespace Quva.Services.Tests;

public class TestLocationParameter
{
    private readonly ILocationParameterService _locationParameterService;

    public TestLocationParameter(ILocationParameterService locationParameterService)
    {
        _locationParameterService = locationParameterService;
    }

    public long IdLocation = 100000009;  //HOH

    public async Task Menu()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("### Test LocationParameter ###");
                Console.WriteLine($"1 = set idLocation (default = HOH = {IdLocation})");
                Console.WriteLine("2 = GetParameter");
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
                    Console.WriteLine("Gruppe.Key:");
                    var s1 = Console.ReadLine();
                    Console.WriteLine("IdPlant:");
                    var s2 = Console.ReadLine();
                    long? plant = s2.IsNullOrEmpty() ? null : long.Parse(s2!);
                    string s2Disp = s2.IsNullOrEmpty() ? "*" : s2!;
                    // für Controller:
                    var val = await _locationParameterService.GetParameter(IdLocation, s1!, plant);
                    Console.WriteLine($"{s1}.{s2Disp}={val}");
                }
                else
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }



}
