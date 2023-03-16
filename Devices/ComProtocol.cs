using Quva.Devices.ComPort;
using Serilog;
using System.Text;

namespace Quva.Devices;

public class ComProtocol : IAsyncDisposable
{
    protected ILogger _log;
    public string DeviceCode { get; }
    private IComPort? _comPort = null;
    public IComPort ComPort { get => _comPort ?? throw new ArgumentNullException(nameof(_comPort)); set => _comPort = value; }
    public string[] Description { get; set; }
    public ComProtErrorActions ErrorActions { get; set; }
    public event EventHandler<TelEventArgs>? OnAnswer;
    public event EventHandler<UserFnEventArgs>? OnUserFn;
    private void DoAnswer(TelEventArgs e) => OnAnswer?.Invoke(this, e);
    //Config:
    public int MaxDataLen { get; set; } = 2048;
    public int ProtTelId { get; set; } = -1;
    public bool DoubleDle { get; set; } = false;
    public bool Echo { get; set; } = false;


    public ComProtocol(string deviceCode, IComPort? comPort)
    {
        _log = Log.ForContext<DeviceService>();
        DeviceCode = deviceCode;
        this._comPort = comPort;
        Description = new string[] { "B:", "S:^M", "A:128,^M" }; //nur Demo
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        _log.Warning($"[{DeviceCode}] {GetType().Name}.DisposeAsyncCore({_comPort != null})");
        if (_comPort != null)
        {
            await _comPort.CloseAsync();
        }
        _comPort = null;
    }

