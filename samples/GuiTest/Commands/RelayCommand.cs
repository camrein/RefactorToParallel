using System;

namespace GuiTest.Commands {
  /// <summary>
  /// <see cref="ICommand" /> implementation for use with lambdas.
  /// </summary>
  public class RelayCommand : BaseRelayCommand {
    private readonly Action<object> _executeMethod;

    /// <summary>
    /// Creates a new instance with which the command can always be executed.
    /// </summary>
    /// <param name="executeMethod"><see cref="Action{T}"/> to execute when invoked.</param>
    public RelayCommand(Action<object> executeMethod) : this(executeMethod, null) { }

    /// <summary>
    /// Creates a new instance with variable execution state.
    /// </summary>
    /// <param name="executeMethod"><see cref="Action{T}"/> to execute when invoked.</param>
    /// <param name="canExecuteMethod"><see cref="Predicate{T}"/> identifying if the method can be invoked.</param>
    public RelayCommand(Action<object> executeMethod, Predicate<object> canExecuteMethod) : base(canExecuteMethod) {
      _executeMethod = executeMethod;
    }

    public override void Execute(object parameter) {
      _executeMethod(parameter);
    }
  }
}