namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// Represents a jump within the code.
  /// </summary>
  public class Jump : Instruction {
    /// <summary>
    /// Gets the target label of the jump.
    /// </summary>
    public Label Target { get; }

    /// <summary>
    /// Creates a new jump.
    /// </summary>
    /// <param name="target">The target label of the jump.</param>
    public Jump(Label target) {
      Target = target;
    }

    public override void Accept(IInstructionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IInstructionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return $"Jump: {Target}";
    }
  }
}
