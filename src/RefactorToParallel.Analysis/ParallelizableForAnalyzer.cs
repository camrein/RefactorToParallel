using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RefactorToParallel.Analysis.Verifier;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis {
  /// <summary>
  /// C# Syntax Analyzer that checks if there any for-loops which can be parallelized with Parallel.For.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ParallelizableForAnalyzer : LoopAnalyzer<ForStatementSyntax> {
    /// <summary>
    /// The diagnostic ID of the analyzer.
    /// </summary>
    public const string DiagnosticId = "PAR_FOR";

    private const string Category = "Concurrency";

    private static readonly LocalizableString Title = "Refactor for to Parallel.";
    private static readonly LocalizableString MessageFormat = "The for loop can be refactored to a Parallel.For loop.";
    private static readonly LocalizableString Description = "";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    /// <summary>
    /// Creates a new instance of the parallelizable for loop analyzer.
    /// </summary>
    public ParallelizableForAnalyzer() : base(Rule) { }

    /// <summary>
    /// Gets the supported diagnostics of this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override bool ContainsLoops(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel) {
      return nodes.OfType<ForStatementSyntax>().Any();
    }

    protected override IEnumerable<(ForStatementSyntax loopStatement, SyntaxNode body, string loopIndex, Location location)> GetLoopsToAnalyze(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel) {
      return nodes.OfType<ForStatementSyntax>()
        .Where(forStatement => LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel))
        .Select(forStatement => (
          forStatement, 
          (SyntaxNode)forStatement.Statement,
          forStatement.Declaration.Variables.Single().Identifier.Text,
          // The location span of the for-loop includes the loop's body. Therefore, a custom location is created.
          Location.Create(forStatement.SyntaxTree, TextSpan.FromBounds(forStatement.SpanStart, forStatement.CloseParenToken.Span.End))
        ));
    }
  }
}
