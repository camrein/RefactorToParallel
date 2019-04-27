using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RefactorToParallel.Evaluation.ByParallelFor {
  public static class Extensions {
    /// <summary>
    /// Checks if the given invocation expression is the invocation of the Parallel.For method.
    /// </summary>
    /// <param name="invocation">The invocation to check.</param>
    /// <param name="semanticModel">The semantic model backing the invocation.</param>
    /// <returns><code>true</code> if the invocation invokes Parallel.For.</returns>
    public static bool IsParallelFor(this InvocationExpressionSyntax invocation, SemanticModel semanticModel) {
      var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
      return symbol?.ToString().StartsWith("System.Threading.Tasks.Parallel.For(", StringComparison.Ordinal) == true;
    }
  }
}
