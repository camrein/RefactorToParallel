using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorToParallel.Refactoring.Generators {
  /// <summary>
  /// Method collection for generating lambda expressions.
  /// </summary>
  public static class LambdaGenerator {
    /// <summary>
    /// Creates a lambda expression with the provided identifier and body.
    /// </summary>
    /// <param name="identifier">The identifier of the parameter of the lambda expression.</param>
    /// <param name="body">The body of the lambda expression to generate.</param>
    /// <returns>The generated lambda expression.</returns>
    public static SimpleLambdaExpressionSyntax CreateLambda(SyntaxToken identifier, StatementSyntax body) {
      var statement = body.WithoutTrivia();
      if(!(statement is BlockSyntax)) {
        // Safest way for conversion. E.g. when nesting multiple loops without a block.
        statement = SyntaxFactory.Block(statement);
      }

      return SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(identifier), statement);
    }

    /// <summary>
    /// Creates a lambda expression with the provided identifier and body. If possible, it converts the body to a delegate.
    /// </summary>
    /// <param name="identifier">The identifier of the parameter of the lambda expression.</param>
    /// <param name="body">The body of the lambda expression to generate.</param>
    /// <returns>The generated lambda expression or delegate.</returns>
    public static ExpressionSyntax CreateLambdaOrDelegate(SyntaxToken identifier, StatementSyntax body) {
      var expression = body as ExpressionStatementSyntax;
      if(expression == null && body is BlockSyntax block && block.Statements.Count == 1) {
        expression = block.Statements.Single() as ExpressionStatementSyntax;
      }

      var invocation = expression?.Expression as InvocationExpressionSyntax;
      var argument = invocation?.ArgumentList.Arguments.FirstOrDefault()?.Expression as IdentifierNameSyntax;

      // TODO replace ToString() with symbol lookup? => Symbols cannot be resolved for SyntaxTokens.
      if(invocation?.ArgumentList.Arguments.Count != 1 || !Equals(identifier.ToString(), argument?.Identifier.ToString())) {
        return CreateLambda(identifier, body);
      }

      return invocation.Expression;
    }
  }
}
