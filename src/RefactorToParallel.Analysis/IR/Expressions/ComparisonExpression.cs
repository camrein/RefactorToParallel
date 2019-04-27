namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Comparison expressions which represents binary comparisons between two values.
  /// </summary>
  public class ComparisonExpression : BinaryExpression {
    // TODO more explicit? depends on how detailed the analysis is. Currently its probably not needed.

    /// <summary>
    /// Creates a new comparison expression.
    /// </summary>
    /// <param name="operator">The operator used within this comparision.</param>
    /// <param name="left">The left hand side of this comparison.</param>
    /// <param name="right">The right hand side of this comparison.</param>
    public ComparisonExpression(string @operator, Expression left, Expression right) : base(@operator, left, right) {}

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
