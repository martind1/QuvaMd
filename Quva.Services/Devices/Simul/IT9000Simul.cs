using System.Text;

namespace Quva.Services.Devices.Simul;

/// <summary>
///     Systec IT9000Simul scale 
/// </summary>
public class IT9000Simul : ComProtocol, ISimulApi
{
    public SimulData _simulData { get; set; }
    private static readonly Random rnd = new();

    public string[] IT9000SimulDescription =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "T:0",      //infinity wait
        "T2:200",   //between character
        "A:10,>",  //simul command eg <RM>
        "B:",       //simul answer
    };


    public IT9000Simul(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = IT9000SimulDescription;

        OnAnswer = IT9000SimulAnswer;

        _simulData = new SimulData(device.Code, "Simul");

        ArgumentNullException.ThrowIfNull(device.Options);
    }


    public async Task<SimulData> SimulCommand()
    {
        var tel = await RunTelegram(_simulData, "SIMUL");
        if (tel.Error != 0)
        {
            _simulData.ErrorNr = 99;
            _simulData.ErrorText = tel.ErrorText;
        }

        return await Task.FromResult(_simulData);
    }

    private void IT9000SimulAnswer(ComTelegram tel)
    {
    }

    /// <summary>
    /// called from DeviceSimulator with Simul Data. Builds OutData
    /// </summary>
    /// <param name="tel"></param>
    /// <param name="simulData"></param>
    public static void OnIT9000Simul(ComTelegram tel, SimulData simulData)
    {
        string errorCode = simulData.ErrorNr.ToString("D2");  //0->00
        string moving = simulData.Stillstand ? "0" : "1";  
        string bruttoNegative = simulData.Negative ? "1" : "0";
        string scaleDate = DateTime.Now.ToString("dd.MM.yy");
        string scaleTime = DateTime.Now.ToString("HH:mm");
        string identNumber = string.Format("{0,4:0}", simulData.CalibrationNumber % 10000);  //Len=4
        string scaleNumber = "1";
        string gross = string.Format("{0,8:F0}", simulData.Weight);
        string tare = string.Format("{0,8:F0}", 0);
        string net = string.Format("{0,8:F0}", 0);
        string unit = string.Format("{0,2}", simulData.unitStr);
        string tareCode = "  ";
        string weighingRange = " ";
        string terminalNumber = "  1";
        string checkCharacter = string.Format("{0,8}", rnd.Next(10000, 99999));
        string outStr;

        if (errorCode != "00")
        {
            outStr = $"<{errorCode}>\r\n";
        }
        else
        {
            outStr =
             //IT6 Hobo 2020:
             //<000016.09.2015:21   01    2240       0    2240kg     1   30955>^M^J
             //IT9000 Melk 2014:
             //<000020.10.1412:22  801    1340       0    1340kg  02     63622>^M^J
             // FFSSDDDDDDDDZZZZZIIIIWBBBBBBBBTTTTTTTTNNNNNNNNEEOORTTTCCCCCCCC
             //01   5    0    5    0    5    0    5    0    5    0    5    0    5
             //          1         2         3         4         5         6
             //<000027.03.2321:18   01       0       0       0       1   83731>
             "<FFSSDDDDDDDDZZZZZIIIIWBBBBBBBBTTTTTTTTNNNNNNNNEEOORMMMCCCCCCCC>\r\n"
             .Replace("FF", errorCode)
             .Replace("SS", moving + bruttoNegative)
             .Replace("DDDDDDDD", scaleDate)
             .Replace("ZZZZZ", scaleTime)
             .Replace("IIII", identNumber)
             .Replace("W", scaleNumber)
             .Replace("BBBBBBBB", gross)
             .Replace("TTTTTTTT", tare)
             .Replace("NNNNNNNN", net)
             .Replace("EE", unit)
             .Replace("OO", tareCode)
             .Replace("R", weighingRange)
             .Replace("MMM", terminalNumber)
             .Replace("CCCCCCCC", checkCharacter);
        }
        tel.OutData.Buff = Encoding.ASCII.GetBytes(outStr);
        tel.OutData.Cnt = outStr.Length;
    }


}