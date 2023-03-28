using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Quva.DeviceSimulator;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool AutoScroll = true;

    public MainWindow()
    {
        InitializeComponent();
        TextRange tr = new TextRange(LogRichTextBox.Document.ContentStart, LogRichTextBox.Document.ContentEnd);
        tr.Text = "";
        LogRichTextBox.Loaded += (s, e) =>
        {
            var scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(LogRichTextBox, 0), 0) as ScrollViewer;
            ArgumentNullException.ThrowIfNull(scrollViewer);
            scrollViewer.ScrollChanged += (scroller, eScroller) => ScrollViewer_ScrollChanged(scroller, eScroller);
        };

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(App.configuration)
            .WriteTo.RichTextBox(LogRichTextBox)
            .CreateLogger();
        Log.Information("Serilog started ...");
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var scrollViewer = e.Source as ScrollViewer;
        // User scroll event : set or unset autoscroll mode
        if (scrollViewer != null && e.ExtentHeightChange == 0)
        {   // Content unchanged : user scroll event
            if ((scrollViewer).VerticalOffset == (scrollViewer).ScrollableHeight)
            {   // Scroll bar is in bottom
                // Set autoscroll mode
                AutoScroll = true;
            }
            else
            {   // Scroll bar isn't in bottom
                // Unset autoscroll mode
                AutoScroll = false;
            }
        }

        // Content scroll event : autoscroll eventually
        if (AutoScroll && e.ExtentHeightChange != 0 && scrollViewer != null)
        {   // Content changed and autoscroll mode set
            // Autoscroll
            (scrollViewer).ScrollToVerticalOffset((scrollViewer).ExtentHeight);
        }
    }

    private void BtnIT9000_Click(object sender, RoutedEventArgs e)
    {
        //var win = new ScaleIT9000Window();
        //var app = (App)Application.Current;
        ArgumentNullException.ThrowIfNull(App.host);
        var sprovider = App.host.Services;
        ArgumentNullException.ThrowIfNull(sprovider);
        var win = sprovider.GetRequiredService<ScaleIT9000Window>();
        win.Show();
        //Hide();
    }

    private void BtnDeviceTests_Click(object sender, RoutedEventArgs e)
    {
        //var win = new DeviceTestWindow();
        ArgumentNullException.ThrowIfNull(App.host);
        var sprovider = App.host.Services;
        ArgumentNullException.ThrowIfNull(sprovider);
        var win = sprovider.GetRequiredService<DeviceTestWindow>();
        win.Show();
        //Hide();
    }
}