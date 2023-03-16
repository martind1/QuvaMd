namespace Quva.Devices.Scale
{
    /// <summary>
    /// IT6000, IT9000 scale _device api
    /// </summary>
    public class IT6000 : ComProtocol, IScaleApi
    {
        public ScaleData StatusData { get; set; }
        private readonly ScaleData _registerData;


        public IT6000(ComDevice device) : base(device.Code, device.ComPort)
        {
            Description = IT60Description;

            OnAnswer += IT60Answer;

            StatusData = new ScaleData(device.Code, ScaleCommands.Status.ToString());
            _registerData = new ScaleData(device.Code, ScaleCommands.Register.ToString());
        }

        public async Task<ScaleData> ScaleCommand(string command)
        {
            if (Enum.TryParse(command, out ScaleCommands cmd))
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

        #region Callbacks

        private void IT60Answer(object? sender, TelEventArgs telEventArgs)
        {
            var tel = telEventArgs.Tel;
            ArgumentNullException.ThrowIfNull(tel.AppData, nameof(IT60Answer));
            //ScaleData data = (ScaleData)tel.AppData;
        }

        #endregion
    }


}
