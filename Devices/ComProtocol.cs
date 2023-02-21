using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Devices;

public class ComProtocol : IAsyncDisposable
{
    private TimerAsync? AsyncTimer { get; set; }
    private IList<ComTelegram> TelList { get; set; }
    public IComPort? ComPort { get; set; }
    public string[] Description { get; set; }
    public int MaxDataLen { get; set; } = 2048;
    public int ProtTelId { get; set; } = -1;
    public bool DoubleDle { get; set; } = false;
    public bool Echo { get; set; } = false;
    public ComProtErrorActions ErrorActions { get; set; }
    public event EventHandler<TelEventArgs>? OnAnswer;
    public event EventHandler? OnError;
    public event EventHandler<TelEventArgs>? OnUserFn;

    private void DoAnswer(TelEventArgs e) => OnAnswer?.Invoke(this, e);
    private void DoError(EventArgs e) => OnError?.Invoke(this, e);
    private void DoUserFnk(TelEventArgs e) => OnUserFn?.Invoke(this, e);


    public ComProtocol(IComPort? comPort)
    {
        ComPort = comPort;
        TelList = new List<ComTelegram>();
        AsyncTimer = new TimerAsync(OnAsyncTimer,
            TimeSpan.FromMilliseconds(1000), //warten
            TimeSpan.FromMilliseconds(100)); //interval
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
        if (ComPort != null)
        {
            await ComPort.CloseAsync();
        }
        ComPort = null;
    }

    public async ValueTask DisposeAsync()
    {
        Log.Warning($"DisposeAsync()");
        await DisposeAsyncCore().ConfigureAwait(false);

        GC.SuppressFinalize(this);
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

    private void Send(ComTelegram tel, byte[] descToken)
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
    private bool Empf(ComTelegram tel, bool data, byte[] descLine)
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
