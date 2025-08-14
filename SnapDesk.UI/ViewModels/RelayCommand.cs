using System;
using System.Windows.Input;

namespace SnapDesk.UI.ViewModels;

/// <summary>
/// A command that delegates its execution to other objects by invoking delegates.
/// This is a common MVVM pattern that allows ViewModels to define commands
/// that the UI can bind to buttons and other controls.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// Event that is raised when the ability to execute the command has changed
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Constructor for a command that can always execute
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked</param>
    public RelayCommand(Action execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = null;
    }

    /// <summary>
    /// Constructor for a command that can conditionally execute
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked</param>
    /// <param name="canExecute">Function that determines if the command can execute</param>
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
    }

    /// <summary>
    /// Determines whether the command can execute in its current state
    /// </summary>
    /// <param name="parameter">Data used by the command (not used in this implementation)</param>
    /// <returns>True if the command can execute, false otherwise</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <summary>
    /// Executes the command
    /// </summary>
    /// <param name="parameter">Data used by the command (not used in this implementation)</param>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            _execute();
        }
    }

    /// <summary>
    /// Raises the CanExecuteChanged event to notify the UI that the command's
    /// ability to execute has changed
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// A command that delegates its execution to other objects by invoking delegates.
/// This version supports commands that take a parameter.
/// </summary>
/// <typeparam name="T">The type of the parameter that the command accepts</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    /// <summary>
    /// Event that is raised when the ability to execute the command has changed
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Constructor for a command that can always execute
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked</param>
    public RelayCommand(Action<T?> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = null;
    }

    /// <summary>
    /// Constructor for a command that can conditionally execute
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked</param>
    /// <param name="canExecute">Function that determines if the command can execute</param>
    public RelayCommand(Action<T?> execute, Func<T?, bool> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
    }

    /// <summary>
    /// Determines whether the command can execute in its current state
    /// </summary>
    /// <param name="parameter">Data used by the command</param>
    /// <returns>True if the command can execute, false otherwise</returns>
    public bool CanExecute(object? parameter)
    {
        if (parameter is T typedParameter)
        {
            return _canExecute?.Invoke(typedParameter) ?? true;
        }
        return _canExecute?.Invoke(default) ?? true;
    }

    /// <summary>
    /// Executes the command
    /// </summary>
    /// <param name="parameter">Data used by the command</param>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            if (parameter is T typedParameter)
            {
                _execute(typedParameter);
            }
            else
            {
                _execute(default);
            }
        }
    }

    /// <summary>
    /// Raises the CanExecuteChanged event to notify the UI that the command's
    /// ability to execute has changed
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
