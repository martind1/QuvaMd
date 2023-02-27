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



}
