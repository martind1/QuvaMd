using Microsoft.IdentityModel.Tokens;
using Quva.Services.Interfaces.Loading;
using Quva.Services.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuvaMd.Services.Tests;

public class TestCustomerAgreement
{
    private readonly IAgreementsService _customerAgreementService;

    public TestCustomerAgreement(IAgreementsService customerAgreementService)
    {
        _customerAgreementService = customerAgreementService;
    }

    public long IdLocation = 100000009;  //HOH
    public long IdDelivery = 100002857;

    public async Task Menu()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("### Test CustomerAgreement ###");
                Console.WriteLine($"1 = set idLocation (default = HOH = {IdLocation})");
                Console.WriteLine($"2 = set idDelivery (default = {IdDelivery})");
                Console.WriteLine("3 = GetParameter");
                Console.WriteLine("sonst = Ende");
                ConsoleKeyInfo key = Console.ReadKey(); //warten auf Taste
                Console.WriteLine("");
                if (key.KeyChar == '1')
                {
                    Console.WriteLine("IdLocation:");
                    var s1 = Console.ReadLine();
                    if (long.TryParse(s1, out var id)) IdLocation = id;
                    Console.WriteLine(IdLocation);
                }
                else if (key.KeyChar == '2')
                {
                    Console.WriteLine("IdDelivery:");
                    var s1 = Console.ReadLine();
                    if (long.TryParse(s1, out var id)) IdDelivery = id;
                    Console.WriteLine(IdDelivery);

                    var agr = await _customerAgreementService.GetAgreementsByDeliveryId(IdDelivery);

                    Console.WriteLine($"Agr:{agr.Agreements.Count} Par:{agr.Parameters.Count} Def:{agr.DefaultValues.Count}");
                    var l1 = agr.Agreements.Select(x => (x.Id, x.IdDebitor, x.IdMaterial)).ToList();
                    Console.WriteLine($"Agr (Id, IdDebitor, IdMaterial): ");
                    Console.WriteLine($"{string.Join(Environment.NewLine, l1)}");
                    var l2 = agr.Parameters.Select(x => (x.IdOptionNavigation.Code, x.ParameterValue)).ToList();
                    Console.WriteLine($"Par (Code, Value): ");
                    Console.WriteLine($"{string.Join(Environment.NewLine, l2)}");
                }
                else if (key.KeyChar == '3')
                {
                    Console.WriteLine("Key:");
                    var s1 = Console.ReadLine();

                    var agr = await _customerAgreementService.GetAgreementsByDeliveryId(IdDelivery);

                    Console.WriteLine($"Agr:{agr.Agreements.Count} Par:{agr.Parameters.Count} Def:{agr.DefaultValues.Count}");
                    var val = agr.GetParameter<string>(s1!);
                    Console.WriteLine($"{IdDelivery}.{s1}={val}");
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
