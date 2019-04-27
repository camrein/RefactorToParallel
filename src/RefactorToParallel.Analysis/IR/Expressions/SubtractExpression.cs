namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents an subtract expression.
  /// </summary>
  public class SubtractExpression : BinaryExpression {
    /// <summary>
    /// Creates a new subtract expression.
    /// </summary>
    /// <param name="left">The left hand side of the binary expression.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    public SubtractExpression(Expression left, Expression right) : base("-", left, right) { }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
