using System.Text;

namespace Devices.Card;

/// <summary>
///     Simple Card Reader only waits for Number
/// </summary>
public class Reader : ComProtocol, ICardApi
{
    private readonly CardData _readData;


    public string[] ReaderDescription =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "T:0", //infinity wait
        "T2:200", //between character
        "A:255:6" //without delimiter
    };

    public Reader(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = ReaderDescription;

        OnAnswer = ReaderAnswer;

        _readData = new CardData(device.Code, CardCommands.Read.ToString());
    }

    public async Task<CardData> CardCommand(string command)
    {
        if (Enum.TryParse(command, out CardCommands cmd))
        {
            var data = cmd switch
            {
                CardCommands.Read => await Read(),
                _ => throw new NotImplementedException($"CardCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }

        throw new NotImplementedException($"CardCommand({command}) not implemented");
    }

    #region Commands

    public async Task<CardData> Read()
    {
        //_readData = new CardData(DeviceCode, CardCommands.Read.ToString());
        var tel = await RunTelegram(_readData, "read"); //command text egal
        if (tel.Error != 0)
        {
            _readData.ErrorNr = 99;
            _readData.ErrorText = tel.ErrorText;
        }

        return await Task.FromResult(_readData);
    }

    #endregion

    #region Callbacks

    private void ReaderAnswer(ComTelegram tel)
    {
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(ReaderAnswer));
        var inBuff = tel.InData;
        var inStr = Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (_readData.Command == CardCommands.Read.ToString()) _readData.CardNumber = inStr;
    }

    #endregion
}