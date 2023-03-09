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

        OnAnswer += ShowAnswer;
    }

    public async Task<DisplayData> DisplayCommand(string command, string message)
    {
        if (Enum.TryParse<DisplayCommands>(command, out DisplayCommands cmd))
        {
            DisplayData data = cmd switch
            {
                DisplayCommands.Show => await Show(message),
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
          "T:3000",  //timeout
          "B:",      //command = message
};

    #region Commands

    public async Task<DisplayData> Show(string message)
    {
        displayData ??= new DisplayData(device.Code, DisplayCommands.Show.ToString());
        displayData.Message = message;

        // Template + message -> command
        string Line = device.Option("Line", "1");
        string Font = device.Option("Font", "1");
        string Template = device.Option("Template", "#T");  //#L=Zeile #F=Font #T=Text  ^M^J=Endekennung
        string command = Template.Replace("#L", Line)
            .Replace("#F", Font)
            .Replace("^M", ((Char)13).ToString())
            .Replace("^J", ((Char)10).ToString())
            .Replace("#T", message);

        var tel = await RunTelegram(displayData, command);
        if (tel.Error != 0)
        {
            displayData.ErrorNr = 99;
            displayData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(displayData);
    }

    #endregion

    #region Callbacks

    private void ShowAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(ShowAnswer));
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
