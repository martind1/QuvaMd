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


        public ScaleDevice(string location, string devicecode) : base(location, devicecode)
        {
        }

        public override void Open()
        {
            // TODO: ComProt usw öffnen
            Id += 1;
            if (ModulCode == "IT6000")
            {
                //ComProt = new IT6000(this)
            }
        }

        public override void Close()
        {
            // TODO: Dispose ComPort usw
            Id += 10;
        }

        public ScaleData Status()
        {
            return new ScaleData()
            {
                Display = "Hallo",
                Weight = 12.34,
                Unit = AppUnit
            };
        }

        public ScaleData Register()
        {
            return new ScaleData()
            {
                Display = "Hallo",
                Weight = 12.34,
                CalibrationNumber = 9999,
                Unit = AppUnit
            };
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


        public virtual void Open()
        {
            // TODO: ComProt usw öffnen
            Id = 2;
        }

        public virtual void Close()
        {
            // TODO: Dispose ComPort usw
            Id = 30;
        }

        public Device(string location, string devicecode)
        {
            Options = new Dictionary<string, string>();
            Code = devicecode;
            // TODO: von DB laden
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
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DeviceFlags Flags { get; set; }
        public string ParamString { get; set; }
        public string ModulCode { get; set; }
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
