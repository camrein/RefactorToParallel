using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace RefactorToParallel.Analysis.Extensions {
  /// <summary>
  /// Extension methods for use with methods.
  /// </summary>
  public static class MemberExtensions {
    /// <summary>
    /// Checks if the given member access chain is a safe field access.
    /// </summary>
    /// <param name="memberAccess">The member access to check.</param>
    /// <param name="semanticModel">The semantic model backing the member access.</param>
    /// <returns><code>True</code> if the member accesses are safe field accesses.</returns>
    public static bool IsSafeFieldAccess(this MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) {
      SyntaxNode current = memberAccess;
      
      while(current != null) {
        if(!_IsSafeAccess(current, semanticModel)) {
          Debug.WriteLine($"the member access {memberAccess} appears to be unsafe");
          return false;
        }

        if(current is MemberAccessExpressionSyntax member) {
          current = member.Expression;
        } else {
          break;
        }
      }

      return current != null;
    }

    private static bool _IsSafeAccess(SyntaxNode node, SemanticModel semanticModel) {
      var symbol = semanticModel.GetSymbolInfo(node).Symbol;
      switch(symbol) {
      case IFieldSymbol field:
        // TODO currently allowing only readonly and const. Otherwise, adjustments to the interprocederual analysis (IR Generation of Methods) are necessary.
        if(!field.IsConst && !field.IsReadOnly) {
          Debug.WriteLine($"field '{field}' of node '{node}' is neither const nor readonly");
          return false;
        } else if(field.Type is IArrayTypeSymbol) {
          Debug.WriteLine($"detected array retrieval '{field}'");
          return false;
        }
        return true;
      case INamespaceOrTypeSymbol type:
        return true;
      default:
        Debug.WriteLine($"detected access to unsupported member '{node}' (symbol='{symbol}', type='{node.GetType()}')");
        return false;
      }
    }
  }
}
