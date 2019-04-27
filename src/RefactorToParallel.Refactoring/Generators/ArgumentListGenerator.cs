using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Refactoring.Generators {
  /// <summary>
  /// Method collection for argument lists expressions.
  /// </summary>
  public static class ArgumentListGenerator {
    /// <summary>
    /// Generates an argument list out of the given expressions.
    /// </summary>
    /// <param name="arguments">The expressions used as arguments.</param>
    /// <returns>The generated argument list.</returns>
    public static ArgumentListSyntax CreateArgumentList(params ExpressionSyntax[] arguments) {
      return CreateArgumentList(arguments.AsEnumerable());
    }

    /// <summary>
    /// Generates an argument list out of the given expressions.
    /// </summary>
    /// <param name="arguments">The expressions used as arguments.</param>
    /// <returns>The generated argument list.</returns>
    public static ArgumentListSyntax CreateArgumentList(IEnumerable<ExpressionSyntax> arguments) {
      return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments.Select(SyntaxFactory.Argument)));
    }
  }
}
