using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public class ComTelegram 
{
    public ComProtocol Owner { get; set; }
    public int Id { get; set; }
    public int Step { get; set; }
    public ByteBuff OutData { get; set; }
    public ByteBuff InData { get; set; }
    public ByteBuff DummyData { get; set; }
    public ComProtFlags Flags { get; set; }
    public ComProtStatus Status { get; set; }
    public ComProtError Error { get; set; }
    public string ErrorText { get; set; }
    public bool DelimRead { get; set; }
    public Stopwatch StartTimer { get; set; }
    public long SleepTime { get; set; }
    public string[] Description { get; set; }

    public object? AppData { get; set; }  //for callbacks and result

    public ComTelegram(ComProtocol owner)
    {
        Owner = owner;
        StartTimer = new Stopwatch();
        OutData = new ByteBuff(Owner.MaxDataLen + 1);
        InData = new ByteBuff(Owner.MaxDataLen + 1);
        DummyData = new ByteBuff(Owner.MaxDataLen + 1);
        Description = new string[owner.Description.Length];
        owner.Description.CopyTo(Description, 0);
        Status = ComProtStatus.OK;
        Error = ComProtError.OK;
        ErrorText = string.Empty;
        Step = 0;
        DelimRead = false;
    }
}
