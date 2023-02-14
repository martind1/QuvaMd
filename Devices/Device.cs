using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices
{
    public class ScaleDevice : Device
    {
        public ScaleUnit ScaleUnit { get; set; }
        public ScaleUnit AppUnit { get; set; }


        public ScaleDevice() : base()
        {
        }

        public override async Task Open()
        {
            // TODO: ComProt usw öffnen
            Log.Information($"{Code}.ScaleDevice.Open");
            if (ModulCode == "IT6000")
            {
                //Protocol = new IT6000(this)
            }
            await Task.Delay( 200 );
        }

        public override async Task Close()
        {
            Log.Information($"{Code}.ScaleDevice.Close");
            // TODO: Dispose ComPort usw
            await Task.Delay(200);
        }

        public async Task<ScaleData> Status()
        {
            Log.Information($"{Code}.ScaleDevice.Status");
            var result = new ScaleData()
            {
                Display = "Hallo",
                Weight = 12.34,
                Unit = AppUnit
            };
            await Task.Delay(200);

            return await Task.FromResult(result);
        }

        public async Task<ScaleData> Register()
        {
            Log.Information($"{Code}.ScaleDevice.Register");
            var result = new ScaleData()
            {
                Display = "Hallo",
                Weight = 12.34,
                CalibrationNumber = 9999,
                Unit = AppUnit
            };
            await Task.Delay(200);

            return await Task.FromResult(result);
        }
    }

    public class ScaleData : DeviceData
    {
        //für Waagen:
        public string Display { get; set; } = string.Empty;  //<Wert Einheit>, Wert+Einh, Fehlertext
        public double Weight { get; set; }
        public int CalibrationNumber { get; set; }  //Eichnummer
        public ScaleUnit Unit { get; set; }  //AppUnit
    }

    public class DeviceData
    {
        //für alle Geräte:
        public int ErrorNr { get; set; }
        public string ErrorText { get; set; } = string.Empty;
    }

    public class Device
    {


        public virtual async Task Open()
        {
            // TODO: ComProt usw öffnen
            Log.Information($"{Code}.Device.Open");
            await Task.Delay(200);
        }

        public virtual async Task Close()
        {
            // TODO: Dispose ComPort usw
            Log.Information($"{Code}.Device.Close");
            await Task.Delay(200);
        }

        public async Task Load()
        {
            Log.Information($"{Code}.Device.Load");
            // TODO: [Code] von DB laden
            Id = 1;
            Name = "Testdevice";
            Flags = DeviceFlags.None;
            ParamString = "111.111.111.111:9999";
            ModulCode = "IT6000";
            Transport = TransportType.Truck;
            Packaging = PackagingType.Bulk;

            Options.Add("GerNr", "1");
            Options.Add("DeviceUnit", ScaleUnit.Kilogram.ToString());
            Options.Add("AppUnit", ScaleUnit.Ton.ToString());

            await Task.Delay(200);
        }

        public Device()
        {
            Options = new Dictionary<string, string>();
            //Code = devicecode; * kein Parameter wg CS0304
            Code = string.Empty;
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceFlags Flags { get; set; }
        public string ParamString { get; set; } = string.Empty;
        public string ModulCode { get; set; } = string.Empty;
        public TransportType Transport { get; set; }
        public PackagingType Packaging { get; set; }
        public IDictionary<string, string> Options { get; set; }
    }

    [Flags]
    public enum DeviceFlags
    {
        None = 0,
        Entry = 1,  //'E'
        Exit = 2,   //'A'
    }

    public enum TransportType
    {
        All,
        Truck,
        Rail
    }

    public enum PackagingType
    {
        All,
        Bulk,
        Packaged
    }

    public enum ScaleUnit
    {
        Ton,
        Kilogram,
        Gram
    }
}
