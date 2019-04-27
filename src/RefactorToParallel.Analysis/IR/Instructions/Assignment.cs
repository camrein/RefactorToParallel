using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.IR.Instructions {
  /// <summary>
  /// Represents an assignment within the code.
  /// </summary>
  public class Assignment : Instruction {
    /// <summary>
    /// Gets the expression on the left hand side of the assignment.
    /// </summary>
    public Expression Left { get; }

    /// <summary>
    /// Gets the expression on the right hand side of the assignment.
    /// </summary>
    public Expression Right { get; }

    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    /// <param name="left">The left hand side of the assignment.</param>
    /// <param name="right">The right hand side of the assignment.</param>
    public Assignment(Expression left, Expression right) {
      Left = left;
      Right = right;
    }

    public override void Accept(IInstructionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IInstructionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }

    public override string ToString() {
      return $"Assignment: {Left} = {Right}";
    }
  }
}
