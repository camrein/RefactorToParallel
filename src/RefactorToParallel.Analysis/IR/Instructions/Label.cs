namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// Represents a label within the code.
  /// </summary>
  public class Label : Instruction {
    private readonly string _name;

    /// <summary>
    /// Creates a new label with an empty name.
    /// </summary>
    public Label() : this(null) { }

    /// <summary>
    /// Creates a new label with the given name.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    public Label(string name) {
      _name = name;
    }

    public override void Accept(IInstructionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IInstructionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return _name ?? "Label";
    }
  }
}
