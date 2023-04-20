using System;
using System.Globalization;
using System.Text;
using System.Windows;
using Serilog;
using System.Windows.Controls;
using System.Configuration;
using Quva.Services.Devices;
using Quva.Services.Devices.Simul;
using Quva.Services.Interfaces.Shared;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for ScaleIT9000Window.xaml
/// </summary>
public partial class ScaleIT9000Window : Window
{
    private readonly IDeviceService _svc;
    private DeviceOptions? _deviceOptions;
    private DeviceOptions deviceOptions { get => _deviceOptions ?? throw new ArgumentNullException(nameof(_deviceOptions)); }
    private ComDevice? _comDevice;

    public ScaleIT9000Window(IDeviceService svc)
    {
        transferObject = new();
        InitializeComponent();
        _svc = svc;
    }

    private void IT9000Window_Closed(object sender, EventArgs e)
    {
        _svc.CloseDevice("SIM.IT9000");
        Application.Current.MainWindow.Show();
        Hide();

    }

    private async void BtnStartSimul_Click(object sender, RoutedEventArgs e)
    {
        _comDevice = await _svc.SimulCommandStart("SIM.IT9000", IT9000Simul);
        _deviceOptions = _comDevice?.Options;

        edCalibrationNumber.Text = DateTime.Now.ToString("HHmm");
    }

    private void IT9000Simul(ComTelegram tel)
    {
        var inStr = Encoding.ASCII.GetString(tel.InData.Buff, 0, tel.InData.Cnt);


        //bool isStatus = inStr.Equals("<RM>");
        bool isRegister = inStr.Equals("<RN>");
        double minWeight = deviceOptions.Option("MinWeight", 0.0);
        double maxWeight = deviceOptions.Option("MaxWeight", 49999.0);

        var simulData = new SimulData("IT9000", inStr);  //<RM> or <RN>
        //avoid System.InvalidOperationException Cross-thread operation not valid:
        //Dispatcher.BeginInvoke(new Action(() =>
        //{
        simulData.ErrorNr = 0;
        simulData.Weight = double.Parse(transferObject.edWeight_Text);
        simulData.CalibrationNumber = 0;
        simulData.unitStr = "kg";
        simulData.Stillstand = transferObject.chbStandStill_Checked; // ?? false;
        simulData.Negative = transferObject.edWeight_Text.Contains('-');

        if (isRegister)
        {
            simulData.CalibrationNumber = int.Parse(transferObject.edCalibrationNumber_Text);
            transferObject.edCalibrationNumber_Text = (simulData.CalibrationNumber + 1).ToString();
            if (simulData.Weight < minWeight)
                simulData.ErrorNr = 20;  //Unterlast
            else if (simulData.Weight > maxWeight)
                simulData.ErrorNr = 12;  //Überlast
            else if (!simulData.Stillstand)
                simulData.ErrorNr = 13;  //Waage in Bewegung
        }
        else
        {
            if (simulData.Weight > maxWeight)
                simulData.ErrorNr = 12;  //Überlast
        }
        //}));
        //write the answer to send:
        Services.Devices.Simul.IT9000Simul.OnIT9000Simul(tel, simulData);
    }

    private void WeightSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        edWeight.Text = (Math.Round(e.NewValue / 20, 0) * 20).ToString("F0", CultureInfo.CreateSpecificCulture("de-DE"));
        SetTranfsferObject();
    }

    private void edCalibrationNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
        SetTranfsferObject();
    }

    private void chbStandstill_Checked(object sender, RoutedEventArgs e)
    {
        SetTranfsferObject();
    }

    private record TransferRecord
    {
        internal string edWeight_Text = string.Empty;
        internal string edCalibrationNumber_Text = string.Empty;
        internal bool chbStandStill_Checked;
    }
    private readonly TransferRecord transferObject;

    private void SetTranfsferObject()
    {
        if (!Dispatcher.CheckAccess()) // CheckAccess returns true if you're on the dispatcher thread
        {
            Dispatcher.Invoke(new ParametrizedMethodInvoker(SetTranfsferObject));
            return;
        }
        transferObject.edWeight_Text = edWeight?.Text ?? string.Empty;
        transferObject.edCalibrationNumber_Text = edCalibrationNumber?.Text ?? string.Empty;
        transferObject.chbStandStill_Checked = chbStandstill?.IsChecked ?? false;
    }

    private delegate void ParametrizedMethodInvoker();
}