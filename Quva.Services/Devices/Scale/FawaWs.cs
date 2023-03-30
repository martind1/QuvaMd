using System.Globalization;
using System.Text;

namespace Quva.Services.Devices.Scale;

/// <summary>
///     Api for Winsocket ComSvr general truck scale
/// </summary>
public class FawaWs : ComProtocol, IScaleApi
{
    private readonly ScaleData _registerData;

    public string[] FawaWsDescription =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "P:100",
        "I:", //Clearinput
        "T:2000", //Stillstand
        "T2:0", //between character
        "B:", //HoleStatus=?
        "A:255:1" //HoleStatus=<Status>;<Gewicht>;<Meldung>
    };

    public FawaWs(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = FawaWsDescription;

        OnAnswer = FawaWsAnswer;

        StatusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
        _registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());
    }

    public ScaleData StatusData { get; set; }

    public async Task<ScaleData> ScaleCommand(string command)
    {
        if (Enum.TryParse(command, out ScaleCommands cmd))
        {
            var data = cmd switch
            {
                ScaleCommands.Status => await Status(),
                ScaleCommands.Register => await Register(),
                _ => throw new NotImplementedException($"ScaleCommands.{command} not implemented")
            };
            return await Task.FromResult(data);
        }

        throw new NotImplementedException($"ScaleCommand({command}) not implemented");
    }

    #region Callbacks

    private void FawaWsAnswer(ComTelegram tel)
    {
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(FawaWsAnswer));
        var data = (ScaleData)tel.AppData; //StatusData or _registerData
        var inStr = Encoding.ASCII.GetString(tel.InData.Buff, 0, tel.InData.Cnt);
        if (data.Command == ScaleCommands.Status.ToString())
        {
            _log.Debug($"[{DeviceCode}] FawaWsAnswer.Status:{inStr}");
            //HoleStatus=StatusStr;GewichtStr;Meldung
            var p = inStr.IndexOf('=');
            var sl = inStr[(p + 1)..]
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var StatusStr = sl[0];
            var GewichtStr = sl[1];
            var Meldung = sl[2];
            var StatusNr = int.Parse(StatusStr);

            data.Weight = float.Parse(GewichtStr, new CultureInfo("de-DE")); //Dezimalkomma!
            data.Unit = ScaleUnit.Ton;
            data.CalibrationNumber = 0;
            if (StatusNr == 0 || StatusNr == 1 || StatusNr == 2 || StatusNr == 5)
            {
                data.Status |= ScaleStatus.WeightOK;
                if (StatusNr == 1 || StatusNr == 5) data.Status |= ScaleStatus.Underload;
                if (StatusNr == 2) data.Status |= ScaleStatus.NoStandstill;
                data.Display = $"{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}";
                data.ErrorNr = 0;
            }
            else
            {
                data.Status |= ScaleStatus.NoWeight;
                data.Display = Meldung;
                data.ErrorNr = 1;
            }
        }

        else if (data.Command == ScaleCommands.Register.ToString())
        {
            _log.Debug($"[{DeviceCode}] FawaWsAnswer.Register:{inStr}");
            //ProtGewicht=StatusStr;GewichtStr;EichNrStr;Meldung
            var p = inStr.IndexOf('=');
            var sl = inStr[(p + 1)..]
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var StatusStr = sl[0];
            var GewichtStr = sl[1];
            var EichNrStr = sl[2];
            var Meldung = sl[3];
            var StatusNr = int.Parse(StatusStr);

            data.Weight = float.Parse(GewichtStr, new CultureInfo("de-DE")); //Dezimalkomma!
            data.Unit = ScaleUnit.Ton;
            data.CalibrationNumber = int.Parse(EichNrStr);
            if (StatusNr == 0)
            {
                data.Status |= ScaleStatus.WeightOK;
                data.Display = $"<{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}>";
            }
            else
            {
                data.Display = Meldung;
                data.Status |= ScaleStatus.WeightOK;
                if (StatusNr == 1 || StatusNr == 5) data.Status |= ScaleStatus.Underload;
                if (StatusNr == 2)
                    data.Status |= ScaleStatus.PositionError;
                else
                    data.Status |= ScaleStatus.NoWeight;
                data.ErrorNr = 1;
            }
        }
    }

    #endregion

    #region Commands

    public async Task<ScaleData> Status()
    {
        var tel = await RunTelegram(StatusData, "Holestatus=?");
        if (tel.Error != 0)
        {
            StatusData.ErrorNr = 99;
            StatusData.ErrorText = tel.ErrorText;
            StatusData.Display = tel.ErrorText; //"Error99";
        }

        return await Task.FromResult(StatusData);
    }

    public async Task<ScaleData> Register()
    {
        var tel = await RunTelegram(_registerData, "ProtGewicht=?");
        if (tel.Error != 0)
        {
            _registerData.ErrorNr = 99;
            _registerData.ErrorText = tel.ErrorText;
            _registerData.Display = tel.ErrorText; //"Error99";
        }

        return await Task.FromResult(_registerData);
    }

    #endregion
}