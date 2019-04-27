using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Visitor for instructions of an intermediate code.
  /// </summary>
  public interface IInstructionVisitor {
    /// <summary>
    /// Visits the given instruction.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(Instruction instruction);

    /// <summary>
    /// Visits the given assignment.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(Assignment instruction);

    /// <summary>
    /// Visits the given declaration.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(Declaration instruction);

    /// <summary>
    /// Visits the given label.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(Label instruction);

    /// <summary>
    /// Visits the given jump.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(Jump instruction);

    /// <summary>
    /// Visits the given conditional jump.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(ConditionalJump instruction);

    /// <summary>
    /// Visits the given invocation.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    void Visit(Invocation instruction);
  }

  /// <summary>
  /// Visitor for basic nodes of an intermediate code which allows return information with each visited node.
  /// </summary>
  /// <typeparam name="TResult">The type of information returned after visiting a node.</typeparam>
  public interface IInstructionVisitor<out TResult> {
    /// <summary>
    /// Visits the given instruction.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(Instruction instruction);

    /// <summary>
    /// Visits the given assignment.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(Assignment instruction);

    /// <summary>
    /// Visits the given declaration.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(Declaration instruction);

    /// <summary>
    /// Visits the given label.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(Label instruction);

    /// <summary>
    /// Visits the given jump.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(Jump instruction);

    /// <summary>
    /// Visits the given conditional jump.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(ConditionalJump instruction);

    /// <summary>
    /// Visits the given invocation.
    /// </summary>
    /// <param name="instruction">The instruction to visit.</param>
    /// <returns>The data after visiting the given instruction.</returns>
    TResult Visit(Invocation instruction);
  }
}
