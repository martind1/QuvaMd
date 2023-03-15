using Microsoft.AspNetCore.Http;
using Quva.Devices.Cam;
using Quva.Devices.Card;
using Quva.Devices.Display;
using Quva.Devices.Scale;
using Serilog;
using static Quva.Devices.ComDevice;

namespace Quva.Devices
{
    public class DeviceService : IAsyncDisposable, IDeviceService
    {
        private readonly ILogger CLog;
        private bool disposeFlag = true;
        public IDictionary<string, ComDevice> DeviceList { get; set; }


        public DeviceService()
        {
            DeviceList = new Dictionary<string, ComDevice>();
            CLog = Log.ForContext<DeviceService>();
            CLog.Information("creating DeviceService");
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            CLog.Information($"{nameof(DeviceService)}.DisposeAsyncCore({disposeFlag})");
            if (disposeFlag)
            {
                foreach (ComDevice d in DeviceList.Values)
                {
                    await CloseDevice(d);
                }
            }
            disposeFlag = false;
        }

        public async ValueTask DisposeAsync()
        {
            CLog.Information($"{nameof(DeviceService)}.DisposeAsync()");
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        #region Card

        public async Task<CardData> CardRead(string devicecode)
        {
            return await CardCommand(devicecode, CardCommands.Read.ToString());
        }

        // bevorzugter Rückgabewert: struct ScaleData
        public async Task<CardData> CardCommand(string devicecode, string command)
        {
            CardData result;
            ComDevice? device = null;
            try
            {
                device = await OpenDevice(devicecode);
                result = await device.CardCommand(command);
            }
            catch (Exception ex)
            {
                if (device != null)
                    await device.Close().ConfigureAwait(false);
                result = new CardData(devicecode, command)
                {
                    ErrorNr = 99,
                    ErrorText = ex.Message,
                };
            }
            return await Task.FromResult(result);
        }

        //with callback:

        public async Task<IResult> CardReadStart(string devicecode, ComDevice.OnCardRead onCardRead)
        {
            return await CardCommandStart(devicecode, CardCommands.Read.ToString(), onCardRead);
        }

        public async Task<IResult> CardCommandStart(string devicecode, string command, ComDevice.OnCardRead onCardRead)
        {
            ComDevice? device = null;
            IResult? result;
            try
            {
                result = Results.Ok();
                device = await OpenDevice(devicecode);
                device.CardCommandStart(command, onCardRead);
            }
            catch (Exception ex)
            {
                result = Results.NotFound(ex.Message);
                if (device != null)
                    await device.Close().ConfigureAwait(false);
            }
            return await Task.FromResult(result);
        }

        #endregion Card

        #region Camera

        public async Task<CamData> CamLoad(string devicecode, int camNumber)
        {
            return await CamCommand(devicecode, CamCommands.Load.ToString(), camNumber);
        }

        // bevorzugter Rückgabewert: struct ScaleData
        public async Task<CamData> CamCommand(string devicecode, string command, int camNumber)
        {
            CamData result;
            ComDevice? device = null;
            try
            {
                device = await OpenDevice(devicecode);
                result = await device.CamCommand(command, camNumber);
            }
            catch (Exception ex)
            {
                if (device != null)
                    await device.Close().ConfigureAwait(false);
                result = new CamData(devicecode, command)
                {
                    ErrorNr = 99,
                    ErrorText = ex.Message,
                };
            }
            return await Task.FromResult(result);
        }

        #endregion Camera

        #region Display

        public async Task<DisplayData> DisplayShow(string devicecode, string message)
        {
            return await DisplayCommand(devicecode, DisplayCommands.Show.ToString(), message);
        }

        public async Task<DisplayData> DisplayCommand(string devicecode, string command, string message)
        {
            DisplayData result;
            ComDevice? device = null;
            try
            {
                device = await OpenDevice(devicecode);
                result = await device.DisplayCommand(command, message);
            }
            catch (Exception ex)
            {
                if (device != null)
                    await device.Close().ConfigureAwait(false);
                result = new DisplayData(devicecode, command)
                {
                    ErrorNr = 99,
                    ErrorText = ex.Message,
                };
            }
            return await Task.FromResult(result);
        }

        //with callback:

        public async Task<IResult> DisplayShowStart(string devicecode, OnDisplayShow onDisplayShow)
        {
            return await DisplayCommandStart(devicecode, DisplayCommands.Show.ToString(), onDisplayShow, null);
        }

        public async Task<IResult> DisplayShowScale(string devicecode, string scaleCode)
        {
            return await DisplayCommandStart(devicecode, DisplayCommands.Show.ToString(), OnShowScale, scaleCode);
        }

        public async Task<IResult> DisplayCommandStart(string devicecode, string command, OnDisplayShow onDisplayShow, string? scaleCode)
        {
            ComDevice? device = null;
            IResult? result;
            try
            {
                result = Results.Ok();
                device = await OpenDevice(devicecode);
                device.ScaleCode = scaleCode;
                device.DisplayCommandStart(command, onDisplayShow);
            }
            catch (Exception ex)
            {
                result = Results.NotFound(ex.Message);
                if (device != null)
                    await device.Close();
            }
            return await Task.FromResult(result);
        }

        #endregion display

        #region Scale

        public async Task<ScaleData> ScaleStatus(string devicecode)
        {
            return await ScaleCommand(devicecode, ScaleCommands.Status.ToString());
        }

        public async Task<ScaleData> ScaleRegister(string devicecode)
        {
            return await ScaleCommand(devicecode, ScaleCommands.Register.ToString());
        }

        // bevorzugter Rückgabewert: struct ScaleData
        public async Task<ScaleData> ScaleCommand(string devicecode, string command)
        {
            ScaleData result;
            ComDevice? device = null;
            try
            {
                device = await OpenDevice(devicecode);
                result = await device.ScaleCommand(command);
            }
            catch (Exception ex)
            {
                if (device != null)
                    await device.Close().ConfigureAwait(false);
                result = new ScaleData(devicecode, command)
                {
                    ErrorNr = 99,
                    ErrorText = ex.Message,
                    Display = ex.Message
                };
            }
            return await Task.FromResult(result);
        }

        // with callback:

        public async Task<IResult> ScaleStatusStart(string devicecode, ComDevice.OnScaleStatus onScaleStatus)
        {
            ComDevice? device = null;
            IResult? result;
            try
            {
                result = Results.Ok();
                device = await OpenDevice(devicecode);
                device.ScaleCommandStart(ScaleCommands.Status.ToString(), onScaleStatus);
            }
            catch (Exception ex)
            {
                result = Results.NotFound(ex.Message);
                if (device != null)
                    await device.Close().ConfigureAwait(false);
            }
            return await Task.FromResult(result);
        }


        // alternativer Rückgabewerte: Json String
        //public async Task<string> ScaleCommand(string devicecode, string command)
        //{
        //    try
        //    {
        //        var device = await OpenDevice<ComDevice>(devicecode);
        //        var result = await device.Command(command);
        //        return await Task.FromResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        var data = new ScaleData()
        //        {
        //            ErrorNr = 99,
        //            ErrorText = ex.Message,
        //            Display = ex.Message
        //        };
        //        JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        //        var result = JsonSerializer.Serialize<ScaleData>(data, options);
        //        return await Task.FromResult(result);
        //    }
        //}

        #endregion Scale

        #region Helper functions

        /// <summary>
        /// Gets the actual Display of a named scale
        /// </summary>
        /// <param name="devicecode"></param>
        public string GetScaleDisplay(string scaleCode)
        {
            string? result = null;
            if (DeviceList.TryGetValue(scaleCode, out ComDevice? device))
            {
                result = device.ScaleApi?.statusData.Display;
            }
            return result ?? string.Empty;
        }

        private void OnShowScale(DisplayData displayData)
        {
            if (DeviceList.TryGetValue(displayData.DeviceCode, out ComDevice? device))
            {
                ArgumentNullException.ThrowIfNull(device.ScaleCode);
                displayData.Message = GetScaleDisplay(device.ScaleCode);
            }
        }


        #endregion Helper functions


        #region internal calls

        private async Task<ComDevice> OpenDevice(string devicecode)
        {
            if (DeviceList.TryGetValue(devicecode, out ComDevice? device))
            {
                CLog.Information($"[{devicecode}] OpenDevice: bereits vorhanden");
            }
            else
            {
                CLog.Information($"[{devicecode}] OpenDevice: add");
                device = new ComDevice
                {
                    Code = devicecode   //wichtig weil nicht in Constructor
                };
                await device.Load();
                DeviceList.Add(devicecode, device);
            }
            await device.Open();
            return await Task.FromResult(device);
        }

        private async Task CloseDevice(ComDevice device)
        {
            await CloseDevice(device.Code);
        }

        public async Task CloseDevice(string devicecode)
        {
            if (DeviceList.TryGetValue(devicecode, out ComDevice? device))
            {
                CLog.Information($"CloseDevice({devicecode}): close");
                DeviceList.Remove(devicecode);
                await device.Close();
            }
            else
            {
                CLog.Warning($"CloseDevice({devicecode}): not found");
            }
        }

        #endregion internal calls
    }

}
