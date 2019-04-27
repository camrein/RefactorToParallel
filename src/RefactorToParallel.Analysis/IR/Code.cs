using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Represents code that has been transformed into the intermediate representation.
  /// </summary>
  public class Code {
    /// <summary>
    /// Gets the root node of the code.
    /// </summary>
    public IReadOnlyList<Instruction> Root { get; }

    /// <summary>
    /// Creates a new instance with the given root.
    /// </summary>
    /// <param name="root">The root node of the code.</param>
    public Code(IReadOnlyList<Instruction> root) {
      Root = root;
    }
  }
}
