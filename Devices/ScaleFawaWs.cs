using Quva.Devices;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quva.Devices
{
    /// <summary>
    /// Api for Winsocket ComSvr general truck scale
    /// </summary>
    public class ScaleFawaWs : ComProtocol, IScaleApi
    {
        public ScaleData statusData { get; set; }
        public ScaleData registerData { get; set; }

        public ScaleFawaWs(ComDevice device) : base(device.Code, device.ComPort)
        {
            Description = FawaWsDescription;

            OnAnswer += FawaWsAnswer;

            statusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
            registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());
        }

        public async Task<ScaleData> ScaleCommand(string command)
        {
            if (Enum.TryParse<ScaleCommands>(command, out ScaleCommands cmd))
            {
                ScaleData data = cmd switch
                {
                    ScaleCommands.Status => await Status(),
                    ScaleCommands.Register => await Register(),
                    _ => throw new NotImplementedException($"ScaleCommands.{command} not implemented")
                };
                return await Task.FromResult(data);
            }
            else
            {
                throw new NotImplementedException($"ScaleCommand({command}) not implemented");
            }
        }

        public string[] FawaWsDescription = new string[]
        {
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "P:50",
          "I:",          //Clearinput
          "T:11000",     //Stillstand
          "T2:0",        //between character
          "B:",          //HoleStatus=?
          "A:255:1"      //HoleStatus=<Status>;<Gewicht>;<Meldung>
        };

        #region Commands

        public async Task<ScaleData> Status()
        {
            var tel = await RunTelegram(statusData, "Holestatus=?");
            if (tel.Error != 0)
            {
                statusData.ErrorNr = 99;
                statusData.ErrorText = tel.ErrorText;
                statusData.Display = tel.ErrorText; //"Error99";
            }
            return await Task.FromResult(statusData);

        }

        public async Task<ScaleData> Register()
        {
            var tel = await RunTelegram(registerData, "ProtGewicht=?");
            if (tel.Error != 0)
            {
                registerData.ErrorNr = 99;
                registerData.ErrorText = tel.ErrorText;
                registerData.Display = tel.ErrorText; //"Error99";
            }
            return await Task.FromResult(registerData);
        }

        #endregion

        #region Callbacks

        private void FawaWsAnswer(object? sender, TelEventArgs telEventArgs)
        {
            var tel = telEventArgs.Tel;
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(FawaWsAnswer));
            ScaleData data = (ScaleData)tel.AppData;
            var rnd = new Random();
            var inBuff = tel.InData;
            string inStr = System.Text.Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
            if (data.Command == ScaleCommands.Status.ToString())
            {
                CLog.Debug($"[{DeviceCode}] FawaWsAnswer.Status:{inStr}");
                //HoleStatus=StatusStr;GewichtStr;Meldung
                int p = inStr.IndexOf('=');
                var sl = inStr[(p + 1)..].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string StatusStr = sl[0];
                string GewichtStr = sl[1];
                string Meldung = sl[2];
                int StatusNr = int.Parse(StatusStr);
                ScaleStatus Status = 0;

                data.Weight = float.Parse(GewichtStr, new CultureInfo("de-DE"));  //Dezimalkomma!
                data.Unit = ScaleUnit.Ton;
                data.CalibrationNumber = 0;
                if (StatusNr == 0 || StatusNr == 1 || StatusNr == 2 || StatusNr == 5)
                {
                    Status |= ScaleStatus.WeightOK;
                    if (StatusNr == 1 || StatusNr == 5)
                    {
                        Status |= ScaleStatus.Underload;
                    }
                    if (StatusNr == 2)
                    {
                        Status |= ScaleStatus.NoStandstill;
                    }
                    data.Display = $"{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}";
                    data.ErrorNr = 0;
                }
                else
                {
                    Status |= ScaleStatus.NoWeight;
                    data.Display = Meldung ?? "Error";
                    data.ErrorNr = 1;
                }
            }

            else if (data.Command == ScaleCommands.Register.ToString())
            {
                CLog.Debug($"[{DeviceCode}] FawaWsAnswer.Register:{inStr}");
                //ProtGewicht=StatusStr;GewichtStr;EichNrStr;Meldung
                int p = inStr.IndexOf('=');
                var sl = inStr[(p + 1)..].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string StatusStr = sl[0];
                string GewichtStr = sl[1];
                string EichNrStr = sl[2];
                string Meldung = sl[3];
                int StatusNr = int.Parse(StatusStr);
                ScaleStatus Status = 0;

                data.Weight = float.Parse(GewichtStr, new CultureInfo("de-DE"));  //Dezimalkomma!
                data.Unit = ScaleUnit.Ton;
                data.CalibrationNumber = int.Parse(EichNrStr);
                if (StatusNr == 0)
                {
                    Status |= ScaleStatus.WeightOK;
                    data.Display = $"<{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}>";
                }
                else
                {
                    data.Display = Meldung ?? $"Error {StatusNr}";
                    Status |= ScaleStatus.WeightOK;
                    if (StatusNr == 1 || StatusNr == 5)
                    {
                        Status |= ScaleStatus.Underload;
                    }
                    if (StatusNr == 2)
                    {
                        Status |= ScaleStatus.PositionError;
                    }
                    else
                    {
                        Status |= ScaleStatus.NoWeight;
                    }
                    data.ErrorNr = 1;
                }
            }
        }


        #endregion
    }


}
