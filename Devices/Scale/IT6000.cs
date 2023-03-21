using System.Globalization;
using System.Text;

namespace Quva.Devices.Scale;

/// <summary>
///     IT6000, IT9000 scale _device api
/// </summary>
public class IT6000 : ComProtocol, IScaleApi
{
    private readonly ScaleData _registerData;
    private readonly DeviceOptions _deviceOptions;

    public string[] IT60Description =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "I:",       //Clearinput
        "T:11000",  //Stillstand
        "B:",       //mit < >
        "A:255,^J"  //LF ist Ende. Mit < >
    };


    public IT6000(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = IT60Description;

        OnAnswer += IT60Answer;

        StatusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
        _registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());

        ArgumentNullException.ThrowIfNull(device.Options);
        _deviceOptions = device.Options;
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

    private void IT60Answer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(IT60Answer));
        var data = (ScaleData)tel.AppData; //StatusData or _registerData
        var inStr = Encoding.ASCII.GetString(tel.InData.Buff, 0, tel.InData.Cnt);
        if (data.Command == ScaleCommands.Status.ToString())
        {
            _log.Debug($"[{DeviceCode}] IT60Answer.Status:{inStr}");
            //<00000    1100       0    1100kg122.12.0812:59   0>
            // FFSSSBBBBBBBBTTTTTTTTNNNNNNNNEEWDDDDDDDDZZZZZIIII
            // 1   5    0    5    0    5    0    5    0    5
            //          1         2         3         4
            //IT6 Hobo 2020:
            //<000016.09.2015:21   01    2240       0    2240kg     1   30955>^M^J
            //IT9000 Melk 2014:
            //<000020.10.1412:22  801    1340       0    1340kg  02     63622>^M^J
            // FFSSDDDDDDDDZZZZZIIIIWBBBBBBBBTTTTTTTTNNNNNNNNEE
            //01   5    0    5    0    5    0    5    0    5
            //          1         2         3         4
            bool it9000 = _deviceOptions.Option($"IT9000", false);





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
            _log.Debug($"[{DeviceCode}] IT60Answer.Register:{inStr}");
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
        var tel = await RunTelegram(StatusData, "<RM>");
        if (tel.Error != 0)
        {
            StatusData.ErrorNr = 99;
            StatusData.ErrorText = tel.ErrorText;
            StatusData.Display = tel.ErrorText;
        }

        return await Task.FromResult(StatusData);
    }

    public async Task<ScaleData> Register()
    {
        var tel = await RunTelegram(_registerData, "<RN>");
        if (tel.Error != 0)
        {
            StatusData.ErrorNr = 99;
            StatusData.ErrorText = tel.ErrorText;
            StatusData.Display = tel.ErrorText;
        }

        return await Task.FromResult(_registerData);
    }

    #endregion
}