using System.Text;

namespace Quva.Devices.Card;

/// <summary>
/// Simple Card Reader only waits for Number
/// </summary>
public class Reader : ComProtocol, ICardApi
{

    private readonly CardData _readData;

    public Reader(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = ReaderDescription;

        OnAnswer += ReaderAnswer;

        _readData = new CardData(device.Code, CardCommands.Read.ToString());
    }

    public async Task<CardData> CardCommand(string command)
    {
        if (Enum.TryParse(command, out CardCommands cmd))
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
        //_readData = new CardData(DeviceCode, CardCommands.Read.ToString());
        var tel = await RunTelegram(_readData, "read");  //command text egal
        if (tel.Error != 0)
        {
            _readData.ErrorNr = 99;
            _readData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(_readData);
    }

    #endregion

    #region Callbacks

    private void ReaderAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(ReaderAnswer));
        var inBuff = tel.InData;
        string inStr = Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (_readData.Command == CardCommands.Read.ToString())
        {
            _readData.CardNumber = inStr;
        }
    }


    #endregion

}
