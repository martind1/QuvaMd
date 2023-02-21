using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public class ComTelegram : IDisposable
{
    public ComProtocol Owner { get; set; }
    public int Id { get; set; }
    public int Step { get; set; }
    public byte[] OutData { get; set; }
    public int OutDataLen { get; set; }
    public byte[] InData { get; set; }
    public int InDataLen { get; set; }
    public byte[] DummyData { get; set; }
    public int DummyDataLen { get; set; }
    public ComProtFlags Flags { get; set; }
    public ComProtStatus Status { get; set; }
    public ComProtError Error { get; set; }
    public bool DelimRead { get; set; }
    Stopwatch StartTimer { get; set; }
    public long SleepTime { get; set; }
    public string[] Description { get; set; }

    static int TelIdCounter = 0;
    static IList<ComTelegram> TelList = new List<ComTelegram>();

    public object AppData { get; set; }

    public ComTelegram(ComProtocol owner)
    {
        Owner = owner;
        StartTimer = new Stopwatch();
        OutData = new byte[Owner.MaxDataLen + 1];
        InData = new byte[Owner.MaxDataLen + 1];
        DummyData = new byte[Owner.MaxDataLen + 1];
        Description = new string[owner.Description.Length];
        owner.Description.CopyTo(Description, 0);
        Status = ComProtStatus.OK;
        Error = ComProtError.OK;
        Step = 0;
        DelimRead = false;
        Id = ++TelIdCounter;
        TelList.Add(this);
    }

    public void Dispose()
    {
        var i = TelList.IndexOf(this);
        if (i >= 0)
        {
            TelList.RemoveAt(i);
        }
    }
}
