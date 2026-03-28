using System.Windows;
using AutoReach.Models;
using AutoReach.Services;
using Microsoft.Extensions.Configuration;

namespace AutoReach;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cts;
    private readonly EmailService _emailService = new();

    public MainWindow() => InitializeComponent();

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        _cts = new CancellationTokenSource();
        BtnStart.IsEnabled = false;
        BtnStop.IsEnabled = true;
        LogTerminal.Clear();

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var settings = new EmailSettings();
            config.GetSection("Email").Bind(settings);
            config.GetSection("Files").Bind(settings);

            var progress = new Progress<string>(msg => {
                LogTerminal.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                LogTerminal.ScrollToEnd();
            });

            await _emailService.ProcessEmailsAsync(settings, progress, _cts.Token);
        }
        catch (Exception ex)
        {
            LogTerminal.AppendText($"CRITICAL ERROR: {ex.Message}\n");
        }
        finally
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e) => _cts?.Cancel();
}