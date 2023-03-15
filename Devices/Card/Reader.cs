using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quva.Devices;
using Quva.Devices.Data;

namespace Quva.Devices.Card;

/// <summary>
/// Simple Card Reader only waits for Number
/// </summary>
public class Reader : ComProtocol, ICardApi
{

    private CardData cardData { get; set; }

    public Reader(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = ReaderDescription;

        OnAnswer += ReaderAnswer;

        cardData = new CardData(device.Code, CardCommands.Read.ToString());
    }

    public async Task<CardData> CardCommand(string command)
    {
        if (Enum.TryParse<CardCommands>(command, out CardCommands cmd))
        {
            CardData data = cmd switch
            {
                CardCommands.Read => await Read(),
                _ => throw new NotImplementedException($"CardCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }
        else
        {
            throw new NotImplementedException($"CardCommand({command}) not implemented");
        }
    }



    public string[] ReaderDescription = new string[]
{
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "T:0",     //infinity wait
          "T2:200",  //between character
          "A:255:6"  //without delimiter
};

    #region Commands

    public async Task<CardData> Read()
    {
        //cardData = new CardData(DeviceCode, CardCommands.Read.ToString());
        var tel = await RunTelegram(cardData, "read");  //command text egal
        if (tel.Error != 0)
        {
            cardData.ErrorNr = 99;
            cardData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(cardData);
    }

    #endregion

    #region Callbacks

    private void ReaderAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(ReaderAnswer));
        CardData data = (CardData)tel.AppData;
        var inBuff = tel.InData;
        string inStr = Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (data.Command == CardCommands.Read.ToString())
        {
            data.CardNumber = inStr;
        }
    }


    #endregion

}
