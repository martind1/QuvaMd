using Quva.DeviceSimulator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Quva.DeviceSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
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
            this.Hide();
        }

        private void BtnDeviceTests_Click(object sender, RoutedEventArgs e)
        {
            //var win = new DeviceTestWindow();
            var app = (App)Application.Current;
            var sprovider = app.host.Services;
            ArgumentNullException.ThrowIfNull(sprovider);
            var win = sprovider.GetRequiredService<DeviceTestWindow>();
            win.Show();
            this.Hide();
        }
    }
}
