using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices
{
    public class TelEventArgs : EventArgs
    {
        public int TelId { get; set; }
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
        public Tel? tel { get; set; }
        public string? FnkName { get; set; }
    }

    public class ComProt: IDisposable
    {
        private TimerAsync AsyncTimer { get; set; }
        private IList<Tel> TelList { get; set; }
        public IComPort ComPort { get; set; }
        public string[] Description { get; set; }
        public int MaxDataLen { get; set; } = 2048;
        public int ProtTelId { get; set; } = -1;
        public bool DoubleDle { get; set; } = false;
        public bool Echo { get; set; } = false;
        public ComProtErrorActions ErrorActions { get; set; }
        public event EventHandler<TelEventArgs>? DoAntwort;
        public void OnAntwort(TelEventArgs e) => DoAntwort?.Invoke(this, e);
        public event EventHandler? DoError;
        public void OnError(EventArgs e) => DoError?.Invoke(this, e);
        public event EventHandler<TelEventArgs>? DoUserFnk;
        public void OnUserFnk(TelEventArgs e) => DoUserFnk?.Invoke(this, e);


        public ComProt(IComPort comPort)
        {
            ComPort = comPort;
            TelList = new List<Tel>();
            TimeSpan dueTime = TimeSpan.FromMilliseconds(1000); //warten
            TimeSpan period = TimeSpan.FromMilliseconds(100); //interval
            AsyncTimer = new TimerAsync(OnAsyncTimer, dueTime, period);
            Description = new string[] { "B:DATA", "S:^M", "A:128,^M" }; //nur Demo


        }

        public void Dispose()
        {
            //ComPort.Dispose Nein, extra!
            AsyncTimer.Dispose();
        }

        public int Start(byte[] befehl, int beflen, ComProtFlags flags, int useId)
        {
            int result = 0;


            return result;
        }

        private async Task OnAsyncTimer(CancellationToken arg)
        {
            DoProt();
            await Task.Delay(0);
        }

        private void Send(Tel tel, byte[] descToken)
        {
            //
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
        /// Empf: Antwort empfangen bzgl Description Zeile
        /// </summary>
        private bool Empf(Tel tel, bool data, byte[] descLine)
        {
            bool result = false;
            // TODO: Empf: add code

            return result;
        }

        /// <summary>
        /// Hauptroutine zum Abarbeiten der offenen Telegramme
        /// </summary>
        private void DoProt()
        {

        }

    }

    public class Tel : IDisposable
    {
        public ComProt Owner { get; set; }
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
        static IList<Tel> TelList = new List<Tel>();

        public Tel(ComProt owner)
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


    public enum ComProtStatus
    {
        OK, Waiting, Hanging, Error
    }

    public enum ComProtError
    {
        OK, TimeOut, Length, Reset
    }

    [Flags]
    public enum ComProtErrorActions
    {
        None = 0,
        cpeOK = 1,
        cpeTimeOut = 2,
        cpeLength = 4,
        cpeReset = 8
    }

    [Flags]
    public enum ComProtFlags
    {
        None = 0,
        Wait = 1,       // Warten auf Antwort
        Poll = 2,       // Antwort als Ereignis(Standard)}
        OnTop = 4,      // vor anderen wartenden Tel.ausführen
        Cache = 8,      // Ergebnis für Remotezugriff cachen
        Wdhlg = 16,     // Wiederholung bei Fehler(nur SpsProt)
        UseId = 32,     // Bestehende ID nochmal verwenden
        Dummy = 64,     // ohne Kommunikation gleich onAntwort
        Start = 128     // Sofortstart
    }
}
