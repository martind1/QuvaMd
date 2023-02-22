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
    /// Interface for concrete Device Dialog Api
    /// </summary>
    public class IT60Api : ComProtocol, IScaleApi
    {
        private readonly ComDevice device;

        public IT60Api(ComDevice device) : base(device.ComPort)
        {
            this.device = device;

            Description = IT60Description;

            OnAnswer += IT60Answer;
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
            statusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
            var tel = await RunTelegram(statusData, "<RM>");
            //return await Task.FromResult(statusData)
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(Status));
            return await Task.FromResult((ScaleData)tel.AppData);

        }

        public async Task<ScaleData> Register()
        {
            registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());
            var tel = await RunTelegram(registerData, "<RN>");
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(Register));
            return await Task.FromResult((ScaleData)tel.AppData);
        }

        #endregion

        #region Callbacks

        private void IT60Answer(object? sender, TelEventArgs telEventArgs)
        {
            var tel = telEventArgs.tel;
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(IT60Answer));
            ScaleData data = (ScaleData)tel.AppData;
        }


        #endregion
    }


}
