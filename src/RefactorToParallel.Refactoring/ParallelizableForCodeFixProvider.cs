using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorToParallel.Analysis;
using RefactorToParallel.Refactoring.Generators;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorToParallel.Refactoring {
  /// <summary>
  /// Code fix provider that allows straightforward refactoring from simple for-loops to Parallel.For.
  /// </summary>
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParallelizableForCodeFixProvider)), Shared]
  public class ParallelizableForCodeFixProvider : CodeFixProvider {
    private const string title = "Refactor to Parallel.For";

    /// <summary>
    /// Gets the ids of the diagnostics that can be fixed by this code fix provider.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ParallelizableForAnalyzer.DiagnosticId);

    /// <summary>
    /// Gets the fix all provider for this code fixer.
    /// </summary>
    /// <returns>The fix all provider.</returns>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Registers the code fix to the context.
    /// </summary>
    /// <param name="context">The context to register the code fix to.</param>
    /// <returns>The asynchronous task of the code fix registration.</returns>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;
      var statement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ForStatementSyntax>().First();

      context.RegisterCodeFix(
          CodeAction.Create(
              title: title,
              createChangedDocument: cancellationToken => _RefactorToParallelForAsync(context.Document, statement, cancellationToken),
              equivalenceKey: title),
          diagnostic);
    }

    private async Task<Document> _RefactorToParallelForAsync(Document document, ForStatementSyntax statement, CancellationToken cancellationToken) {
      var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
      root = root.ReplaceNode(statement, _CreateParallelFor(statement));

      if(root is CompilationUnitSyntax compilation) {
        root = UsingGenerator.AddUsingIfMissing(compilation, NameGenerator.CreateName("System", "Threading", "Tasks"));
      }

      return document.WithSyntaxRoot(root);
    }

    private static ExpressionStatementSyntax _CreateParallelFor(ForStatementSyntax statement) {
      var variable = statement.Declaration.Variables.Single();
      var from = variable.Initializer.Value;

      // The analyzer only allows binary expressions where the upper bound is on the left hand-side.
      // Therefore, no further checks are necessary.
      var binary = (BinaryExpressionSyntax)statement.Condition;
      var to = binary.Right;
      if(binary.IsKind(SyntaxKind.LessThanOrEqualExpression)) {
        // TODO maybe try to reduce resulting expressions like ...-1+1
        to = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, to, _CreateOneLiteral());
      }

      return SyntaxFactory.ExpressionStatement(
        SyntaxFactory.InvocationExpression(
          MemberAccessGenerator.CreateMemberAccess("Parallel", "For"),
          ArgumentListGenerator.CreateArgumentList(from, to, LambdaGenerator.CreateLambdaOrDelegate(variable.Identifier, statement.Statement))
        )
      ).WithTriviaFrom(statement);
    }

    private static LiteralExpressionSyntax _CreateOneLiteral() {
      return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
    }
  }
}