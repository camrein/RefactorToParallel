using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorToParallel.Refactoring.Generators {
  /// <summary>
  /// Method collection for generating usings.
  /// </summary>
  public static class UsingGenerator {
    /// <summary>
    /// Adds a using statement to the compilation if necessary.
    /// </summary>
    /// <param name="compilation">The compilation unit to add the using to.</param>
    /// <param name="name">The name of the using to add.</param>
    /// <returns>The updated compilation unit with the new using.</returns>
    public static CompilationUnitSyntax AddUsingIfMissing(CompilationUnitSyntax compilation, NameSyntax name) {
      // TODO improve string comparison (replace)
      var stringName = name.ToString();
      if(compilation.DescendantNodes().OfType<UsingDirectiveSyntax>().Any(u => Equals(u.Name.ToString(), stringName))) {
        return compilation;
      }

      // TODO ensure order?
      if(!compilation.ContainsDirectives || compilation.Usings.Count > 0) {
        return compilation.AddUsings(SyntaxFactory.UsingDirective(name));
      }

      // Roslyn bug: AddUsings adds the using before preprocessor settings (aka #define), which is incorrect (compiler error).
      return compilation.WithoutLeadingTrivia().AddUsings(SyntaxFactory.UsingDirective(name).WithLeadingTrivia(compilation.GetLeadingTrivia()));
    }
  }
}
