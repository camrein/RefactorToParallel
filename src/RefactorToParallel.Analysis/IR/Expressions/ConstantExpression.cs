namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents a constant expression.
  /// </summary>
  public abstract class ConstantExpression : Expression {
    /// <summary>
    /// Gets the label of the constant represented by this expression.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Creates a new constant expression.
    /// </summary>
    /// <param name="label">The label of the constant represented by this expression.</param>
    protected ConstantExpression(string label) {
      Label = label;
    }

    public override string ToString() {
      return Label;
    }
  }
}
