namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents an integer expression
  /// </summary>
  public class IntegerExpression : ConstantExpression {
    /// <summary>
    /// Gets the actual value represented by this integer expression.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Creates a new integer constant expression.
    /// </summary>
    /// <param name="value">The integer constant represented by this expression.</param>
    public IntegerExpression(long value) : base(value.ToString()) {
      Value = value;
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
