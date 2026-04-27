using System.Windows;
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
