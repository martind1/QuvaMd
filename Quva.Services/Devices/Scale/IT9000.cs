using Quva.Services.Devices.Modbus;
using System.Text;

namespace Quva.Services.Devices.Scale;

/// <summary>
///     Systec IT9000 scale 
/// </summary>
public class IT9000 : ComProtocol, IScaleApi
{
    public ScaleData StatusData { get; set; }
    public bool PositionInit { get; set; } = false;
    public string PositionCode { get; set; }
    public ModbusData? PositionData { get; set; }
    private readonly ScaleData _registerData;
    private readonly DeviceOptions _deviceOptions;
    private readonly bool _it9000;
    private readonly string _positionVariable;
    private readonly string _positionOkValue;

    public string[] IT9000Description =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "I:",       //Clearinput
        "T:11000",  //Stillstand
        "B:",       //mit < >
        "A:255,^J"  //LF ist Ende. Mit < >
    };


    public IT9000(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = IT9000Description;

        OnAnswer = IT9000Answer;

        StatusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
        _registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());

        ArgumentNullException.ThrowIfNull(device.Options);
        _deviceOptions = device.Options;
        _it9000 = _deviceOptions.Option($"IT9000", false);
        PositionCode = _deviceOptions.Option("POSITION_CODE", string.Empty);
        _positionVariable = _deviceOptions.Option("POSITION_VARIABLE", string.Empty);
        _positionOkValue = _deviceOptions.Option("POSITION_OK_VALUE", "0");
    }

    public bool PositionOk()
    {
        bool result = true;
        if (PositionCode != string.Empty && PositionInit)
        {
            foreach (string variable in _positionVariable.Split(';'))
            {
                string value = PositionData?.GetValue(variable) ?? _positionOkValue;
                if (value != _positionOkValue)
                    result = false;
            }
        }
        return result;
    }

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
        if (!PositionOk())
        {
            _registerData.ErrorNr = 13;
            _registerData.Status = ScaleStatus.PositionError;
            _registerData.ErrorText = "PositionError";
            _registerData.Display = _registerData.ErrorText;
        }
        else
        {
            var tel = await RunTelegram(_registerData, "<RN>");
            if (tel.Error != 0)
            {
                _registerData.ErrorNr = 99;
                _registerData.Status = ScaleStatus.Timeout;
                _registerData.ErrorText = tel.ErrorText;
                _registerData.Display = _registerData.ErrorText;
            }
        }
        return await Task.FromResult(_registerData);
    }

    #endregion

    #region Callbacks

    private void IT9000Answer(ComTelegram tel)
    {
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(IT9000Answer));
        var data = (ScaleData)tel.AppData; //StatusData or _registerData
        data.Reset();  //all 0
        var inStr = Encoding.ASCII.GetString(tel.InData.Buff, 0, tel.InData.Cnt);

        double minWeight = _deviceOptions.Option("MinWeight", 0.0);

        //for all commands:
        var errorCode = inStr.Substring(1, 2);
        if (errorCode != "00")
        {
            if (!int.TryParse(errorCode, out int err))
                err = 99;
            data.ErrorNr = err;
            data.ErrorText = $"Fehler {err}";
            data.Display = data.ErrorText;
            if (err == 13 || err == 18)
                data.Status |= ScaleStatus.NoStandstill;
            else if (err == 12)
                data.Status |= ScaleStatus.Overload;

            data.Status |= err switch
            {
                12 => ScaleStatus.Overload,
                13 or 18 => ScaleStatus.NoStandstill,
                16 or 17 => ScaleStatus.PrinterFault,
                20 => ScaleStatus.Underload,
                31 => ScaleStatus.Timeout,
                32 or 33 => ScaleStatus.ScaleNumberError,
                _ => ScaleStatus.ScaleFault,
            };
            return;
        }  //error

        if (data.Command == ScaleCommands.Status.ToString() || data.Command == ScaleCommands.Register.ToString())
        {
            //see Kommunikation_IT3000M.pdf
            //IT6 Hobo 2020:
            //<000016.09.2015:21   01    2240       0    2240kg     1   30955>^M^J
            //IT9000 Melk 2014:
            //<000020.10.1412:22  801    1340       0    1340kg  02     63622>^M^J
            // FFSSDDDDDDDDZZZZZIIIIWBBBBBBBBTTTTTTTTNNNNNNNNEE
            //01   5    0    5    0    5    0    5    0    5    0    5    0    5
            //          1         2         3         4         5         6
            var stillstand = inStr.Substring(3, 1);
            var bruttoNegative = inStr.Substring(4, 1);
            var scaleDate = inStr.Substring(5, 8);
            var scaleTime = inStr.Substring(13, 5);
            var identNumber = inStr.Substring(18, 4);
            var scaleNumber = inStr.Substring(22, 1);
            var gross = inStr.Substring(23, 8);
            var tare = inStr.Substring(31, 8);
            var net = inStr.Substring(39, 8);
            var unit = inStr.Substring(47, 2);
            var tareCode = inStr.Substring(49, 2);
            var weighingRange = inStr.Substring(51, 1);
            var terminalNumber = inStr.Substring(52, 3);
            var checkCharacter = inStr.Substring(55, 8);

            data.Unit = ScaleUnit.Ton;
            double weight;
            try
            {
                //_430.00
                weight = double.Parse(gross, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                weight = 0;
                data.ErrorNr = 2;
                data.ErrorText = ex.Message;
                data.Display = data.ErrorText;
            }
            if (data.ErrorNr != 0)
                return;


            if (stillstand == "1")
                data.Status |= ScaleStatus.NoStandstill;
            if (weight < minWeight)
                data.Status |= ScaleStatus.Underload;
            if (!PositionOk())
                data.Status |= ScaleStatus.PositionError;
            if (bruttoNegative == "1")
            {
                data.Status |= ScaleStatus.Underload;
                //beware - is already negativ weight = -weight;
            }
            if (unit == "kg" && data.Unit == ScaleUnit.Ton)
                weight /= 1000.0;
            data.Weight = weight;

            if (data.Command == ScaleCommands.Status.ToString())
            {
                _log.Debug($"[{DeviceCode}] IT9000Answer.Status:{inStr}");
                data.Status |= ScaleStatus.WeightOK;
                data.Display = $"{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}";
            }
            else if (data.Command == ScaleCommands.Register.ToString())
            {
                _log.Debug($"[{DeviceCode}] IT9000Answer.Register:{inStr}");
                data.CalibrationNumber = int.Parse(identNumber);
                data.Status |= ScaleStatus.WeightOK;
                data.Display = $"<{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}>";
            }
        }
    }

    #endregion

}