using System.Text;

namespace Quva.Devices.Display;

/// <summary>
/// Simple Display Shower only waits for Number
/// </summary>
public class RemoteDisplay : ComProtocol, IDisplayApi
{
    public DisplayData ShowData { get; set; }
    private readonly DeviceOptions _deviceOptions;

    public RemoteDisplay(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = DefaultDescription;

        OnAnswer += ShowAnswer;

        ShowData = new DisplayData(device.Code, DisplayCommands.Show.ToString());
        ArgumentNullException.ThrowIfNull(device.Options);
        _deviceOptions = device.Options;
    }

    public async Task<DisplayData> DisplayCommand(string command, string message)
    {
        if (Enum.TryParse(command, out DisplayCommands cmd))
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

    public string[] DefaultDescription = new string[]
{
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "T:3000",  //timeout
          "B:",      //command = message
};

    #region Commands

    public async Task<DisplayData> Show(string message)
    {
        ShowData.Message = message;

        // Template + message -> command
        string? Line = _deviceOptions.Option("Line", "1");
        string? Font = _deviceOptions.Option("Font", "1");
        string? Template = _deviceOptions.Option("Template", "#T");  //#L=Zeile #F=Font #T=Text  ^M^J=Endekennung
        string? command = Template?.Replace("#L", Line)
            .Replace("#F", Font)
            .Replace("^M", ((char)13).ToString())
            .Replace("^J", ((char)10).ToString())
            .Replace("#T", message);
        ArgumentNullException.ThrowIfNull(command, "Template");
        var tel = await RunTelegram(ShowData, command);
        if (tel.Error != 0)
        {
            ShowData.ErrorNr = 99;
            ShowData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(ShowData);
    }

    #endregion

    #region Callbacks

    private void ShowAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(ShowAnswer));
        var inBuff = tel.InData;
        string inStr = Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (ShowData.Command == DisplayCommands.Show.ToString())
        {
            //no answer here
        }
    }


    #endregion

}
