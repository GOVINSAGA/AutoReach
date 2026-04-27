using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;
using AutoReach.ViewModels;

namespace AutoReach;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// Code-behind is intentionally minimal — all logic lives in <see cref="MainViewModel"/>.
/// Only strictly view-specific concerns are handled here:
///   - Drag-and-drop (requires UIElement events)
///   - PasswordBox sync (WPF PasswordBox cannot data-bind for security reasons)
///   - File browse dialogs
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        // Populate PasswordBox after DataContext is set
        TxtPassword.Password = ViewModel.AppPassword;
        Loaded += MainWindow_Loaded;
    }

    // ── RichTextBox sync ─────────────────────────────────────────────────────

    private bool _isUpdatingRichText = false;

    private void TxtBody_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_isUpdatingRichText) return;

        // Instead of plain text, we save the HTML representation to the ViewModel
        // But we ALSO need to be able to load it back if they restart the app.
        // For simplicity, we just save Xaml to TemplateBody and convert to HTML in EmailService.
        // Wait, saving Xaml is easiest.
        using (var ms = new System.IO.MemoryStream())
        {
            var textRange = new TextRange(TxtBody.Document.ContentStart, TxtBody.Document.ContentEnd);
            textRange.Save(ms, DataFormats.Xaml);
            ViewModel.TemplateBody = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize RichTextBox content from saved XAML settings
        _isUpdatingRichText = true;
        if (!string.IsNullOrWhiteSpace(ViewModel.TemplateBody))
        {
            try
            {
                // Try XAML first
                if (ViewModel.TemplateBody.Contains("<Section"))
                {
                    using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(ViewModel.TemplateBody)))
                    {
                        var textRange = new TextRange(TxtBody.Document.ContentStart, TxtBody.Document.ContentEnd);
                        textRange.Load(ms, DataFormats.Xaml);
                    }
                }
                else
                {
                    // Fallback to plain text if they had old settings
                    var textRange = new TextRange(TxtBody.Document.ContentStart, TxtBody.Document.ContentEnd);
                    textRange.Text = ViewModel.TemplateBody;
                }
            }
            catch
            {
                // Fallback
                var textRange = new TextRange(TxtBody.Document.ContentStart, TxtBody.Document.ContentEnd);
                textRange.Text = ViewModel.TemplateBody;
            }
        }
        _isUpdatingRichText = false;
    }

    // ── PasswordBox sync ─────────────────────────────────────────────────────
    // PasswordBox intentionally does not support binding; sync manually.

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        => ViewModel.AppPassword = TxtPassword.Password;

    // ── File Browse Dialogs ──────────────────────────────────────────────────

    private void BrowseResume_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "PDF Files (*.pdf)|*.pdf", Title = "Select your Resume" };
        if (dialog.ShowDialog() == true)
            ViewModel.ResumePath = dialog.FileName;
    }

    private void BrowseList_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt", Title = "Select your Email List" };
        if (dialog.ShowDialog() == true)
            ViewModel.EmailListPath = dialog.FileName;
    }

    // ── Drag-and-Drop Handlers ───────────────────────────────────────────────

    private void Resume_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 0 && files[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            ViewModel.ResumePath = files[0];
        else
            MessageBox.Show("Please drop a valid PDF file for the resume.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void EmailList_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 0 && files[0].EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            ViewModel.EmailListPath = files[0];
        else
            MessageBox.Show("Please drop a valid TXT file for the email list.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ── Coming Soon ──────────────────────────────────────────────────────────

    private void ComingSoon_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "We are excited too! These features will be available in the next release. Stay tuned!",
            "Coming Soon 🚀", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
