using Microsoft.AspNetCore.Components.Web;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public class ComProtocol : IAsyncDisposable
{
    private TimerAsync? AsyncTimer { get; set; }
    private IComPort? comPort = null;
    public IComPort ComPort { get => comPort ?? throw new ArgumentNullException(nameof(comPort)); set => comPort = value; }
    public string[] Description { get; set; }
    public ComProtErrorActions ErrorActions { get; set; }
    public event EventHandler<TelEventArgs>? OnAnswer;
    public event EventHandler? OnError;
    public event EventHandler<UserFnEventArgs>? OnUserFn;
    private void DoAnswer(TelEventArgs e) => OnAnswer?.Invoke(this, e);
    private void DoError(EventArgs e) => OnError?.Invoke(this, e);
    private void DoUserFnk(UserFnEventArgs e) => OnUserFn?.Invoke(this, e);
    //Config:
    public int MaxDataLen { get; set; } = 2048;
    public int ProtTelId { get; set; } = -1;
    public bool DoubleDle { get; set; } = false;
    public bool Echo { get; set; } = false;


    public ComProtocol(IComPort? comPort)
    {
        this.comPort = comPort;
        Description = new string[] { "B:", "S:^M", "A:128,^M" }; //nur Demo
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        Log.Warning($"DisposeAsyncCore({AsyncTimer != null})");
        if (AsyncTimer != null)
        {
            await Task.Run(() => { AsyncTimer.Dispose(); });
        }
        AsyncTimer = null;
        if (comPort != null)
        {
            await comPort.CloseAsync();
        }
        comPort = null;
    }

    public async ValueTask DisposeAsync()
    {
        Log.Warning($"DisposeAsync()");
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
        ByteBuff tempBuff = new ByteBuff(1024);  //Len = 0
        // evaluate params and send bytes
        for (int i = 0; i < descParam.Length; i++)
        {
            if (descParam[i] == '^' && i < descParam.Length - 1 && (byte)descParam[i + 1] > 64)
            {
                i++;
                tempBuff.Buff[tempBuff.Cnt++] = (byte)(((byte)descParam[i]) - 64);
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
                DoUserFnk(new UserFnEventArgs(tel, userFnk));
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
        if (comPort != null)
        {
            dummydata.Cnt = await ComPort.InCountAsync();
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
        ByteBuff tmpBuff = new ByteBuff(MaxDataLen);

        // erstes Zeichen lesen mit timeout. Danach nur wenn InCount > 0.
        tmpBuff.Cnt = Math.Max(1, minLen);
        int n = await ComPort.ReadAsync(tmpBuff);
        tmpBuff.AppendTo(data);
        if (n == 0 || n < minLen)
        {
            result = ReadDataResult.NoMinLen;
            return await Task.FromResult(result);
        }
        if (delim1 != 0)
            result = ReadDataResult.DelimiterMissing;
        while (await ComPort.InCountAsync() > 0)
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
    private async Task<bool> Receive(ByteBuff data, bool answer, string descParam)
    {
        var delimiter = new byte[] { 0, 0 };
        int nDelim = 0;
        bool delimReached;
        int MaxLen = MaxDataLen;
        int MinLen = 0;
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
                else
                {
                    var slLen = slTok[i].Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    MaxLen = int.Parse(slLen[0]);
                    MinLen = slLen.Length >= 2 ? int.Parse(slLen[1]) : 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"error description({descParam})", nameof(descParam), ex);
        }

        data.Cnt = 0;  //read from 0. Moves Cnt.
        ReadDataResult rdResult;
        rdResult = await ReadDataAsync(data, MinLen, MaxLen, delimiter[0], delimiter[1]);
        //nochmal timeout lesen wenn mindestens 1 Zeichen gelesen und Len<minLen oder delimiter nicht erreicht
        if (rdResult != ReadDataResult.OK && data.Cnt > 0 && (data.Cnt < MinLen || rdResult == ReadDataResult.DelimiterMissing))
        {
            delimReached = rdResult != ReadDataResult.DelimiterMissing;
            MinLen = Math.Max(0, MinLen - data.Cnt);
            rdResult = await ReadDataAsync(data, MinLen, MaxLen, delimiter[0], delimiter[1]);
        }
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
        var tel = new ComTelegram(this);
        tel.Status = ComProtStatus.OK;
        tel.AppData = appData;
        tel.InData.Cnt = 0;
        tel.DummyData.Cnt = 0;
        tel.OutData.CopyFrom(byteCmd);


        if (!ComPort.IsConnected())
        {
            tel.Status = ComProtStatus.Error;
            tel.Error = ComProtError.Reset;
            tel.ErrorText = $"Error ComPort not connected";
            return await Task.FromResult(tel);
        }

        // perform dialog from description
        for (int descIdx = 0; descIdx < tel.Description.Length; descIdx++)
        {
            var descLine = tel.Description[descIdx];
            if (descLine.StartsWith(";"))
                continue;  // ;comment
            string descCmd = string.Empty;
            string descParam = string.Empty;
            int splitPos = descLine.IndexOf(':');  //first occurance
            try
            {
                descCmd = descLine[0..(splitPos - 1)];
                descParam = descLine[(splitPos + 1)..];
            }
            catch (Exception ex)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.Length;
                tel.ErrorText = $"Error at Description.{descIdx}({descLine}): {ex.Message}";
                break;  // escape for loop
            }
            await RunDescLine(tel, descCmd, descParam);
            if (tel.Status != ComProtStatus.OK)
                break;
        }
        // ComProt cleanup, flush:
        await ComPort.FlushAsync();

        // answer data to tel.AppData
        DoAnswer(new TelEventArgs(tel));

        return await Task.FromResult(tel);
    }

    /* *** Description: *** keine Leerzeichen! ***
    T:m             m = TimeOut in ms
    C:              Block Check Character auf 0 zurücksetzen
    D:b             b: 1=DoubleDle, 0=no DoubleDle
    I:              Empfangspuffer löschen (ClearInput)
    S:^c|a|$xx      Sende Steuerzeichen. c=A..Z, a=0..9,a..z,A..Z, x=0..9,a..f
    B:              Befehl senden
    W:n[:m],d1,d2   Warte auf Steuerzeichen. n=Anzahl[:Mindestanzahl], d1,d2=max. 2 Steuerzeichen
                        die das Ende markieren i.d.F, ^c: c=A..Z,[,\,] oder $xx
    A:n[:m],d1,d2   Antwort empfangen. Mit Anzahl oder Steuerzeichen wie bei W:
    ;               Kommentar
    P:m             Pause. m = Zeit in ms
    E:1 bzw. E:0    Echo ein/Ausschalten (ein:erwartet Echo nach jedem Sendezeichen)
    */

    private async Task<bool> RunDescLine(ComTelegram tel, string descCmd, string descParam)
    {
        bool bResult = false;

        if (descCmd == "T")  //T:m
        {
            ComPort.ComParameter.TimeoutMs = int.Parse(descParam);
        }
        else if (descCmd == "C")  //C:
        {
            ComPort.Bcc = 0;
        }
        else if (descCmd == "D")  //D:1 or 0
        {
            ComPort.ComParameter.DoubleDle = int.Parse(descParam) == 1;
        }
        else if (descCmd == "E")  //E:1 or 0
        {
            ComPort.ComParameter.Echo = int.Parse(descParam) == 1;
        }
        else if (descCmd == "I")  //I:
        {
            await ClearInputAsync(tel.DummyData);
        }
        else if (descCmd == "S")  //S:^c|a|$xx
        {
            bResult = await SendAsync(tel, descParam);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error writing command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "B")  //B:
        {
            bResult = await ComPort.WriteAsync(tel.OutData);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error writing command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "W")  //W:n[:m],d1,d2
        {
            bResult = await Receive(tel.DummyData, false, descParam);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error reading command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "A")  //A:n[:m],d1,d2
        {
            bResult = await Receive(tel.InData, false, descParam);
            if (!bResult)
            {
                tel.Status = ComProtStatus.Error;
                tel.Error = ComProtError.TimeOut;
                tel.ErrorText = $"Error reading command.{descCmd}({descParam})";
            }
        }
        else if (descCmd == "P")  //P:m
        {
            await Task.Delay(int.Parse(descParam));
        }
        else
        {
            tel.Status = ComProtStatus.Error;
            tel.Error = ComProtError.Length;
            tel.ErrorText = $"syntax error in description {descCmd}:{descParam}";
        }
        /* if ATel.Reseting then
            begin
              if ComPort <> nil then
                ComPort.Reset;
              ATel.Status := cpsError;
              ATel.Error := cpeReset;
            end;
        */


        return await Task.FromResult(bResult);
    }

    #endregion

}
