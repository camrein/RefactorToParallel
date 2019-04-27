using System;
using System.Windows.Input;

namespace GuiTest.Commands {

  /// <summary>
  /// <see cref="ICommand" /> implementation for use with lambdas.
  /// </summary>
  public abstract class BaseRelayCommand : ICommand {
    private readonly Predicate<object> _canExecuteMethod;

    /// <summary>
    /// Creates a new instance with which the command can always be executed.
    /// </summary>
    public BaseRelayCommand() : this(null) { }

    /// <summary>
    /// Creates a new instance with variable execution state.
    /// </summary>
    /// <param name="canExecuteMethod"><see cref="Predicate{T}"/> identifying if the method can be invoked.</param>
    public BaseRelayCommand(Predicate<object> canExecuteMethod) {
      _canExecuteMethod = canExecuteMethod;
    }

    public bool CanExecute(object parameter) {
      return _canExecuteMethod?.Invoke(parameter) != false;
    }

    public abstract void Execute(object parameter);

    public event EventHandler CanExecuteChanged {
      add {
        if(_canExecuteMethod != null) {
          CommandManager.RequerySuggested += value;
        }
      }

      remove {
        if(_canExecuteMethod != null) {
          CommandManager.RequerySuggested -= value;
        }
      }
    }
  }
}
