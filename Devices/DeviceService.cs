using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Quva.Devices
{
    public class DeviceService
    {
        public string Location { get; set; }  //falls Code nicht eindeutig
        public IDictionary<string, Device> DeviceList { get; set; }

        public DeviceService()
        {
            DeviceList = new Dictionary<string, Device>();
            Location = string.Empty;
            Log.Information("creating DeviceService");
        }


        public ScaleDevice OpenScale(string devicecode)
        {
            if (DeviceList.TryGetValue(devicecode, out Device? device))
            {
                Log.Warning($"OpenScale({devicecode}): bereits vorhanden");
                return (ScaleDevice)device;  //bereits vorhanden
            }
            Log.Information($"OpenScale({devicecode}): add");
            var scale = new ScaleDevice(Location, devicecode);
            DeviceList.Add(devicecode, scale);
            scale.Open();

            return scale;
        }

        public void CloseScale(ScaleDevice scale)
        {
            CloseScale(scale.Code);
        }

        public void CloseScale(string devicecode)
        {
            if (DeviceList.TryGetValue(devicecode, out Device? device))
            {
                Log.Information($"CloseScale({devicecode}): close");
                ScaleDevice scale = (ScaleDevice)device;
                DeviceList.Remove(devicecode);
                scale.Close();
            }
            else
            {
                Log.Warning($"CloseScale({devicecode}): nicht vorhanden");
            }
        }


    }

}
