namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a basic literal where no further details are necessary during the analysis.
  /// </summary>
  public class GenericLiteralExpression : Expression {
    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return "\\literal";
    }
  }
}
