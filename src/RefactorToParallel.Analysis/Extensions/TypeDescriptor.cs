using Microsoft.CodeAnalysis;

namespace RefactorToParallel.Analysis.Extensions {
  /// <summary>
  /// Holds information about a given type.
  /// </summary>
  internal class TypeDescriptor {
    private readonly string _assemblyName;
    private readonly string _typeName;

    /// <summary>
    /// Creates a new type descriptor.
    /// </summary>
    /// <param name="assemblyName">The assembly where the type is located.</param>
    /// <param name="typeName">The fully-qualified type name.</param>
    public TypeDescriptor(string assemblyName, string typeName) {
      _assemblyName = assemblyName;
      _typeName = typeName;
    }

    /// <summary>
    /// Checks if the given type symbol is of the expected type.
    /// </summary>
    /// <param name="type">The type symbol to check against.</param>
    /// <returns><code>True</code> if the type symbol is of the same type.</returns>
    public bool IsType(ITypeSymbol type) {
      return Equals(_typeName, type?.ToString()) && Equals(_assemblyName, type?.ContainingAssembly?.Name);
    }
  }
}
