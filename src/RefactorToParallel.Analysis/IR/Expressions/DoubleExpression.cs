namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a double expression
  /// </summary>
  public class DoubleExpression : ConstantExpression {
    /// <summary>
    /// Gets the actual value represented by this double expression.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Creates a new double constant expression.
    /// </summary>
    /// <param name="value">The double constant represented by this expression.</param>
    public DoubleExpression(double value) : base(value.ToString()) {
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
