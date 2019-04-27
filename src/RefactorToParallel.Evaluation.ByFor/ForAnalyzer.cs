using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace RefactorToParallel.Evaluation.ByFor {
  /// <summary>
  /// C# Syntax Analyzer that checks if there any for-loops which can be parallelized with Parallel.For.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ForAnalyzer : DiagnosticAnalyzer {
    /// <summary>
    /// The diagnostic ID of the analyzer.
    /// </summary>
    public const string DiagnosticId = "FOR";

    private const int MaxOptimizationIterations = 5;
    private const string Category = "Evaluation";

    private static readonly LocalizableString Title = "For Loop";
    private static readonly LocalizableString MessageFormat = "A conventional for loop.";
    private static readonly LocalizableString Description = "";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    /// <summary>
    /// Gets the supported diagnostics of this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <summary>
    /// Initializes this analyzer.
    /// </summary>
    /// <param name="context">The analysis context to utilize.</param>
    public override void Initialize(AnalysisContext context) {
      context.RegisterSyntaxNodeAction(_AnalyzeForLoop, ImmutableArray.Create(SyntaxKind.ForStatement));
    }

    private static void _AnalyzeForLoop(SyntaxNodeAnalysisContext context) {
      var forStatement = (ForStatementSyntax)context.Node;
      var location = Location.Create(forStatement.SyntaxTree, TextSpan.FromBounds(forStatement.SpanStart, forStatement.CloseParenToken.Span.End));
      var diagnostic = Diagnostic.Create(Rule, location, ImmutableList.Create(forStatement.GetLocation()));
      context.ReportDiagnostic(diagnostic);
    }
  }
}
