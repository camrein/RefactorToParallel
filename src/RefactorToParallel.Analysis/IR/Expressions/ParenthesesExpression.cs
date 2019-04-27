namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents the parentheses within an expression.
  /// </summary>
  public class ParenthesesExpression : Expression {
    /// <summary>
    /// Gets the expression enclosed by these parentheses.
    /// </summary>
    public Expression Expression { get; }

    /// <summary>
    /// Creates a new expression enclosed with parentheses.
    /// </summary>
    /// <param name="expression">The expression to enclose.</param>
    public ParenthesesExpression(Expression expression) {
      Expression = expression;
    }

    public override string ToString() {
      return $"({Expression})";
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
