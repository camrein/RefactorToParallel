namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a multiply expression.
  /// </summary>
  public class MultiplyExpression : BinaryExpression {
    /// <summary>
    /// Creates a new multiply expression.
    /// </summary>
    /// <param name="left">The left hand side of the binary expression.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    public MultiplyExpression(Expression left, Expression right) : base("*", left, right) { }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
