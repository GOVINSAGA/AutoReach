using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;
using AutoReach.Models;
using AutoReach.Services;

namespace AutoReach;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cts;
    private readonly EmailService _emailService = new();
    private readonly string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_settings.json");

    public MainWindow()
    {
        InitializeComponent();
        LoadUserSettings();
    }

    private void LoadUserSettings()
    {
        if (File.Exists(_settingsPath))
        {
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<EmailSettings>(json);
            if (settings != null)
            {
                TxtEmail.Text = settings.Address;
                TxtPassword.Password = settings.AppPassword;
                TxtSubject.Text = settings.Subject;
                TxtBody.Text = settings.TemplateBody; // Added this line
                TxtResumePath.Text = settings.ResumePath;
                TxtListPath.Text = settings.EmailListPath;
            }
        }
    }

    private void SaveUserSettings(EmailSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(_settingsPath, json);
    }

    private void BrowseResume_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf" };
        if (dialog.ShowDialog() == true) TxtResumePath.Text = dialog.FileName;
    }

    private void BrowseList_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
        if (dialog.ShowDialog() == true) TxtListPath.Text = dialog.FileName;
    }

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        // 1. Create settings from UI input
        var settings = new EmailSettings
        {
            Address = TxtEmail.Text,
            AppPassword = TxtPassword.Password,
            Subject = TxtSubject.Text,
            TemplateBody = TxtBody.Text, // Grab the text from the UI here
            ResumePath = TxtResumePath.Text,
            EmailListPath = TxtListPath.Text,
            DailyLimit = 50,
            SentListPath = Path.Combine(Path.GetDirectoryName(TxtListPath.Text) ?? "", "sent_emails.txt")
        };

        // 2. Persist for next time
        SaveUserSettings(settings);

        // 3. Start processing
        _cts = new CancellationTokenSource();
        BtnStart.IsEnabled = false;
        BtnStop.IsEnabled = true;

        var progress = new Progress<string>(msg => {
            LogTerminal.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            LogTerminal.ScrollToEnd();
        });

        try
        {
            await _emailService.ProcessEmailsAsync(settings, progress, _cts.Token);
            MessageBox.Show("All emails sent successfully!", "Complete");
        }
        catch (Exception ex)
        {
            LogTerminal.AppendText($"ERROR: {ex.Message}\n");
        }
        finally
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e) => _cts?.Cancel();
}