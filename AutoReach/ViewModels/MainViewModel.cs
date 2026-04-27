using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AutoReach.Models;
using AutoReach.Services;

namespace AutoReach.ViewModels;

/// <summary>
/// ViewModel for <see cref="AutoReach.MainWindow"/>.
/// Owns all presentation state and orchestrates the email batch workflow.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    // ── Dependencies ────────────────────────────────────────────────────────
    private readonly IEmailService _emailService;
    private readonly string _settingsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_settings.json");

    // ── Cancellation ────────────────────────────────────────────────────────
    private CancellationTokenSource? _cts;

    // ── Observable Collections ───────────────────────────────────────────────
    /// <summary>Activity log entries shown in the activity DataGrid.</summary>
    public ObservableCollection<ActivityLog> ActivityLogs { get; } = new();

    // ── Commands ────────────────────────────────────────────────────────────
    public ICommand StartCommand { get; }
    public ICommand StopCommand  { get; }

    // ── Bindable Properties ─────────────────────────────────────────────────
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsIdle));
        }
    }
    public bool IsIdle => !_isBusy;

    private string _senderName = string.Empty;
    public string SenderName
    {
        get => _senderName;
        set { _senderName = value; OnPropertyChanged(); }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    private string _appPassword = string.Empty;
    public string AppPassword
    {
        get => _appPassword;
        set { _appPassword = value; OnPropertyChanged(); }
    }

    private string _subject = "Job Application";
    public string Subject
    {
        get => _subject;
        set { _subject = value; OnPropertyChanged(); }
    }

    private string _templateBody = string.Empty;
    public string TemplateBody
    {
        get => _templateBody;
        set { _templateBody = value; OnPropertyChanged(); }
    }

    private int _dailyLimit = 50;
    public int DailyLimit
    {
        get => _dailyLimit;
        set { _dailyLimit = value; OnPropertyChanged(); }
    }

    private string _smtpHost = "smtp.gmail.com";
    public string SmtpHost
    {
        get => _smtpHost;
        set { _smtpHost = value; OnPropertyChanged(); }
    }

    private int _smtpPort = 587;
    public int SmtpPort
    {
        get => _smtpPort;
        set { _smtpPort = value; OnPropertyChanged(); }
    }

    private string _resumePath = string.Empty;
    public string ResumePath
    {
        get => _resumePath;
        set { _resumePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(ResumePathDisplay)); }
    }

    private string _emailListPath = string.Empty;
    public string EmailListPath
    {
        get => _emailListPath;
        set { _emailListPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(EmailListPathDisplay)); }
    }

    // ── Display Helpers ──────────────────────────────────────────────────────
    public string ResumePathDisplay    => string.IsNullOrWhiteSpace(ResumePath)    ? "Drag-and-drop your resume" : ResumePath;
    public string EmailListPathDisplay => string.IsNullOrWhiteSpace(EmailListPath) ? "Drag-and-drop email list"  : EmailListPath;

    // ── Constructor ──────────────────────────────────────────────────────────
    public MainViewModel() : this(new EmailService()) { }

    public MainViewModel(IEmailService emailService)
    {
        _emailService = emailService;
        StartCommand  = new RelayCommand(async _ => await StartBatchAsync(), _ => IsIdle);
        StopCommand   = new RelayCommand(_ => StopBatch(), _ => IsBusy);

        LoadUserSettings();
    }

    // ── Settings Persistence ─────────────────────────────────────────────────
    private void LoadUserSettings()
    {
        if (!File.Exists(_settingsPath)) return;

        try
        {
            var json     = File.ReadAllText(_settingsPath, Encoding.UTF8);
            var options  = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var settings = JsonSerializer.Deserialize<EmailSettings>(json, options);
            if (settings is null) return;

            SenderName    = settings.SenderName;
            Email         = settings.Address;
            AppPassword   = settings.AppPassword;
            Subject       = settings.Subject;
            TemplateBody  = settings.TemplateBody;
            DailyLimit    = settings.DailyLimit;
            SmtpHost      = settings.SmtpHost;
            SmtpPort      = settings.SmtpPort;
            ResumePath    = settings.ResumePath;
            EmailListPath = settings.EmailListPath;
            
            System.Diagnostics.Debug.WriteLine($"[AutoReach] Loaded TemplateBody:\n{settings.TemplateBody}");
        }
        catch (Exception ex)
        {
            // Log to debug output rather than silently swallowing
            System.Diagnostics.Debug.WriteLine($"[AutoReach] Failed to load settings: {ex.Message}");
        }
    }

    private void SaveUserSettings()
    {
        var settings = BuildSettings();
        var options  = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var json     = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(_settingsPath, json, Encoding.UTF8);
    }

    private EmailSettings BuildSettings() => new()
    {
        SenderName    = SenderName,
        Address       = Email,
        AppPassword   = AppPassword,
        Subject       = Subject,
        TemplateBody  = TemplateBody,
        DailyLimit    = DailyLimit,
        SmtpHost      = SmtpHost,
        SmtpPort      = SmtpPort,
        ResumePath    = ResumePath,
        EmailListPath = EmailListPath,
        SentListPath  = string.IsNullOrWhiteSpace(EmailListPath)
            ? string.Empty
            : Path.Combine(Path.GetDirectoryName(EmailListPath) ?? string.Empty, "sent_emails.txt")
    };

    // ── Batch Execution ──────────────────────────────────────────────────────
    private async Task StartBatchAsync()
    {
        if (string.IsNullOrWhiteSpace(ResumePath) || string.IsNullOrWhiteSpace(EmailListPath))
        {
            MessageBox.Show(
                "Please select both a resume (PDF) and an email list (TXT) before starting.",
                "Missing Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ActivityLogs.Clear();
        SaveUserSettings();

        _cts    = new CancellationTokenSource();
        IsBusy  = true;

        var progress = new Progress<string>(ReportProgress);
        var settings = BuildSettings();

        try
        {
            await _emailService.ProcessEmailsAsync(settings, progress, _cts.Token);
            MessageBox.Show("Batch processing finished!", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            LogActivity("System", "Stopped by user");
        }
        catch (Exception ex)
        {
            LogActivity("System", "Fatal Error");
            MessageBox.Show($"ERROR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StopBatch() => _cts?.Cancel();

    // ── Progress Handling ────────────────────────────────────────────────────
    private void ReportProgress(string msg)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var time = DateTime.Now.ToString("hh:mm tt");

            if (msg.StartsWith("[") && msg.Contains("Successfully sent to:"))
            {
                var email = msg[(msg.IndexOf("Successfully sent to:") + "Successfully sent to:".Length)..].Trim();
                LogActivity(email, "Delivered", time);
            }
            else if (msg.StartsWith("Failed to send to"))
            {
                // "Failed to send to foo@bar.com: reason"
                var afterTo = msg["Failed to send to".Length..].Trim();
                var colon   = afterTo.IndexOf(':');
                var email   = colon > 0 ? afterTo[..colon].Trim() : afterTo;
                LogActivity(email, "Error", time);
            }
            else
            {
                LogActivity("System", msg, time);
            }
        });
    }

    private void LogActivity(string recipient, string status, string? time = null)
    {
        ActivityLogs.Insert(0, new ActivityLog
        {
            Recipient = recipient,
            Status    = status,
            TimeSent  = time ?? DateTime.Now.ToString("hh:mm tt")
        });
    }

    // ── INotifyPropertyChanged ───────────────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
