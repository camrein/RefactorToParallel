using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Extensions {
  /// <summary>
  /// Extension methods for use with methods.
  /// </summary>
  public static class MethodExtensions {
    // List of types where all public static members are thread-safe. Methods accepting arrays are automatically filtered at this time.
    // TODO when this list gets to large, it should be converted into a more suitable data-structure (e.g. an index).
    private static TypeDescriptor[] SafeTypes = {
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

      new TypeDescriptor("mscorlib", "string"),

      new TypeDescriptor("mscorlib", "System.Math"),
      new TypeDescriptor("mscorlib", "System.Convert"),
      new TypeDescriptor("mscorlib", "System.Guid"),
      new TypeDescriptor("mscorlib", "System.Runtime.InteropServices.Marshal"),

      new TypeDescriptor("System", "System.Net.IPAddress"),
      new TypeDescriptor("System.Core", "System.Linq.Expressions.Expression"),
      new TypeDescriptor("System.Drawing", "System.Drawing.Color"),

      new TypeDescriptor("PresentationCore", "System.Windows.Media.Color"),
    };

    /// <summary>
    /// Checks if the given method is virtual.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns><code>True</code> if the method is virtual.</returns>
    public static bool IsVirtual(this MethodDeclarationSyntax method) {
      return method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.VirtualKeyword) || modifier.IsKind(SyntaxKind.OverrideKeyword));
    }

    /// <summary>
    /// Checks if the given method is pure API method / has no side effects.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns><code>True</code> if the method is pure.</returns>
    public static bool IsPureApi(this IMethodSymbol method) {
      if(method == null) {
        return false;
      }

      if(!method.IsStatic) {
        Debug.WriteLine($"detected non-static method '{method}'");
        return false;
      }

      // The usage of array based parameters are prevented at this time. The "SafeApi" CFG would require proper setup.
      // A first step to improve this filter is modelling the CFG so it reads the array at a constant position.
      if(method.Parameters.Any(parameter => parameter.Type is IArrayTypeSymbol)) {
        Debug.WriteLine($"detected method accepting arrays '{method}'");
        return false;
      }

      // This check is necessary to prevent that arrays are encaspulated in an object which is then passed to a method.
      // For example passing the array reference as an enumerable. However, it probably prevents benign cases.
      // It would be safer/preferable to create an actual list of supported method. But this requires a lot of effort.
      if(method.Parameters.Any(parameter => !parameter.Type.IsPrimitiveType())) {
        Debug.WriteLine($"detected method accepting unsupported argument types '{method}'");
        return false;
      }

      if(!SafeTypes.Any(descriptor => descriptor.IsType(method.ContainingType))) {
        Debug.WriteLine($"method '{method}' has not been identified as safe api");
        return false;
      }

      return true;
    }
  }
}
