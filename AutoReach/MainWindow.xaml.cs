using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using AutoReach.Models;
using AutoReach.Services;

namespace AutoReach
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _cts;
        private readonly EmailService _emailService = new();
        private readonly string _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user_settings.json");

        // Collection to bind to the Activity DataGrid
        public ObservableCollection<ActivityLog> ActivityLogs { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            // Bind the DataGrid to our collection
            ActivityGrid.ItemsSource = ActivityLogs;

            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<EmailSettings>(json);
                    if (settings != null)
                    {
                        TxtEmail.Text = settings.Address;
                        TxtPassword.Password = settings.AppPassword;
                        TxtSubject.Text = settings.Subject;
                        TxtBody.Text = settings.TemplateBody;

                        // If path is saved, display it; otherwise show the default drag-and-drop text
                        TxtResumePath.Text = string.IsNullOrWhiteSpace(settings.ResumePath) ? "Drag-and-drop your resume" : settings.ResumePath;
                        TxtListPath.Text = string.IsNullOrWhiteSpace(settings.EmailListPath) ? "Drag-and-drop email list" : settings.EmailListPath;

                        if (!string.IsNullOrWhiteSpace(settings.ResumePath)) TxtResumePath.Foreground = System.Windows.Media.Brushes.Black;
                        if (!string.IsNullOrWhiteSpace(settings.EmailListPath)) TxtListPath.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }
                catch { /* Ignore deserialization errors on load */ }
            }
        }

        private void SaveUserSettings(EmailSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json);
        }

        // ================= DRAG AND DROP HANDLERS =================

        private void Resume_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    TxtResumePath.Text = files[0];
                    TxtResumePath.Foreground = System.Windows.Media.Brushes.Black; // Change color to indicate file loaded
                }
                else
                {
                    MessageBox.Show("Please drop a valid PDF file for the resume.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void EmailList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    TxtListPath.Text = files[0];
                    TxtListPath.Foreground = System.Windows.Media.Brushes.Black; // Change color to indicate file loaded
                }
                else
                {
                    MessageBox.Show("Please drop a valid TXT file for the email list.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // ================= EXECUTION LOGIC =================

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Reset logs
            ActivityLogs.Clear();

            // 1. Create settings from UI input
            var settings = new EmailSettings
            {
                Address = TxtEmail.Text,
                AppPassword = TxtPassword.Password,
                Subject = TxtSubject.Text,
                TemplateBody = TxtBody.Text,
                // Ensure we don't save the placeholder instructional text as a path
                ResumePath = TxtResumePath.Text.Contains("Drag-and-drop") ? "" : TxtResumePath.Text,
                EmailListPath = TxtListPath.Text.Contains("Drag-and-drop") ? "" : TxtListPath.Text,
                DailyLimit = 50
            };

            // Set SentListPath relative to EmailListPath if it exists
            if (!string.IsNullOrWhiteSpace(settings.EmailListPath))
            {
                settings.SentListPath = Path.Combine(Path.GetDirectoryName(settings.EmailListPath) ?? "", "sent_emails.txt");
            }

            // Basic validation
            if (string.IsNullOrWhiteSpace(settings.ResumePath) || string.IsNullOrWhiteSpace(settings.EmailListPath))
            {
                MessageBox.Show("Please drag and drop both a resume (PDF) and an email list (TXT) before starting.", "Missing Files", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Persist for next time
            SaveUserSettings(settings);

            // 3. Start processing
            _cts = new CancellationTokenSource();
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;

            // Handle progress updates by parsing the strings from EmailService to fit our new DataGrid
            var progress = new Progress<string>(msg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    string timeStr = DateTime.Now.ToString("hh:mm tt");

                    if (msg.Contains("Successfully sent to:"))
                    {
                        var email = msg.Split(':').Last().Trim();
                        // Insert at index 0 so newest items appear at the top of the list
                        ActivityLogs.Insert(0, new ActivityLog { Recipient = email, Status = "Delivered", TimeSent = timeStr });
                    }
                    else if (msg.Contains("Failed to send to"))
                    {
                        // Parse: "Failed to send to example@domain.com: Error message"
                        var parts = msg.Split(new[] { "to ", ":" }, StringSplitOptions.RemoveEmptyEntries);
                        var email = parts.Length > 1 ? parts[1].Trim() : "Unknown";
                        ActivityLogs.Insert(0, new ActivityLog { Recipient = email, Status = "Error", TimeSent = timeStr });
                    }
                    else
                    {
                        // General system messages (like "Connecting to SMTP...")
                        ActivityLogs.Insert(0, new ActivityLog { Recipient = "System", Status = msg, TimeSent = timeStr });
                    }
                });
            });

            try
            {
                await _emailService.ProcessEmailsAsync(settings, progress, _cts.Token);
                MessageBox.Show("Batch processing finished!", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                ActivityLogs.Insert(0, new ActivityLog { Recipient = "System", Status = "Stopped by user", TimeSent = DateTime.Now.ToString("hh:mm tt") });
            }
            catch (Exception ex)
            {
                ActivityLogs.Insert(0, new ActivityLog { Recipient = "System", Status = "Fatal Error", TimeSent = DateTime.Now.ToString("hh:mm tt") });
                MessageBox.Show($"ERROR: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            BtnStop.IsEnabled = false;
        }
    }

    // Class representing a single row in the Recent Activity Grid
    public class ActivityLog
    {
        public string Recipient { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TimeSent { get; set; } = string.Empty;
    }
}