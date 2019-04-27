namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a binary expression.
  /// </summary>
  public abstract class BinaryExpression : Expression {
    /// <summary>
    /// Gets the left hand side.
    /// </summary>
    public Expression Left { get; }

    /// <summary>
    /// Gets the right hand side.
    /// </summary>
    public Expression Right { get; }

    /// <summary>
    /// Gets the operator.
    /// </summary>
    public string Operator { get; }

    /// <summary>
    /// Creates a new binary expression
    /// </summary>
    /// <param name="operator">The oeprator that this binary expression represents.</param>
    /// <param name="left">The left hand side of the binary expression.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    protected BinaryExpression(string @operator, Expression left, Expression right) {
      Operator = @operator;
      Left = left;
      Right = right;
    }

    public override string ToString() {
      return $"{Left} {Operator} {Right}";
    }
  }
}
