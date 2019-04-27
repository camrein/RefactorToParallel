using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Represents a flow boundary within the control flow graph.
  /// </summary>
  public class FlowBoundary : FlowNode {
    public readonly string _label;

    /// <summary>
    /// Gets the name of the method this boundary belongs to.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Creates a new flow boundary instance.
    /// </summary>
    /// <param name="methodName">The containing method.</param>
    /// <param name="kind">The kind of the transferred flow control.</param>
    public FlowBoundary(string methodName, FlowKind kind) : base(new Label(), kind) {
      MethodName = methodName ?? "<root>";
      var prefix = methodName == null ? "" : $"{methodName}:";
      _label = kind == FlowKind.Start ? $"<{prefix}Start>" : $"<{prefix}End>";
    }

    public override string ToString() {
      return _label;
    }
  }
}
