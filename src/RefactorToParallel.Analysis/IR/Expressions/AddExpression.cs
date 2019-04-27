namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents an add expression.
  /// </summary>
  public class AddExpression : BinaryExpression {
    /// <summary>
    /// Creates a new add expression.
    /// </summary>
    /// <param name="left">The left hand side of the binary expression.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    public AddExpression(Expression left, Expression right) : base("+", left, right) { }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
