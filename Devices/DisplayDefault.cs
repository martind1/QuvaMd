using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

/// <summary>
/// Simple Display Shower only waits for Number
/// </summary>
public class DisplayDefault : ComProtocol, IDisplayApi
{
    private readonly ComDevice device;

    public DisplayDefault(string deviceCode, ComDevice device) : base(deviceCode, device.ComPort)
    {
        this.device = device;

        Description = DefaultDescription;

        OnAnswer += ShowerAnswer;
    }

    public async Task<DisplayData> DisplayCommand(string command, string message)
    {
        if (Enum.TryParse<DisplayCommands>(command, out DisplayCommands cmd))
        {
            DisplayData data = cmd switch
            {
                DisplayCommands.Show => await Show(),
                _ => throw new NotImplementedException($"DisplayCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }
        else
        {
            throw new NotImplementedException($"DisplayCommand({command}) not implemented");
        }
    }

    private DisplayData? displayData;

    public string[] DefaultDescription = new string[]
{
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "T:0",     //infinity wait
          "T2:200",  //between character
          "A:255:6"  //without delimiter
};

    #region Commands

    public async Task<DisplayData> Show()
    {
        displayData = new DisplayData(device.Code, DisplayCommands.Show.ToString());
        var tel = await RunTelegram(displayData, "read");
        if (tel.Error != 0)
        {
            displayData.ErrorNr = 99;
            displayData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(displayData);
    }

    #endregion

    #region Callbacks

    private void ShowerAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(ShowerAnswer));
        DisplayData data = (DisplayData)tel.AppData;
        var inBuff = tel.InData;
        string inStr = System.Text.Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (data.Command == DisplayCommands.Show.ToString())
        {
            //no answer here
        }
    }


    #endregion

}
