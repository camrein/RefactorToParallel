using RefactorToParallel.Analysis.Collectors;
using System.Linq;

namespace RefactorToParallel.Analysis.Verifier {
  /// <summary>
  /// Verifies that there are no write accesses to shared variables within the given variable accesses.
  /// </summary>
  public class WriteAccessVerifier {
    private readonly VariableAccesses _variableAccesses;

    private WriteAccessVerifier(VariableAccesses variableAccesses) {
      _variableAccesses = variableAccesses;
    }

    /// <summary>
    /// Checks that there are no write accesses to shared variables.
    /// </summary>
    /// <param name="variableAccesses">A prelimary applied variable access collection on the loop's body.</param>
    /// <returns><code>True</code> if there are no write accesses to shared variables.</returns>
    public static bool HasNoWriteAccessToSharedVariables(VariableAccesses variableAccesses) {
      return !new WriteAccessVerifier(variableAccesses)._ContainsWriteAccessesToSharedVariables();
    }

    private bool _ContainsWriteAccessesToSharedVariables() {
      return _variableAccesses.WrittenVariables.Any(_IsSharedVariable);
    }

    private bool _IsSharedVariable(string name) {
      return !_variableAccesses.DeclaredVariables.Contains(name);
    }
  }
}
