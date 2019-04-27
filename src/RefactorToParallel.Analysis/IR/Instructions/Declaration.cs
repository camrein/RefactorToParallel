namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// Represents an assignment within the code.
  /// </summary>
  public class Declaration : Instruction {
    /// <summary>
    /// Gets the name of the declared variable. Is never <code>null</code>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new variable declaration.
    /// </summary>
    /// <param name="name">The name of the variable declared.</param>
    public Declaration(string name) {
      Name = name;
    }

    public override void Accept(IInstructionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IInstructionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return $"Declaration: {Name}";
    }
  }
}
