using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

/// <summary>
/// Simple Card Reader only waits for Number
/// </summary>
public class CardReader : ComProtocol, ICardApi
{
    private readonly ComDevice device;

    public CardReader(string deviceCode, ComDevice device) : base(deviceCode, device.ComPort)
    {
        this.device = device;

        Description = ReaderDescription;

        OnAnswer += ReaderAnswer;
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

    public event EventHandler<CardDataArgs>? OnCommandAnswer;
    private void DoCommandAnswer(CardDataArgs e) => OnCommandAnswer?.Invoke(this, e);
    private CardData? cardData;

    public string[] ReaderDescription = new string[]
{
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "T:0",     //ewig warten
          "A:255:6"    //ohne Endezeichen
};

    #region Commands

    public async Task<CardData> Read()
    {
        cardData = new CardData(device.Code, CardCommands.Read.ToString());
        var tel = await RunTelegram(cardData, "read");
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
        string inStr = System.Text.Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (data.Command == CardCommands.Read.ToString())
        {
            data.CardNumber = inStr;
        }
    }


    #endregion

}