    public async ValueTask DisposeAsync()
    {
        _log.Warning($"[{DeviceCode}] {GetType().Name}.DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
    }

    #region i/o operations

    /// <summary>
    /// send control bytes. Call UserFnc (blockchecks and other)
    /// ^C -> 0x03
    /// $28 -> 0x28
    /// 'c' -> (byte)c
    /// [FncName] -> UserFnc(tel, FncName) -> sends via ComPort
    /// </summary>
    /// <param name="tel"></param>
    /// <param name="descParam"></param>
    private async Task<bool> SendAsync(ComTelegram tel, string descParam)
    {
        bool result = false;
        ByteBuff tempBuff = new(1024);  //Len = 0
        // evaluate params and send bytes
        for (int i = 0; i < descParam.Length; i++)
        {
            if (descParam[i] == '^' && i < descParam.Length - 1 && (byte)descParam[i + 1] > 64)
            {
                i++;
                tempBuff.Buff[tempBuff.Cnt++] = (byte)(((byte)descParam[i]) - 64);  //^A->01
            }
            else if (descParam[i] == '$' && i < descParam.Length - 2)
            {
                tempBuff.Buff[tempBuff.Cnt++] = Convert.ToByte(descParam[(i + 1)..(i + 2)], 16);
                i += 2;
            }
            // abc[def]g  i=3, p=4 -> i=7, userFn=4..6='def'
            // 012345678
            else if (descParam[i] == '[' && descParam[i..].IndexOf(']') > 0)
            {
                var p = descParam[i..].IndexOf(']');
                string userFnk = descParam[(i + 1)..(i + p - 1)];
                i += p;
                DoUserFn(new UserFnEventArgs(tel, userFnk));
            }
            else
            {
                tempBuff.Buff[tempBuff.Cnt++] = (byte)descParam[i];
            }
        }
        if (tempBuff.Cnt > 0)
        {
            result = await ComPort.WriteAsync(tempBuff);
        }
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Clear input buffer
    /// </summary>
    /// <returns>count of cleared and readed bytes</returns>
    private async Task<int> ClearInputAsync(ByteBuff dummydata)
    {
        dummydata.Cnt = 0;
        if (_comPort != null)
        {
            dummydata.Cnt = await ComPort.InCountAsync(0);
            if (dummydata.Cnt > 0)
                await ComPort.ReadAsync(dummydata);
        }
        return await Task.FromResult(dummydata.Cnt);
    }

    /// <summary>
    /// ReadData: read from Port min characters or until delim1/delim2 got
    ///   into data.Buff offset data.Cnt
    /// </summary>
    /// <param name="data">Buff, Cnt</param>
    /// <returns>false bedeutet nur daß noch nicht alle Zeichen eingelesen wurden und kein Abbruch des Telegramms</returns>
    private async Task<ReadDataResult> ReadDataAsync(ByteBuff data, int minLen, int maxLen, byte delim1, byte delim2)
    {
        ReadDataResult result = ReadDataResult.OK;

        // for HttpClient: performs complete reading
        if (ComPort.DirectMode)
        {
            await ComPort.ReadAsync(data);
            return await Task.FromResult(result);
        }
        var tmpBuff = new ByteBuff(MaxDataLen)
        {
            Cnt = Math.Max(1, minLen)
        };
        // erstes Zeichen lesen mit timeout. Danach nur wenn InCount > 0.
        int n = await ComPort.ReadAsync(tmpBuff);
        tmpBuff.AppendTo(data);
        //if (n == 0 || n < minLen)
        if (n == 0)
        {
            result = ReadDataResult.NoMinLen;
            return await Task.FromResult(result);
        }
        if (delim1 != 0)
            result = ReadDataResult.DelimiterMissing;
        while (await ComPort.InCountAsync((result == ReadDataResult.DelimiterMissing || delim1 == 0)
            ? ComPort.ComParameter.Timeout2Ms : 0) > 0)
        {
            tmpBuff.Cnt = 1;
            await ComPort.ReadAsync(tmpBuff);
            if (delim1 != 0 && tmpBuff.Buff[0] == delim1)
            {
                if (delim2 == 0)
                {
                    result = ReadDataResult.OK;
                    break;
                }
                else
                {
                    tmpBuff.Cnt = 1;
                    await ComPort.ReadAsync(tmpBuff);
                    if (tmpBuff.Buff[0] == delim2)
                    {
                        result = ReadDataResult.OK;
                        break;
                    }
                }
            }
            else
            {
                tmpBuff.AppendTo(data);
                if (data.Cnt >= maxLen)
                    break;
            }
        }
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Receive answer and/or control character into data.Buff offest 0. Increments data.Cnt.
    /// maxcount, mincount, delimiter1, delimiter2
    /// n[:m],d1,d2
    /// </summary>
    /// <param name="answer">true = get answer data, false = only wait for tokens</param>
    /// <returns>true = ok, false = timeout error</returns>
    private async Task<bool> Receive(ByteBuff data, string descParam)
    {
        var delimiter = new byte[] { 0, 0 };
        int nDelim = 0;
        int MaxLen = MaxDataLen;
        int MinLen = 0;
        int RemainingMinLen;
        var slTok = descParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        try
        {
            for (int i = 0; i < slTok.Length; i++)
            {
                if (slTok[i].StartsWith("^"))
                {
                    delimiter[nDelim++] = (byte)(((byte)slTok[i][1]) - 64);
                }
                else if (slTok[i].StartsWith("$"))
                {
                    delimiter[nDelim++] = Convert.ToByte(slTok[i][1..2], 16);
                }
                else if (i == 0 && char.IsDigit(slTok[i][0]))
                {
                    var slLen = slTok[i].Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    MaxLen = int.Parse(slLen[0]);
                    MinLen = slLen.Length > 1 ? int.Parse(slLen[1]) : 0;
                }
                else
                {
                    delimiter[nDelim++] = (byte)slTok[i][0];  //zB < (IT6000)
                }
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"[{DeviceCode}] error description({descParam})", nameof(descParam), ex);
        }

        data.Cnt = 0;  //read from 0. Moves Cnt.
        RemainingMinLen = MinLen;
        var rdResult = await ReadDataAsync(data, RemainingMinLen, MaxLen, delimiter[0], delimiter[1]);
        return await Task.FromResult(rdResult == ReadDataResult.OK);
    }

    #endregion

    #region Telegrams

    public async Task<ComTelegram> RunTelegram(object appData, string strCmd)
    {
        byte[] byteCmd = Encoding.ASCII.GetBytes(strCmd);
        return await RunTelegram(appData, new ByteBuff(byteCmd, byteCmd.Length));
    }

    public async Task<ComTelegram> RunTelegram(object appData, ByteBuff byteCmd)
    {
        // in constructor: description, buffer, ..
        var tel = new ComTelegram(this)
        {
            Status = ComProtStatus.OK,
            AppData = appData
        };
        tel.InData.Cnt = 0;
        tel.DummyData.Cnt = 0;
        tel.OutData.CopyFrom(byteCmd);


        if (!ComPort.IsConnected())
        {
            tel.Status = ComProtStatus.Error;
            tel.Error = ComProtError.InternalError;
            tel.ErrorText = $"Error ComPort not connected";
            _log.Warning($"[{DeviceCode}] {tel.ErrorText}");
            return await Task.FromResult(tel);
        }
        // perform dialog from description
        for (int descIdx = 0; descIdx < tel.Description.Length; descIdx++)
        {
            var descLine = tel.Description[descIdx];
            if (descLine.StartsWith(";"))
                continue;  // ;comment
            string descCmd;
            string descParam;
            int splitPos = descLine.IndexOf(':');  //first occurance
            try
            {
                descCmd = descLine[0..splitPos];
                descParam = descLine[(splitPos + 1)..];
            }
            catch (Exception ex)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.LengthError;
                tel.ErrorText = $"Error at Description.{descIdx}({descLine}): {ex.Message}";
                break;  // escape for loop
            }
            try
            {
                await RunDescLine(tel, descCmd, descParam);
            }
            catch (Exception ex)
            {
                _log.Information(ex, $"[{DeviceCode}] Error at {descIdx}({descLine})");
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.InternalError;
                tel.ErrorText = $"{ex.Message} Error at {descIdx}({descLine})";
            }
            if (tel.Status != ComProtStatus.OK)
                break;
        }
        if (tel.Status == ComProtStatus.OK)
        {
            // ComProt cleanup, flush:
            await ComPort.FlushAsync();

            // answer data to tel.AppData
            DoAnswer(new TelEventArgs(tel));
        }
        else
        {
            _log.Warning($"RunTelegram [{tel.Error}] {tel.ErrorText}");
        }
        return await Task.FromResult(tel);
    }

    /* *** Description: *** keine Leerzeichen! ***
    L:              Host:Listen - warte bis (Client verbunden und) Empfangsdaten anliegen
    T:m             m = TimeOut in ms (0 = infinity)
    T2:m            m = TimeOut between character
    C:              Block Check Character auf 0 zurücksetzen
    D:b             b: 1=DoubleDle, 0=no DoubleDle
    I:              Empfangspuffer löschen (ClearInput)
    S:^c|a|$xx      Sende Steuerzeichen. c=A..Z, a=0..9,a..z,A..Z, x=0..9,a..f
    B:              Befehl senden
    W:n[:m],d1,d2   Warte auf Steuerzeichen. n=Anzahl[:Mindestanzahl], d1,d2=max. 2 Steuerzeichen
                        die das Ende markieren i.d.F, ^c: c=A..Z,[,\,] oder $xx oder ASCII Zeichen
    A:n[:m],d1,d2   Antwort empfangen. Mit Anzahl oder Steuerzeichen wie bei W:
    ;               Kommentar
    P:m             Pause. m = Zeit in ms
    E:1 bzw. E:0    Echo ein/Ausschalten (ein:erwartet Echo nach jedem Sendezeichen)
    */

    private async Task<bool> RunDescLine(ComTelegram tel, string descCmd, string descParam)
    {
        bool bResult = false;
        _log.Debug($"[{DeviceCode}] {GetType().Name}.RunDescLine({descCmd}:{descParam})");
        if (descCmd == "T")  //T:m - Timeout
        {
            ComPort.ComParameter.TimeoutMs = int.Parse(descParam);
        }
        else if (descCmd == "T2")  //T2:m - Timeout2 between Ch
        {
            ComPort.ComParameter.Timeout2Ms = int.Parse(descParam);
        }
        else if (descCmd == "L")  //L: - Wait until available
        {
            await Task.Run(async () =>
            {
                while (await ComPort.InCountAsync(0) == 0)
                {
                    await Task.Delay(1000);
                }
            });
        }
        else if (descCmd == "P")  //P:m - pause
        {
            await Task.Delay(int.Parse(descParam));
        }
        else if (descCmd == "C")  //C: - Clear BlockCheck
        {
            ComPort.Bcc = 0;
        }
        else if (descCmd == "D")  //D:1 or 0 - Set DoubleDLE
        {
            ComPort.ComParameter.DoubleDle = int.Parse(descParam) == 1;
        }
        else if (descCmd == "E")  //E:1 or 0 - Set Echo
        {
            ComPort.ComParameter.Echo = int.Parse(descParam) == 1;
        }
        else if (descCmd == "I")  //I: - Clear Input Buffer
        {
            await ClearInputAsync(tel.DummyData);
        }
        else if (descCmd == "S")  //S:^c|a|$xx - Send Characters
        {
            bResult = await SendAsync(tel, descParam);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error writing command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "B")  //B: - Send Command
        {
            bResult = await ComPort.WriteAsync(tel.OutData);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error writing command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "W")  //W:n[:m],d1,d2 - wait for cerain characters
        {
            bResult = await Receive(tel.DummyData, descParam);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error reading command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "A")  //A:n[:m],d1,d2 - wait for answer
        {
            bResult = await Receive(tel.InData, descParam);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error reading command.{descCmd}({descParam})";
            }
        }
        else
        {
            tel.Status = ComProtStatus.Error;
            tel.Error = ComProtError.LengthError;
            tel.ErrorText = $"syntax error in description({descCmd}:{descParam})";
        }
        /* if ATel.Reseting then
            begin
              if ComPort <> nil then
                ComPort.InternalError;
              ATel.Status := cpsError;
              ATel.Error := cpeReset;
            end;
        */


        return await Task.FromResult(bResult);
    }

    /// <summary>
    /// Callback of SendAsync. For sending individual characters and/or computing block checks
    /// </summary>
    /// <param name="e"></param>
    private void DoUserFn(UserFnEventArgs e)
    {
        var bb = new ByteBuff(1);
        if (e.UserFn.Equals("BCC", StringComparison.OrdinalIgnoreCase))
        {
            bb.Buff[0] = (byte)ComPort.Bcc;
            bb.Cnt = 1;
            _ = ComPort.Write(bb);
        }
        else
        {
            OnUserFn?.Invoke(this, e);
        }
    }

    #endregion

}
