using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Core.Util;

namespace RefactorToParallel.Analysis.Collectors {
  /// <summary>
  /// Identifies an array access at a specific position within the control flow graph.
  /// </summary>
  public class ArrayAccess {
    private readonly int _hashCode;

    /// <summary>
    /// Gets the node of the control flow graph this access was identified.
    /// </summary>
    public FlowNode Node { get; }

    /// <summary>
    /// Gets the expression accessing the array.
    /// </summary>
    public ArrayExpression Expression { get; }

    /// <summary>
    /// Gets if the access was writing or reading.
    /// </summary>
    public bool Write { get; }

    /// <summary>
    /// Creates a new array access instance.
    /// </summary>
    /// <param name="node">The node of the control flow graph where the access was identified.</param>
    /// <param name="expression">The expression accessing the array.</param>
    /// <param name="write"><code>True</code> if the array access is writing.</param>
    public ArrayAccess(FlowNode node, ArrayExpression expression, bool write) {
      Node = node;
      Expression = expression;
      Write = write;
      _hashCode = Hash.With(node).And(expression).And(write).Get();
    }

    public override bool Equals(object obj) {
      var other = obj as ArrayAccess;
      return other != null && Write.Equals(other.Write) && Node.Equals(other.Node) && Expression.Equals(other.Expression);
    }

    public override int GetHashCode() {
      return _hashCode;
    }
  }
}
