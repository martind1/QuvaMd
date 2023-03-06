using Quva.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public interface ICardApi
{
    public Task<CardData> CardCommand(string command);

}

public class CardDataArgs : EventArgs
{
    public CardData Data { get; set; }

    public CardDataArgs(CardData cardData)
    {
        Data = cardData;
    }
}


public class CardData : DeviceData
{
    public string? CardNumber { get; set; }
    public DateTime? ReadDate { get; set; }
    public bool Dispenced { get; set; } = false;  //wurde gespendet
    public CardStatus Status { get; set; }

    public CardData(string deviceCode, string command) : base(deviceCode, command)
    {
    }
}

// Commands for Card Device:
public enum CardCommands
{
    None,
    Read,
    Dispence
}

//Statusflags for Card Device
[Flags]
public enum CardStatus
{
    CardAvailable = 1,
    DispencerError = 2,
    Timeout = 4          //keine Verbindung
}
