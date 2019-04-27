namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents the basic expression type.
  /// </summary>
  public abstract class Expression {
    /// <summary>
    /// Invokes the given visitor with the respective type.
    /// </summary>
    /// <param name="visitor">The visitor to invoke.</param>
    public abstract void Accept(IExpressionVisitor visitor);

    /// <summary>
    /// Invokes the given visitor with the respective type.
    /// </summary>
    /// <typeparam name="TResult">The type of the resulting value after visiting the expressions.</typeparam>
    /// <param name="visitor">The visitor to invoke.</param>
    /// <returns>The result after visiting the expression tree.</returns>
    public abstract TResult Accept<TResult>(IExpressionVisitor<TResult> visitor);
  }
}
