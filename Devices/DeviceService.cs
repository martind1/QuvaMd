using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace Quva.Devices
{
    public class DeviceService : IAsyncDisposable
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

        #endregion Card

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


        #region internal calls

        public async Task<ComDevice> OpenDevice(string devicecode)
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

        private async Task CloseDevice(string devicecode)
        {
            if (DeviceList.TryGetValue(devicecode, out ComDevice? device))
            {
                CLog.Information($"CloseDevice({devicecode}): close");
                DeviceList.Remove(devicecode);
                await device.Close();
            }
            else
            {
                CLog.Warning($"CloseDevice({devicecode}): nicht vorhanden");
            }
        }

        #endregion internal calls
    }

}
