using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// Represents a jump within the code which is only executed if the condition is met.
  /// </summary>
  public class ConditionalJump : Instruction {
    /// <summary>
    /// Gets the target label of the jump.
    /// </summary>
    public Label Target { get; }

    /// <summary>
    /// The condition that has to be met to apply the jump.
    /// </summary>
    public Expression Condition { get; }

    /// <summary>
    /// Creates a new jump.
    /// </summary>
    /// <param name="condition">The condition of the jump.</param>
    /// <param name="target">The target label of the jump.</param>
    public ConditionalJump(Expression condition, Label target) {
      Target = target;
      Condition = condition;
    }

    public override void Accept(IInstructionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IInstructionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return $"ConditionalJump: {Condition}";
    }
  }
}
