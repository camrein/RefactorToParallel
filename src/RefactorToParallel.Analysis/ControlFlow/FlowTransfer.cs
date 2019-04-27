using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Represents a transfer in control by an invocation.
  /// </summary>
  public class FlowTransfer : FlowNode {
    private readonly bool _isInvocation;

    /// <summary>
    /// Gets if the control is transferred to the target method by an invocation.
    /// </summary>
    public bool TransfersTo => _isInvocation;

    /// <summary>
    /// Gets if the control is transferred by returning to the initial invoker.
    /// </summary>
    public bool TransfersBack => !_isInvocation;

    /// <summary>
    /// Gets the name of the method that is transferring the control.
    /// </summary>
    public string SourceMethod { get; }

    /// <summary>
    /// Gets the name of the method that receives the control.
    /// </summary>
    public string TargetMethod { get; }

    /// <summary>
    /// Creates a new node that denotes a transfer in the control.
    /// </summary>
    /// <param name="isInvocation"><code>True</code> if the transfer is resulting from an invocation, otherwise <code>false</code>.</param>
    /// <param name="sourceMethod">The method that is transferring the control.</param>
    /// <param name="targetMethod">The method that is receiving the control.</param>
    public FlowTransfer(bool isInvocation, string sourceMethod, string targetMethod) : base(new Label()) {
      _isInvocation = isInvocation;
      SourceMethod = sourceMethod;
      TargetMethod = targetMethod;
    }

    public override string ToString() {
      var type = _isInvocation ? "FROM" : "BACK";
      return $"TRANSFER {type} {SourceMethod} TO {TargetMethod}";
    }
  }
}
