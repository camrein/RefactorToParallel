namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a variable expression
  /// </summary>
  public class VariableExpression : Expression {
    /// <summary>
    /// Gets the name of the variable represented by this expression.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new variable expression.
    /// </summary>
    /// <param name="name">The name of the variable represented by this expression.</param>
    public VariableExpression(string name) {
      Name = name;
    }

    public override string ToString() {
      return Name;
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
