using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public class TelEventArgs : EventArgs
{
    public ComTelegram Tel { get; set; }

    public TelEventArgs(ComTelegram tel)
    {
        Tel = tel;
    }
}


public class UserFnEventArgs : EventArgs
{
    public ComTelegram Tel { get; set; }
    public string UserFn { get; set; }

    public UserFnEventArgs(ComTelegram tel, string userFn)
    {
        Tel = tel;
        UserFn = userFn;
    }
}

public class ComSimulEventArgs : EventArgs
{
    public byte[]? OutData { get; set; }
    public int OutDataLen { get; set; }
    public byte[]? InData { get; set; }
    public int InDataLen { get; set; }
}

public class UserFnkEventArgs : EventArgs
{
    public ComTelegram? tel { get; set; }
    public string? FnkName { get; set; }
}

public class ByteBuff
{
    public byte[] Buff { get; set; }
    public int Cnt { get; set; } = 0;

    public ByteBuff(int buffSize)
    {
        Buff = new byte[buffSize];
        Cnt = 0;
    }

    public ByteBuff(byte[] buff, int len)
    {
        Buff = buff;
        Cnt = len;
    }

    public int CopyFrom(ByteBuff src)
    {
        src.Buff.CopyTo(Buff, 0);
        Cnt = src.Cnt;
        return Cnt;
    }

    /// <summary>
    /// moves max [dst.Cnt] Bytes to empty [dst]. Deletes moved bytes in [src], set [Cnt] on bothe sides.
    /// </summary>
    /// <returns>Count of moved bytes</returns>
    public int MoveTo(ByteBuff dst)
    {
        int maxCnt = dst.Cnt;
        int movedCnt = 0;
        while (Cnt > 0 && movedCnt < maxCnt)
        {
            dst.Buff[movedCnt] = Buff[movedCnt];
            movedCnt++;
            Cnt --;
        }
        dst.Cnt = movedCnt;
        // 123, m:1 c:2 -> 0<-1, 1<-2
        for (int i = 0; i < Cnt; i++)
        {
            Buff[i] = Buff[i + movedCnt];
        }


        return movedCnt;
    }
}

public enum ComProtStatus
{
    OK,      // successfully completed
    Waiting, // waiting for start
    Hanging, // waiting for finish
    Error    // terminated with error
}

public enum ComProtError
{
    OK,      // ok
    TimeOut, // device timeout
    Length,  // length or description error
    Reset    // device will reset
}

[Flags]
public enum ComProtErrorActions
{
    None = 0,   // no error logging
    Ignore = 1, // ignore logging
    Show = 2,   // show error in UI
    Prot = 4    // output error in logfile
}

[Flags]
public enum ComProtFlags
{
    None = 0,
    Wait = 1,       // Warten auf Antwort
    Poll = 2,       // Antwort als Ereignis(Standard)}
    OnTop = 4,      // vor anderen wartenden ComTelegram.ausführen
    Cache = 8,      // Ergebnis für Remotezugriff cachen
    Wdhlg = 16,     // Wiederholung bei Fehler(nur SpsProt)
    UseId = 32,     // Bestehende ID nochmal verwenden
    Dummy = 64,     // ohne Kommunikation gleich onAntwort
    Start = 128     // Sofortstart
}

public enum ReadDataResult
{
    OK,
    NoMinLen,
    DelimiterMissing
}