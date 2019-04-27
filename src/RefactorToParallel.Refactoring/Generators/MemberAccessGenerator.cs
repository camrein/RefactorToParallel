using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorToParallel.Refactoring.Generators {
  /// <summary>
  /// Method collection for generating member access syntaxes.
  /// </summary>
  public static class MemberAccessGenerator {
    /// <summary>
    /// Creates a member access out of the given components.
    /// </summary>
    /// <param name="type">The root type of the member access.</param>
    /// <param name="member">The first member accessed.</param>
    /// <param name="tail">All succeeding member accesses.</param>
    /// <returns>The generated member access.</returns>
    public static MemberAccessExpressionSyntax CreateMemberAccess(string type, string member, params string[] tail) {
      return CreateMemberAccess(SyntaxFactory.IdentifierName(type), member, tail);
    }

    /// <summary>
    /// Creates a member access out of the given components.
    /// </summary>
    /// <param name="expression">The root expression of the member access.</param>
    /// <param name="member">The first member accessed.</param>
    /// <param name="tail">All succeeding member accesses.</param>
    /// <returns>The generated member access.</returns>
    public static MemberAccessExpressionSyntax CreateMemberAccess(ExpressionSyntax expression, string member, params string[] tail) {
      var memberAccess = SyntaxFactory.MemberAccessExpression(
        SyntaxKind.SimpleMemberAccessExpression,
        expression,
        SyntaxFactory.IdentifierName(member)
      );

      return tail.Aggregate(memberAccess,
        (memo, component) => SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          memo,
          SyntaxFactory.IdentifierName(component)
        )
      );
    }
  }
}
