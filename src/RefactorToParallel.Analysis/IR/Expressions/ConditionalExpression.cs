namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a conditional expression.
  /// </summary>
  public class ConditionalExpression : Expression {
    /// <summary>
    /// Gets the condition.
    /// </summary>
    public Expression Condition { get; }

    /// <summary>
    /// Gets the case that results when the condition evaluates to <code>true</code>.
    /// </summary>
    public Expression WhenTrue { get; }

    /// <summary>
    /// Gets the case that results when the condition evaluates to <code>false</code>.
    /// </summary>
    public Expression WhenFalse { get; }

    /// <summary>
    /// Creates a new binary expression
    /// </summary>
    /// <param name="condition">The condition of this conditional expression..</param>
    /// <param name="whenTrue">The resulting case if the condition evaluates to <code>true</code>.</param>
    /// <param name="whenFalse">The resulting case if the condition evaluates to <code>false</code>.</param>
    public ConditionalExpression(Expression condition, Expression whenTrue, Expression whenFalse) {
      Condition = condition;
      WhenTrue = whenTrue;
      WhenFalse = whenFalse;
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Condition} ? {WhenTrue} : {WhenFalse}";
    }
  }
}
