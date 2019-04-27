namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// represents a node within the AST of the IR.
  /// </summary>
  public abstract class Instruction {
    /// <summary>
    /// Invokes the given visitor with the respective type.
    /// </summary>
    /// <param name="visitor">The visitor to invoke.</param>
    public abstract void Accept(IInstructionVisitor visitor);

    /// <summary>
    /// Invokes the given visitor with the respective type.
    /// </summary>
    /// <typeparam name="TResult">The type of the resulting value after visiting the expressions.</typeparam>
    /// <param name="visitor">The visitor to invoke.</param>
    /// <returns>The result after visiting the expression tree.</returns>
    public abstract TResult Accept<TResult>(IInstructionVisitor<TResult> visitor);
  }
}
