using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Extensions {
  /// <summary>
  /// Extension methods for use with properties.
  /// </summary>
  public static class PropertyExtensions {
    // Note: Expression Properties are currently not supported because they need special treatment in terms of aliasing.

    /// <summary>
    /// Checks if the property is a locally defined (aka non-virtual) auto-property.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <param name="semanticModel">The semantic model containing the property.</param>
    /// <returns><code>True</code> if the property does not have any side-effects.</returns>
    public static bool IsLocalAutoProperty(this IPropertySymbol property, SemanticModel semanticModel) {
      return !property.IsVirtual && !property.IsOverride && property.IsAutoProperty(semanticModel);
    }

    /// <summary>
    /// Checks if the property is an auto-property.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <param name="semanticModel">The semantic model containing the property.</param>
    /// <returns><code>True</code> if the property is an auto-property.</returns>
    public static bool IsAutoProperty(this IPropertySymbol property, SemanticModel semanticModel) {
      var declarations = property.DeclaringSyntaxReferences.ToImmutableArray();
      if(declarations.Length != 1) {
        return false;
      }

      var propertyDeclaration = declarations.Single().GetSyntax() as PropertyDeclarationSyntax;
      return propertyDeclaration.AccessorList?.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null) == true;
    }
  }
}
