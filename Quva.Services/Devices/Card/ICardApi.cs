namespace Quva.Services.Devices.Card;

public interface ICardApi
{
    int PollInterval { get; set; }
    Task<CardData> CardCommand(string command);
}

public class CardData : DeviceData
{
    public CardData(string deviceCode, string command) : base(deviceCode, command)
    {
    }

    public string? CardNumber { get; set; }
    public DateTime? ReadDate { get; set; }
    public bool Dispenced { get; set; } = false; //wurde gespendet
    public CardStatus Status { get; set; }
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
    Timeout = 4 //keine Verbindung
}