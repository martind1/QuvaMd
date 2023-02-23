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
    private void Send(ComTelegram tel, string descParam)
    {
        byte[] tempBuff = new byte[1024];
        int n = 0;
        // evaluate params and send bytes
        for (int i = 0; i < descParam.Length; i++)
        {
            if (descParam[i] == '^' && i < descParam.Length - 1 && (byte)descParam[i+1] > 64)
            {
                i++;
                tempBuff[n++] = (byte)(((byte)descParam[i]) - 64);
            }
            else if (descParam[i] == '$' && i < descParam.Length - 2)
            {
                tempBuff[n++] = Convert.ToByte(descParam[(i + 1)..(i + 2)], 16);
                i += 2;
            }
            // abc[def]g  i=3, p=4 -> i=7, userFn=4..6='def'
            // 012345678
            else if (descParam[i] == '[' && descParam[i..].IndexOf(']') > 0)
            {
                var p = descParam[i..].IndexOf(']');
                string userFnk = descParam[(i+1)..(i + p - 1)];
                i += p;
                DoUserFnk(new UserFnEventArgs(tel, userFnk));
            }
            else
            {
                tempBuff[n++] = (byte)descParam[i];
            }
        }
        if (n > 0)
        {
            ComPort.Write(tempBuff, n);
        }
    }

    /// <summary>
    /// Clear input buffer
    /// </summary>
    /// <returns>cleared readed bytes</returns>
    private byte[] ClearInput()
    {
        byte[] buff = Array.Empty<byte>();
        if (comPort != null)
        {
            var cnt = ComPort.InCount();
            buff = new byte[cnt];
            ComPort.Read(buff, cnt);
        }
        return buff;
    }

    /// <summary>
    /// ReadData: Daten von Port lesen mit Bedingungen (Länge, Endezeichen)
    /// </summary>
    /// <param name="data">Puffer für Empfangsdaten</param>
    /// <param name="len">bekommt Sollwert, liefert Istwert</param>
    /// <returns>false bedeutet nur daß noch nicht alle Zeichen eingelesen wurden und kein Abbruch des Telegramms</returns>
    private bool ReadData(byte[] data, ref uint len, byte delim1, byte delim2, ref bool delimRead)
    {
        bool result = false;

        // TODO: ReadData: add some code here

        return result;
    }

    /// <summary>
    /// Receive answer and/or tokens resp description
    /// </summary>
    /// <param name="data">true = get answer data, false = only wait for tokens</param>
    private async Task<ComTelegram> Receive(ComTelegram tel, bool data, string descParam)
    {
        // TODO: Empf: add code

        return await Task.FromResult(tel);
    }

    #endregion

    #region Telegrams

    public async Task<ComTelegram> RunTelegram(object appData, string strCmd)
    {
        byte[] byteCmd = Encoding.ASCII.GetBytes(strCmd);
        int len = byteCmd.Length;
        return await RunTelegram(appData, byteCmd, len);
    }

    public async Task<ComTelegram> RunTelegram(object appData, byte[] byteCmd, int len)
    {
        // in constructor: description, buffer, ..
        var tel = new ComTelegram(this)
        {
            Status = ComProtStatus.OK,
            AppData = appData,
            OutDataLen = len,
            InDataLen = 0,
            DummyDataLen = 0
        };
        byteCmd.CopyTo(tel.OutData, 0);

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
            tel = await RunDescLine(tel, descCmd, descParam);
            if (tel.Status != ComProtStatus.OK)
                break;
        }
        // TODO: ComProt cleanup, flush?
        return await Task.FromResult(tel);
    }

    /* *** Description: *** keine Leerzeichen! ***
    T:m             m = TimeOut in ms
    C:              Block Check Character auf 0 zurücksetzen
    D:b             b: 1=DoubleDle, 0=no DoubleDle
    I:              Empfangspuffer löschen (ClearInput)
    S:^c|a|$xx      Sende Steuerzeichen. c=A..Z, a=0..9,a..z,A..Z, x=0..9,a..f
    B:              Befehl senden
    W:n[:m]|d1,d2   Warte auf Steuerzeichen. n=Anzahl[:Mindestanzahl], d1,d2=max. 2 Steuerzeichen
                        die das Ende markieren i.d.F, ^c: c=A..Z,[,\,] oder $xx
    A:n[:m],d1,d2   Antwort empfangen. Mit Anzahl oder Steuerzeichen wie bei W:
    ;               Kommentar
    P:m             Pause. m = Zeit in ms
    E:1 bzw. E:0    Echo ein/Ausschalten (ein:erwartet Echo nach jedem Sendezeichen)
    */

    private async Task<ComTelegram> RunDescLine(ComTelegram tel, string descCmd, string descParam)
    {
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
            ClearInput();
        }
        else if (descCmd == "S")  //S:^c|a|$xx
        {
            Send(tel, descParam);
        }
        else if (descCmd == "B")  //B:
        {
            ComPort.Write(tel.OutData, tel.OutDataLen);
        }
        else if (descCmd == "W")  //W:n[:m]|d1,d2
        {
            tel = await Receive(tel, false, descParam);
        }
        else if (descCmd == "A")  //A:n[:m],d1,d2
        {
            tel = await Receive(tel, true, descParam);
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

        // answer data to tel.AppData
        DoAnswer(new TelEventArgs(tel));

        return await Task.FromResult(tel);
    }

    #endregion

}
