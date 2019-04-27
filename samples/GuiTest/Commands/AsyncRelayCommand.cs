using System;
using System.Threading.Tasks;

namespace GuiTest.Commands {
  /// <summary>
  /// Asynchronous <see cref="ICommand" /> implementation for use with lambdas.
  /// </summary>
  public class AsyncRelayCommand : BaseRelayCommand {
    private readonly Func<object, Task> _executeMethod;

    /// <summary>
    /// Creates a new instance with which the command can always be executed.
    /// </summary>
    /// <param name="executeMethod"><see cref="Action{T}"/> to execute when invoked.</param>
    public AsyncRelayCommand(Func<object, Task> executeMethod) : this(executeMethod, null) { }

    /// <summary>
    /// Creates a new instance with variable execution state.
    /// </summary>
    /// <param name="executeMethod"><see cref="Action{T}"/> to execute when invoked.</param>
    /// <param name="canExecuteMethod"><see cref="Predicate{T}"/> identifying if the method can be invoked.</param>
    public AsyncRelayCommand(Func<object, Task> executeMethod, Predicate<object> canExecuteMethod) : base(canExecuteMethod) {
      _executeMethod = executeMethod;
    }

    public async override void Execute(object parameter) {
      await _executeMethod(parameter);
    }
  }
}