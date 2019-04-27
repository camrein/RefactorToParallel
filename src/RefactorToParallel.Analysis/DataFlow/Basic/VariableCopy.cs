using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Core.Util;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Represents a copy instruction within the control flow graph.
  /// </summary>
  public class VariableCopy {
    private readonly int _hashCode;

    /// <summary>
    /// Gets the node where the copy assignment occurs.
    /// </summary>
    public FlowNode TargetNode { get; }

    /// <summary>
    /// Gets the target variable of the copy (left hand-side of assignment).
    /// </summary>
    public string TargetVariable { get; }

    /// <summary>
    /// Gets the source variable of the copy (right hand-side of assignment).
    /// </summary>
    public string SourceVariable { get; }

    /// <summary>
    /// Creates a new variable copy identifier with the given information.
    /// </summary>
    /// <param name="targetNode">The node where the copy assignment has been identified.</param>
    /// <param name="targetVariable">The target variable of the copy (left hand-side of assignment).</param>
    /// <param name="sourceVariable">The source variable of the copy (right hand-side of assignment).</param>
    public VariableCopy(FlowNode targetNode, string targetVariable, string sourceVariable) {
      TargetNode = targetNode;
      TargetVariable = targetVariable;
      SourceVariable = sourceVariable;
      _hashCode = Hash.With(targetNode).And(targetVariable).And(sourceVariable).Get();
    }

    public override bool Equals(object obj) {
      var other = obj as VariableCopy;
      return other != null && Equals(TargetNode, other.TargetNode)
        && Equals(TargetVariable, other.TargetVariable) && Equals(SourceVariable, other.SourceVariable);
    }

    public override int GetHashCode() {
      return _hashCode;
    }
  }
}
