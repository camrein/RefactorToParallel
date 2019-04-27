namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents any arbitrary binary expression that has not its individual implementation.
  /// </summary>
  public class GenericBinaryExpression : BinaryExpression {
    /// <summary>
    /// Creates a new generic binary expression
    /// </summary>
    /// <param name="operator">The oeprator that this binary expression represents.</param>
    /// <param name="left">The left hand side of the binary expression.</param>
    /// <param name="right">The right hand side of the binary expression.</param>
    public GenericBinaryExpression(string @operator, Expression left, Expression right) : base(@operator, left, right) { }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
