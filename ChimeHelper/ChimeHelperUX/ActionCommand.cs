using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ChimeHelperUX
{

  /// <summary>
  /// Basic Command class based on:
  /// https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Xaml.Input.ICommand?view=winrt-19041
  /// </summary>
  class ActionCommand : ICommand
  {
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    /// <summary>
    /// Raised when RaiseCanExecuteChanged is called.
    /// </summary>
    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public ActionCommand(Action<object> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public ActionCommand(Action<object> execute, Func<object, bool> canExecute)
    {
      _execute = execute ?? throw new ArgumentNullException("execute");
      _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether this RelayCommand can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed,
    /// this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
      return _canExecute == null || _canExecute(parameter);
    }

    /// <summary>
    /// Executes the RelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed,
    /// this object can be set to null.
    /// </param>
    public void Execute(object parameter)
    {
      _execute(parameter);
    }

    /// <summary>
    /// Method used to raise the CanExecuteChanged event
    /// to indicate that the return value of the CanExecute
    /// method has changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

  }
}
