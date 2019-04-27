using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// Represents an invocation within the code.
  /// </summary>
  public class Invocation : Instruction {
    // TODO a more generic type for expressions that can be an instruction for themselfs? currently not needed.

    /// <summary>
    /// Gets the invocation expression of this invocation.
    /// </summary>
    public InvocationExpression Expression { get; }

    /// <summary>
    /// Creates a new invocation.
    /// </summary>
    /// <param name="expression">The invocation expression encapsulated by this instruction.</param>
    public Invocation(InvocationExpression expression) {
      Expression = expression;
    }

    public override void Accept(IInstructionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IInstructionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return $"Invocation: {Expression}";
    }
  }
}
