using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Evaluation.ByParallelFor {
  /// <summary>
  /// C# Syntax Analyzer that checks if there any for-loops which can be parallelized with Parallel.For.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ParallelForAnalyzer : DiagnosticAnalyzer {
    /// <summary>
    /// The diagnostic ID of the analyzer.
    /// </summary>
    public const string DiagnosticId = "EX_PAR_FOR";

    private const int MaxOptimizationIterations = 5;
    private const string Category = "Evaluation";

    private static readonly LocalizableString Title = "Parallel.For Loop";
    private static readonly LocalizableString MessageFormat = "A Parallel.For loop.";
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
      context.RegisterSemanticModelAction(_AnalyzeSemanticModel);
    }

    private static void _AnalyzeSemanticModel(SemanticModelAnalysisContext context) {
      var semanticModel = context.SemanticModel;
      var nodes = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Where(i => i.IsParallelFor(context.SemanticModel));

      foreach(InvocationExpressionSyntax invocation in nodes) {
        _AnalyzeParallelForLoop(invocation, context);
      }
    }

    private static void _AnalyzeParallelForLoop(InvocationExpressionSyntax parallelForInvocation, SemanticModelAnalysisContext context) {
      var diagnostic = Diagnostic.Create(Rule, parallelForInvocation.Expression.GetLocation());
      context.ReportDiagnostic(diagnostic);
    }
  }
}
