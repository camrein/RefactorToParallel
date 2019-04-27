using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Represents an invocation within the control flow graph.
  /// </summary>
  public class FlowInvocation : FlowNode {
    /// <summary>
    /// Gets the invocation expresssion represented by this node.
    /// </summary>
    public InvocationExpression Expression { get; }

    /// <summary>
    /// Creats a new instance with the given invocation expression.
    /// </summary>
    /// <param name="expression">The invocation expression represented by this node.</param>
    public FlowInvocation(InvocationExpression expression) : base(new Label()) {
      Expression = expression;
    }

    public override string ToString() {
      return $"CALL {Expression}";
    }
  }
}
