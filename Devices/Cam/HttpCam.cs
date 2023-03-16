namespace Quva.Devices.Cam;

/// <summary>
/// Simple Http HttpCam only waits for Number
/// </summary>
public class HttpCam : ComProtocol, ICamApi
{

    private CamData _loadData;
    private readonly DeviceOptions _deviceOptions;
    private readonly string? _url0;

    public HttpCam(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = HttpCamDescription;
        OnAnswer += HttpCamAnswer;
        _loadData = new CamData(device.Code, CamCommands.Load.ToString());

        ArgumentNullException.ThrowIfNull(device.Options);
        _deviceOptions = device.Options;
        _url0 = device.Device.ParamString;
    }

    public async Task<CamData> CamCommand(string command, int camNumber)
    {
        if (Enum.TryParse(command, out CamCommands cmd))
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
        _loadData = new CamData(DeviceCode, CamCommands.Load.ToString())  //important
        {
            Status = CamStatus.Timeout,
            CamNumber = camNumber
        };
        if (camNumber == 0)
        {
            ArgumentNullException.ThrowIfNull(_url0, "ParamString");
            _loadData.Url = _url0;
        }
        else
        {
            string? dfltString = null;  //avoiding CS0121 The call is ambiguous between the methods 'Option'
            string? s = _deviceOptions.Option($"Url{camNumber}", dfltString);
            ArgumentNullException.ThrowIfNull(s, $"Url{camNumber}");
            _loadData.Url = s;
        }
        ComPort.SetParamString(_loadData.Url);  //set _httpParameter.URL
        _log.Debug($"[{DeviceCode}] HttpCam.Load({camNumber}) Host:{new Uri(_loadData.Url).Host}");
        var tel = await RunTelegram(_loadData, "Load");  //no command to send
        if (tel.Error != 0)
        {
            _loadData.ErrorNr = 99;
            _loadData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(_loadData);
    }

    #endregion

    #region Callbacks

    private void HttpCamAnswer(object? sender, TelEventArgs telEventArgs)
    {
        var tel = telEventArgs.Tel;
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(HttpCamAnswer));
        var inBuff = tel.InData;
        //string inStr = Encoding.ASCII.GetString(_inBuff.Buff, 0, _inBuff.Cnt);
        if (_loadData.Command == CamCommands.Load.ToString())
        {
            _loadData.Status = CamStatus.Ok;
            _loadData.ImageBytes = inBuff.Buff;
            _loadData.ImageSize = inBuff.Cnt;
            _loadData.ImageFormat = _loadData.Url?.Split('.').Last();  // E.g. png or jpg
            _log.Debug($"[{DeviceCode}] HttpCam.Answer({_loadData.CamNumber}) Size:{_loadData.ImageSize} Host:{new Uri(_loadData.Url ?? "").Host}");
        }
    }


    #endregion

}
