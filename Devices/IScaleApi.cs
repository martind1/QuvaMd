using Quva.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices
{
    /// <summary>
    /// Interface for concrete Device Dialog Api
    /// </summary>
    public interface IScaleApi
    {
        public Task<ScaleData> ScaleCommand(string command);

        public event EventHandler<JsonStringArgs>? OnCommandAnswer;
        public event EventHandler<JsonStringArgs>? OnCommandError;

    }

    public class JsonStringArgs : EventArgs
    {
        public string JsonString { get; set; } = string.Empty;
    }

    // Commands for Scale:
    public enum ScaleCommands
    {
        None,
        Status,
        Register
    }


    public class ScaleData : DeviceData
    {
        //für Waagen:
        public string Display { get; set; } = string.Empty;  //<Wert Einheit>, Wert+Einh, Fehlertext
        public double Weight { get; set; }
        public int CalibrationNumber { get; set; }  //Eichnummer
        public ScaleUnit Unit { get; set; }  //AppUnit

        public ScaleData(string deviceCode, string command) : base(deviceCode, command)
        {
        }
    }

    public class DeviceData
    {
        //für alle Geräte:
        public string DeviceCode { get; set; }
        public string Command { get; set; }
        public int ErrorNr { get; set; }
        public string ErrorText { get; set; } = string.Empty;

        public DeviceData(string deviceCode, string command)
        {
            DeviceCode = deviceCode;
            Command = command;
        }
    }


}
