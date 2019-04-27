namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents an unary minus expression.
  /// </summary>
  public class UnaryMinusExpression : Expression {
    /// <summary>
    /// Gets the expression encapsulated by the unary minus.
    /// </summary>
    public Expression Expression { get; }

    /// <summary>
    /// Creates a new unary minus expression.
    /// </summary>
    /// <param name="expression">The expression to encapsulate.</param>
    public UnaryMinusExpression(Expression expression) {
      Expression = expression;
    }

    public override string ToString() {
      return $"-{Expression}";
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
