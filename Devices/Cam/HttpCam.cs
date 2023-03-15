using Quva.Devices.Cam;
using Quva.Devices.Card;
using Quva.Devices.Data;
using Quva.Devices.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices.Cam;

/// <summary>
/// Simple Http HttpCam only waits for Number
/// </summary>
public class HttpCam : ComProtocol, ICamApi
{

    private CamData camData { get; set; }
    private DeviceOptions deviceOptions { get; set; }
    private string? Url0 { get; set; }

    public HttpCam(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = HttpCamDescription;
        OnAnswer += HttpCamAnswer;
        camData = new CamData(device.Code, CamCommands.Load.ToString());

        ArgumentNullException.ThrowIfNull(device.Options);
        deviceOptions = device.Options;
        Url0 = device.Device.ParamString;
    }

    public async Task<CamData> CamCommand(string command, int camNumber)
    {
        if (Enum.TryParse<CamCommands>(command, out CamCommands cmd))
        {
            CamData data = cmd switch
            {
                CamCommands.Load => await Load(camNumber),
                _ => throw new NotImplementedException($"CamCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }
        else
        {
            throw new NotImplementedException($"CamCommand({command}) not implemented");
        }
    }



    public string[] HttpCamDescription = new string[]
    {
          ";Automatische Erzeugung. Ändern nicht möglich.",
          "T:3000",  //waiting 3 seconds for cam image
          "A:"
    };

    #region Commands

    public async Task<CamData> Load(int camNumber)
    {
        camData = new CamData(DeviceCode, CamCommands.Load.ToString());  //important
        camData.Status = CamStatus.Timeout;
        camData.CamNumber = camNumber;
        if (camNumber == 0)
        {
            ArgumentNullException.ThrowIfNull(Url0, "ParamString");
            camData.Url = Url0;
        }
        else
        {
            string? dfltString = null;  //avoiding CS0121 The call is ambiguous between the methods 'Option'
            string? s = deviceOptions.Option($"Url{camNumber}", dfltString);
            ArgumentNullException.ThrowIfNull(s, $"Url{camNumber}");
            camData.Url = s;
        }
        ComPort.SetParamString(camData.Url);  //set HttpParameter.URL
        CLog.Debug($"HttpCam.Load({camNumber}) Host:{new Uri(camData.Url).Host}");
        var tel = await RunTelegram(camData, "Load");  //no command to send
        if (tel.Error != 0)
        {
            camData.ErrorNr = 99;
            camData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(camData);
    }

    #endregion

    #region Callbacks

    private void HttpCamAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(HttpCamAnswer));
        CamData data = (CamData)tel.AppData;
        var inBuff = tel.InData;
        //string inStr = Encoding.ASCII.GetString(inBuff.Buff, 0, inBuff.Cnt);
        if (data.Command == CamCommands.Load.ToString())
        {
            data.Status = CamStatus.Ok;
            //data.ImageBytes = (byte[]?)inBuff.Buff.Clone();
            data.ImageBytes = inBuff.Buff;
            data.ImageSize = inBuff.Cnt;
            data.ImageFormat = data.Url?.Split('.').Last();  // E.g. png or jpg
            CLog.Debug($"HttpCam.Answer({data.CamNumber}) Url:{data.Url}");
        }
    }


    #endregion

}
