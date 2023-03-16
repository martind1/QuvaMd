using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BtnIT9000_Click(object sender, RoutedEventArgs e)
    {
        var win = new ScaleIT9000Window();
        win.Show();
        Hide();
    }

    private void BtnDeviceTests_Click(object sender, RoutedEventArgs e)
    {
        //var win = new DeviceTestWindow();
        var app = (App)Application.Current;
        var sprovider = app.host.Services;
        ArgumentNullException.ThrowIfNull(sprovider);
        var win = sprovider.GetRequiredService<DeviceTestWindow>();
        win.Show();
        Hide();
    }
}