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
    /// IT6000, IT9000 scale device api
    /// </summary>
    public class ScaleIT6000 : ComProtocol, IScaleApi
    {
        public ScaleData statusData { get; set; }
        public ScaleData registerData { get; set; }


        public ScaleIT6000(ComDevice device) : base(device.Code, device.ComPort)
        {
            Description = IT60Description;

            OnAnswer += IT60Answer;

            statusData  = new ScaleData(device.Code, ScaleCommands.Status.ToString());
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

        public string[] IT60Description = new string[]
        {
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "I:",          //Clearinput
          "T:11000",     //Stillstand
          "B:",          //mit < >
          "A:255,^J"     //LF ist Ende. Mit < >
        };

        #region Commands

        public async Task<ScaleData> Status()
        {
            _ = await RunTelegram(statusData, "<RM>");
            return await Task.FromResult(statusData);

        }

        public async Task<ScaleData> Register()
        {
            _ = await RunTelegram(registerData, "<RN>");
            return await Task.FromResult(registerData);
        }

        #endregion

        #region Callbacks

        private void IT60Answer(object? sender, TelEventArgs telEventArgs)
        {
            var tel = telEventArgs.Tel;
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(IT60Answer));
            ScaleData data = (ScaleData)tel.AppData;
        }


        #endregion
    }


}
