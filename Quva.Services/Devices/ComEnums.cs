using System.Text;

namespace Quva.Services.Devices;

public class ByteBuff
{
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

    public byte[] Buff { get; set; }
    public int Cnt { get; set; }

    public string DebugString()
    {
        return DebugString(0);
    }

    // for Debug Output
    public string DebugString(int offset)
    {
        var sb = new StringBuilder(256);
        for (var i = offset; i < Cnt; i++)
        {
            var b = Buff[i];
            if (b < 32)
            {
                sb.Append('^'); //01 -> ^A
                b += 64;
            }
            else if (b == 94)
            {
                sb.Append('\\'); //^ -> \^
            }
            sb.Append((char)b);
        }
        return sb.ToString();
    }

    public string HexString()
    {
        var sb = new StringBuilder(256);
        for (var i = 0; i < Cnt; i++)
        {
            var b = Buff[i];
            sb.Append(b.ToString("X3"));
        }
        return sb.ToString();
    }

    // Duplicate other ByteBuff
    //beware bc length - src.Buff.CopyTo(Buff, 0);
    public int CopyFrom(ByteBuff src)
    {
        for (var i = 0; i < src.Cnt; i++)
            Buff[i] = src.Buff[i];
        Cnt = src.Cnt;
        return Cnt;
    }

    /// <summary>
    ///     appends dst offset dst.Cnt with this.Buff offset 0 len Cnt
    /// </summary>
    /// <returns>Count of copied bytes</returns>
    public int AppendTo(ByteBuff dst)
    {
        for (var i = 0; i < Cnt; i++) dst.Buff[dst.Cnt++] = Buff[i];
        return Cnt;
    }

    /// <summary>
    ///     moves max [dst.Cnt] Bytes to empty [dst]. Deletes moved bytes in [src], set [Cnt] on bothe sides.
    /// </summary>
    /// <returns>Count of moved bytes</returns>
    public int MoveTo(ByteBuff dst)
    {
        var maxCnt = dst.Cnt;
        var movedCnt = 0;
        while (Cnt > 0 && movedCnt < maxCnt)
        {
            dst.Buff[movedCnt] = Buff[movedCnt];
            movedCnt++;
            Cnt--;
        }

        dst.Cnt = movedCnt;
        // 123, m:1 c:2 -> 0<-1, 1<-2
        for (var i = 0; i < Cnt; i++) Buff[i] = Buff[i + movedCnt];


        return movedCnt;
    }
}

public enum ComProtStatus
{
    Ok, // successfully completed
    Waiting, // waiting for start
    Hanging, // waiting for finish
    Error // terminated with error
}

public enum ComProtError
{
    Ok, // ok
    TimeOut, // _device timeout
    LengthError, // length or description error
    InternalError // _device will reset
}

[Flags]
public enum ComProtErrorActions
{
    None = 0, // no error logging
    Ignore = 1, // ignore logging
    Show = 2, // show error in UI
    Prot = 4 // output error in logfile
}

[Flags]
public enum ComProtFlags
{
    None = 0,
    Wait = 1, // Warten auf Antwort
    Poll = 2, // Antwort als Ereignis(Standard)}
    OnTop = 4, // vor anderen wartenden ComTelegram.ausführen
    Cache = 8, // Ergebnis für Remotezugriff cachen
    Wdhlg = 16, // Wiederholung bei Fehler(nur SpsProt)
    UseId = 32, // Bestehende ID nochmal verwenden
    Dummy = 64, // ohne Kommunikation gleich onAntwort
    Start = 128 // Sofortstart
}

public enum ReadDataResult
{
    Ok,
    NoMinLen,
    DelimiterMissing
}