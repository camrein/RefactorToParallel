using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorToParallel.Refactoring.Generators {
  /// <summary>
  /// Method collection for generating name syntaxes.
  /// </summary>
  public static class NameGenerator {
    /// <summary>
    /// Creates a qualified name out of the given components.
    /// </summary>
    /// <param name="head">The first component of the name.</param>
    /// <param name="second">The first component accessed by the qualified name.</param>
    /// <param name="tail">The succeeding components of the name.</param>
    /// <returns>A qualified identifier with the given components.</returns>
    public static QualifiedNameSyntax CreateName(string head, string second, params string[] tail) {
      var name = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(head), SyntaxFactory.IdentifierName(second));
      return tail.Aggregate(name, (memo, component) => SyntaxFactory.QualifiedName(memo, SyntaxFactory.IdentifierName(component)));
    }
  }
}
