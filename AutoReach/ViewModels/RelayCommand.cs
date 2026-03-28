using System.Windows.Input;

namespace AutoReach.ViewModels;

/// <summary>
/// A lightweight <see cref="ICommand"/> implementation that delegates
/// execution to caller-supplied delegates.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsync;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute   = canExecute;
    }

    // Convenience constructor for synchronous actions
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        : this(p => { execute(p); return Task.CompletedTask; }, canExecute) { }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public async void Execute(object? parameter)
    {
        try { await _executeAsync(parameter); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[RelayCommand] {ex}"); }
    }

    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
