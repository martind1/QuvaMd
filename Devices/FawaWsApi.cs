using Quva.Devices;
using System;
using System.Collections.Generic;
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
    public class FawaWsApi : ComProtocol, IScaleApi
    {
        private readonly ComDevice device;

        public FawaWsApi(ComDevice device) : base(device.ComPort)
        {
            this.device = device;

            Description = FawaWsDescription;

            OnAnswer += FawaWsAnswer;
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

        public event EventHandler<JsonStringArgs>? OnCommandAnswer;
        public event EventHandler<JsonStringArgs>? OnCommandError;
        private void DoCommandAnswer(JsonStringArgs e) => OnCommandAnswer?.Invoke(this, e);
        private void DoCommandError(JsonStringArgs e) => OnCommandError?.Invoke(this, e);
        private ScaleData? statusData;
        private ScaleData? registerData;


        public string[] FawaWsDescription = new string[]
        {
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "I:",          //Clearinput
          "T:11000",     //Stillstand
          "B:",          //HoleStatus=?
          "A:255:1"      //HoleStatus=<Status>;<Gewicht>;<Meldung>
        };

        #region Commands

        public async Task<ScaleData> Status()
        {
            statusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
            var tel = await RunTelegram(statusData, "Holestatus=?");
            //return await Task.FromResult(statusData)
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(Status));
            return await Task.FromResult((ScaleData)tel.AppData);

        }

        public async Task<ScaleData> Register()
        {
            registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());
            var tel = await RunTelegram(registerData, "ProtGewicht=?");
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(Register));
            return await Task.FromResult((ScaleData)tel.AppData);
        }

        #endregion

        #region Callbacks

        private void FawaWsAnswer(object? sender, TelEventArgs telEventArgs)
        {
            var tel = telEventArgs.Tel;
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(FawaWsAnswer));
            ScaleData data = (ScaleData)tel.AppData;
            var rnd = new Random();

            if (data.Command == ScaleCommands.Status.ToString())
            {
                data.Weight = float.Parse(DateTime.Now.ToString("ss")) + Math.Round(rnd.NextDouble(), 2);
                data.Unit = ScaleUnit.Ton;
                data.CalibrationNumber = 0;
                data.Display = $"{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}";
            }

            else if (data.Command == ScaleCommands.Register.ToString())
            {
                data.Weight = float.Parse(DateTime.Now.ToString("ss")) + Math.Round(rnd.NextDouble(), 2);
                data.Unit = ScaleUnit.Ton;
                data.CalibrationNumber = (int)rnd.NextInt64(10000, 99999);
                data.Display = $"<{data.Weight:F2} {DeviceUtils.UnitShort(data.Unit)}>";  //mit Eichklammern<>
            }
        }


        #endregion
    }


}
