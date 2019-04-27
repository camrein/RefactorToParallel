using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Extensions {
  /// <summary>
  /// Extension methods for use with types.
  /// </summary>
  public static class TypeExtensions {
    /// <summary>
    /// Gets the type descriptor for strings.
    /// </summary>
    internal static TypeDescriptor StringType { get; } = new TypeDescriptor("mscorlib", "string");

    /// <summary>
    /// Gets the primitive types.
    /// </summary>
    internal static ImmutableArray<TypeDescriptor> PrimitiveTypes { get; } = ImmutableArray.Create(
      new TypeDescriptor("mscorlib", "bool"),
      new TypeDescriptor("mscorlib", "byte"),
      new TypeDescriptor("mscorlib", "char"),
      new TypeDescriptor("mscorlib", "decimal"),
      new TypeDescriptor("mscorlib", "double"),
      new TypeDescriptor("mscorlib", "enum"),
      new TypeDescriptor("mscorlib", "float"),
      new TypeDescriptor("mscorlib", "int"),
      new TypeDescriptor("mscorlib", "long"),
      new TypeDescriptor("mscorlib", "sbyte"),
      new TypeDescriptor("mscorlib", "short"),
      new TypeDescriptor("mscorlib", "uint"),
      new TypeDescriptor("mscorlib", "ulong"),
      new TypeDescriptor("mscorlib", "ushort"),

      StringType
    );

    /// <summary>
    /// Gets the base types. This list contains all primitive types including the <code>object</code> type.
    /// </summary>
    internal static ImmutableArray<TypeDescriptor> BaseTypes { get; } = PrimitiveTypes.Add(new TypeDescriptor("mscorlib", "object"));

    /// <summary>
    /// Checks if the given type symbol represents a primitive type.
    /// </summary>
    /// <param name="type">The type symbol to check.</param>
    /// <returns><code>True</code> if the given type symbol is a primitive type.</returns>
    public static bool IsPrimitiveType(this ITypeSymbol type) {
      return PrimitiveTypes.Any(primitiveType => primitiveType.IsType(type));
    }

    /// <summary>
    /// Checks if the given type symbol represents a base type.
    /// </summary>
    /// <param name="type">The type symbol to check.</param>
    /// <returns><code>True</code> if the given type symbol is a base type.</returns>
    public static bool IsBaseType(this ITypeSymbol type) {
      return BaseTypes.Any(baseType => baseType.IsType(type));
    }

    /// <summary>
    /// Checks if the given type is a string type.
    /// </summary>
    /// <param name="type">The type symbol to check.</param>
    /// <returns><code>True</code> if the given type symbol is a string type.</returns>
    public static bool IsStringType(this ITypeSymbol type) {
      return StringType.IsType(type);
    }

    /// <summary>
    /// Checks if the given expression is a member method of a primitive type (e.g. operator overload).
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <param name="semanticModel">The semantic model containing the given node.</param>
    /// <returns><code>True</code> if the given node is a member method of a primitive type.</returns>
    public static bool IsMemberMethodOfPrimitiveType(this SyntaxNode node, SemanticModel semanticModel) {
      var symbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
      return symbol != null && symbol.ContainingType.IsPrimitiveType();
    }

    /// <summary>
    /// Checks if the given binary expression is a custom operator overload.
    /// </summary>
    /// <param name="expression">The binary expression to check.</param>
    /// <param name="semanticModel">The semantic model containing the given expression.</param>
    /// <returns><code>True</code> if the given expression is a custom operator overload.</returns>
    public static bool IsOverloadedBinaryOperator(this BinaryExpressionSyntax expression, SemanticModel semanticModel) {
      // Operators that cannot be overloaded do not require a check (they also have no defining type).
      switch(expression.Kind()) {
      case SyntaxKind.LogicalAndExpression:
      case SyntaxKind.LogicalOrExpression:
        return false;
      }

      // object is allowed in this case because the == and != operators are statically resolved.
      // Therefore, if the containing type is "object" and thus it is a reference equality comparision.
      // Most importantly, the compiler prevents overloading of binary operators accepting solely object typed arguments.
      var symbol = semanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;
      return symbol == null || !symbol.ContainingType.IsBaseType();
    }

    /// <summary>
    /// Checks if the given expression evaluates to a primitive type.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <param name="semanticModel">The semantic model containing the given expression.</param>
    /// <returns><code>True</code> if the given expression will evaluate to a primitive type.</returns>
    public static bool IsEvaluatingToPrimitiveType(this ExpressionSyntax expression, SemanticModel semanticModel) {
      var type = semanticModel.GetTypeInfo(expression).Type;
      return type != null && type.IsPrimitiveType();
    }
  }
}
