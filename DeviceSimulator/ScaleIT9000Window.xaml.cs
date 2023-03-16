using System;
using System.Globalization;
using System.Windows;
using Quva.Devices;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for ScaleIT9000Window.xaml
/// </summary>
public partial class ScaleIT9000Window : Window
{
    public ScaleIT9000Window()
    {
        InitializeComponent();
    }

    private void IT9000Window_Closed(object sender, EventArgs e)
    {
        Application.Current.MainWindow.Show();
        Close();
    }

    private void BtnStartSimul_Click(object sender, RoutedEventArgs e)
    {
        var it9000Port = new TcpPort("IT9000", "Listen:12040");
        var it9000Prot = new ComProtocol("IT9000", it9000Port);
        if (it9000Prot != null)
        {
        }
    }

    private void WeightSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        edWeight.Text = (Math.Round(e.NewValue * 5, 0) / 5).ToString("F", CultureInfo.CreateSpecificCulture("de-DE"));
    }
}