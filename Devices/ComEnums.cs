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
