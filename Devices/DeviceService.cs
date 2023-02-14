using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Quva.Devices
{
    public class DeviceService : IAsyncDisposable
    {
        private bool _disposeFlag = true;
        public IDictionary<string, Device> DeviceList { get; set; }

        public DeviceService()
        {
            DeviceList = new Dictionary<string, Device>();
            Log.Information("creating DeviceService");
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            Log.Warning($"DisposeAsyncCore({_disposeFlag})");
            if (_disposeFlag)
            {
                foreach (Device d in DeviceList.Values)
                {
                    await CloseDevice(d);
                }
            }
            _disposeFlag = false;
        }

        public async ValueTask DisposeAsync()
        {
            Log.Warning($"DisposeAsync()");
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        public async Task<ScaleData> ScaleStatus(string devicecode)
        {
            try
            {
                var scale = await OpenDevice<ScaleDevice>(devicecode);
                var result = await scale.Status();
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return new ScaleData()
                {
                    ErrorNr = 99,
                    ErrorText = ex.Message,
                    Display = ex.Message
                };
            }
        }

        public async Task<ScaleData> ScaleRegister(string devicecode)
        {
            try
            {
                var scale = await OpenDevice<ScaleDevice>(devicecode);
                var result = await scale.Register();
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return new ScaleData()
                {
                    ErrorNr = 99,
                    ErrorText = ex.Message,
                    Display = ex.Message
                };
            }
        }


        #region Interne Aufrufe

        //private async Task<ScaleDevice> OpenScale(string devicecode)
        public async Task<T> OpenDevice<T>(string devicecode) where T : Device, new()
        {
            T typedDevice;
            if (DeviceList.TryGetValue(devicecode, out Device? device))
            {
                Log.Warning($"OpenDevice({typeof(T).Name}.{devicecode}): bereits vorhanden");
                typedDevice = (T)device;  //bereits vorhanden
            }
            else
            {
                Log.Information($"OpenDevice({typeof(T).Name}.{devicecode}): add");
                typedDevice = new T
                {
                    Code = devicecode   //wicht weil nicht in Constructor
                };
                await typedDevice.Load();
                DeviceList.Add(devicecode, typedDevice);
            }
            await typedDevice.Open();
            return await Task.FromResult(typedDevice);
        }

        private async Task CloseDevice<T>(T typedDevice) where T : Device
        {
            await CloseDevice<T>(typedDevice.Code);
        }

        private async Task CloseDevice<T>(string devicecode) where T : Device
        {
            if (DeviceList.TryGetValue(devicecode, out Device? device))
            {
                Log.Information($"CloseDevice({typeof(T).Name}.{devicecode}): close");
                T typedDevice = (T)device;
                DeviceList.Remove(devicecode);
                await typedDevice.Close();
            }
            else
            {
                Log.Warning($"CloseDevice({typeof(T).Name}.{devicecode}): nicht vorhanden");
            }
        }

        #endregion
    }

}
