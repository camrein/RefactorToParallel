using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactorToParallel.Analysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Evaluation.ByParallelFor {
  /// <summary>
  /// C# Syntax Analyzer that checks if there is an existing Parallel.For use that is considered parallelizable.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ParallelizableParallelForAnalyzer : LoopAnalyzer<InvocationExpressionSyntax> {
    /// <summary>
    /// The diagnostic ID of the analyzer.
    /// </summary>
    public const string DiagnosticId = "COR_PAR_FOR";

    private const string Category = "Concurrency";

    private static readonly LocalizableString Title = "Correct Parallel.For.";
    private static readonly LocalizableString MessageFormat = "A Parallel.For loop that is free of concurrency errors.";
    private static readonly LocalizableString Description = "";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    /// <summary>
    /// Creates a new instance of the parallelizable for loop analyzer.
    /// </summary>
    public ParallelizableParallelForAnalyzer() : base(Rule) { }

    /// <summary>
    /// Gets the supported diagnostics of this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override bool ContainsLoops(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel) {
      return nodes.OfType<InvocationExpressionSyntax>().Any(i => i.IsParallelFor(semanticModel));
    }

    protected override IEnumerable<(InvocationExpressionSyntax loopStatement, SyntaxNode body, string loopIndex, Location location)> GetLoopsToAnalyze(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel) {
      // TODO support delegates
      return nodes.OfType<InvocationExpressionSyntax>()
        .Where(invocation => invocation.IsParallelFor(semanticModel))
        .Where(invocation => _IsSupportedParallelFor(invocation))
        .Select(invocation => (invocation, _GetBody(invocation), _GetLoopIndex(invocation), invocation.Expression.GetLocation()));
    }

    private bool _IsSupportedParallelFor(InvocationExpressionSyntax invocation) {
      if(invocation.ArgumentList.Arguments.Count > 4) {
        return false;
      }

      var body = _GetBody(invocation);
      var loopIndex = _GetLoopIndex(invocation);
      return body != null && loopIndex != null;
    }

    private IEnumerable<LambdaExpressionSyntax> _GetLambdaExpressions(InvocationExpressionSyntax invocation) {
      return invocation.ArgumentList.Arguments.Select(a => a.Expression).OfType<LambdaExpressionSyntax>();
    }

    private SyntaxNode _GetBody(InvocationExpressionSyntax invocation) {
      return _GetLambdaExpressions(invocation).SingleOrDefault()?.Body;
    }

    private string _GetLoopIndex(InvocationExpressionSyntax invocation) {
      return _GetLambdaExpressions(invocation).OfType<SimpleLambdaExpressionSyntax>().FirstOrDefault()?.Parameter?.Identifier.Text
        ?? _GetLambdaExpressions(invocation).OfType<ParenthesizedLambdaExpressionSyntax>().FirstOrDefault()?.ParameterList?.Parameters.FirstOrDefault()?.Identifier.Text;
    }
  }
}
