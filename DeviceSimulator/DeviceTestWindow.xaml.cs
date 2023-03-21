using System;
using System.Globalization;
using System.Windows;
using Quva.Devices;
using Quva.Devices.Scale;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for DeviceTestWindow.xaml
/// </summary>
public partial class DeviceTestWindow : Window
{
    private string? _scaleStatus;
    private readonly IDeviceService svc;

    public DeviceTestWindow(IDeviceService svc)
    {
        InitializeComponent();
        this.svc = svc;
    }

    private void DeviceTestWindow_Closed(object sender, EventArgs e)
    {
        svc.CloseDevice("HOH.FW2");
        svc.CloseDevice("HOH.DISP1");
        Application.Current.MainWindow.Show();
        Close();
    }

    private void protLb(string message)
    {
        if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
        {
            Dispatcher.Invoke(new ParametrizedMethodInvoker(protLb), message);
            return;
        }

        message = DateTime.Now.ToString("T") + " " + message;
        lbProt.Items.Insert(0, message);
        while (lbProt.Items.Count > 1000) lbProt.Items.RemoveAt(1000);
    }

    private void DeviceTestWindow_Initialized(object sender, EventArgs e)
    {
        //Service anlegen
        //svc = new DeviceService();
        protLb("DeviceService created");
    }

    private async void BtnDisplayShow_Click(object sender, RoutedEventArgs e)
    {
        //C:\Portlistener\listener.exe 14080
        var message = DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE"));
        var data = await svc.DisplayShow("HOH.DISP1", message);
        protLb($"DisplayShow Err:{data.ErrorNr} {data.ErrorText} Msg:{data.Message}");
    }

    private async void BtnCardRead_Click(object sender, RoutedEventArgs e)
    {
        //telnet localhost 14070
        var data = await svc.CardRead("HOH.TRANSP1");
        protLb($"CardRead Err:{data.ErrorNr} {data.ErrorText} Card:{data.CardNumber}");
    }

    private async void BtnScaleRegister_Click(object sender, RoutedEventArgs e)
    {
        //ShTcpSvr
        var data = await svc.ScaleRegister("HOH.FW2");
        protLb(
            $"ScaleRegister Err:{data.ErrorNr} Display:{data.Display} Eichnr:{data.CalibrationNumber} Weight:{data.Weight} Unit:{data.Unit}");
    }

    private async void BtnScaleStatusStart_Click(object sender, RoutedEventArgs e)
    {
        var result = await svc.ScaleStatusStart("HOH.FW2", MyScaleStatus);
        protLb("ScaleStatusStart Started");
    }

    private void MyScaleStatus(ScaleData scaleData)
    {
        if (_scaleStatus != scaleData.Display)
        {
            _scaleStatus = scaleData.Display;
            protLb($"### ScaleStatus {scaleData.Display} ###");
        }
    }

    private async void BtnDisplayScale_Click(object sender, RoutedEventArgs e)
    {
        var result = await svc.ScaleStatusStart("HOH.FW2", MyScaleStatus);
        protLb("ScaleStatus Started");

        _ = await svc.DisplayShowScale("HOH.DISP1", "HOH.FW2");
        protLb("DisplayShow Started");
    }

    private delegate void ParametrizedMethodInvoker(string message);
}