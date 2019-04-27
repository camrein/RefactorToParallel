using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RefactorToParallel.Analysis.Collectors;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.Optimizer;
using RefactorToParallel.Analysis.Verifier;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis {
  /// <summary>
  /// C# Syntax Analyzer that checks if there any for-loops which can be parallelized with Parallel.For.
  /// </summary>
  public abstract class LoopAnalyzer<TLoopStatementSyntax> : DiagnosticAnalyzer where TLoopStatementSyntax : SyntaxNode {
    private readonly DiagnosticDescriptor _rule;

    /// <summary>
    /// Creates a new loop analyzer instance.
    /// </summary>
    /// <param name="rule">The diagnostic descriptor of the analyzer.</param>
    protected LoopAnalyzer(DiagnosticDescriptor rule) {
      _rule = rule;
    }

    /// <summary>
    /// Initializes this analyzer.
    /// </summary>
    /// <param name="context">The analysis context to utilize.</param>
    public override void Initialize(AnalysisContext context) {
      context.RegisterSemanticModelAction(_AnalyzeSemanticModel);
    }

    private void _AnalyzeSemanticModel(SemanticModelAnalysisContext context) {
      var semanticModel = context.SemanticModel;
      var nodes = semanticModel.SyntaxTree.GetRoot().DescendantNodes().ToList();

      if(!ContainsLoops(nodes, semanticModel)) {
        Debug.WriteLine("syntax tree does not contain any for loops, no analysis necessary");
        return;
      }

      var methods = _CollectMethods(nodes, semanticModel);
      Debug.WriteLine($"got {methods.Count} methods available for interprocedural analysis");
      _AnalyzeLoops(context, nodes, methods);
    }

    private void _AnalyzeLoops(SemanticModelAnalysisContext context, IEnumerable<SyntaxNode> nodes, IDictionary<string, ControlFlowGraph> methods) {
      var analyzer = new Analyzer(_rule, context, methods);

      foreach(var (loop, body, loopIndex, location) in GetLoopsToAnalyze(nodes, context.SemanticModel)) {
        try {
          analyzer.Scan(loop, body, loopIndex, location);
        } catch(UnsupportedSyntaxException e) {
          Debug.WriteLine($"cannot analyze loop: {e.Message}");
        }
      }
    }

    /// <summary>
    /// Gets if any of the nodes is a loop that should be analyzed for parallelization.
    /// </summary>
    /// <param name="nodes">The nodes of the syntax tree.</param>
    /// <param name="semanticModel">The semantic model backing the nodes.</param>
    /// <returns><code>true</code> if there is a loop declared that can be analyzed for parallelization.</returns>
    protected abstract bool ContainsLoops(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel);

    /// <summary>
    /// Gets the the lop statements that should be analyzed if they're parallelizable.
    /// </summary>
    /// <param name="nodes">The nodes of the syntax tree.</param>
    /// <param name="semanticModel">The semantic model backing the nodes.</param>
    /// <returns>A quadruple of the loop statement itself, the body of the loop, the loop index, and the desired reporting location.</returns>
    protected abstract IEnumerable<(TLoopStatementSyntax loopStatement, SyntaxNode body, string loopIndex, Location location)> GetLoopsToAnalyze(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel);

    private static IDictionary<string, ControlFlowGraph> _CollectMethods(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel) {
      // TODO this and many other steps can be parallelized, but how well does this work in combination with visual studio?
      var dictionary = nodes.OfType<MethodDeclarationSyntax>()
        .Where(method => !method.IsVirtual())  // only non-virtual methods because of polymorphism.
        .GroupBy(method => method.Identifier.Text)
        .Where(entry => entry.Count() == 1) // TODO No overloads supported at this time
        .Select(entry => entry.Single())
        .Select(method => _GetCodeOfMethod(method, semanticModel))
        .Where(method => method.Name != null)
        .ToDictionary(method => method.Name, method => method.Cfg);
      dictionary.Add(CodeFactory.SafeApiIdentifier, ControlFlowGraphFactory.Empty);
      return dictionary;
    }

    private static (string Name, ControlFlowGraph Cfg) _GetCodeOfMethod(MethodDeclarationSyntax method, SemanticModel semanticModel) {
      var methodName = method.Identifier.Text;

      try {
        var code = ThreeAddressCodeFactory.Create(CodeFactory.CreateMethod(method, semanticModel));
        var optimized = OptimizationRunner.Optimize(code);

        Debug.WriteLine($"got optimized code for {methodName}");
        return (methodName, ControlFlowGraphFactory.Create(optimized.Code, methodName, true));
      } catch(UnsupportedSyntaxException e) {
        Debug.WriteLine($"method '{methodName}' cannot be utilized in interprocedural anaylsis: {e.Message}");
      }

      return (null, null);
    }

    private class Analyzer {
      private readonly DiagnosticDescriptor _rule;
      private readonly SemanticModelAnalysisContext _context;
      private readonly IDictionary<string, ControlFlowGraph> _methods;
      private readonly SemanticModel _semanticModel;

      public Analyzer(DiagnosticDescriptor rule, SemanticModelAnalysisContext context, IDictionary<string, ControlFlowGraph> methods) {
        _rule = rule;
        _context = context;
        _methods = methods;
        _semanticModel = context.SemanticModel;
      }

      public void Scan(TLoopStatementSyntax loopStatement, SyntaxNode body, string loopIndex, Location location) {
        var code = ThreeAddressCodeFactory.Create(CodeFactory.Create(body, _semanticModel));
        var variableAccesses = VariableAccesses.Collect(code);
        if(!WriteAccessVerifier.HasNoWriteAccessToSharedVariables(variableAccesses)) {
          Debug.WriteLine("the loop contains write accesses to shared variables");
          return;
        }

        if(_HasConflictingArrayAccesses(loopStatement, loopIndex, code, variableAccesses)) {
          Debug.WriteLine("the loop contains loop carried dependencies");
          return;
        }

        _ReportLoopForParallelization(loopStatement, location);
      }

      private bool _HasConflictingArrayAccesses(TLoopStatementSyntax loopStatement, string loopIndex, Code code, VariableAccesses variableAccesses) {
        var optimized = OptimizationRunner.Optimize(code);
        code = optimized.Code;
        variableAccesses = optimized.Changed ? VariableAccesses.Collect(code) : variableAccesses;

        var cfg = ControlFlowGraphFactory.Create(code, true);
        var callGraph = CallGraphFactory.Create(cfg, _methods);
        var calledMethods = callGraph.Methods.Select(methodName => _methods[methodName]).ToImmutableList();
        var aliasAnalysis = AliasAnalysis.Analyze(cfg, callGraph, _methods.Values, _GetAliasesAtLoopEntry(loopStatement, variableAccesses));

        return ArrayAccessVerifier.HasConflictingAccesses(loopIndex, cfg, aliasAnalysis, callGraph, calledMethods);
      }

      private IEnumerable<VariableAlias> _GetAliasesAtLoopEntry(TLoopStatementSyntax loopStatement, VariableAccesses variableAccesses) {
        // TODO maybe use a different Expression to setup the input aliases instead of variables. This might cause trouble when forgetting this situation in further changes.
        var filtered = variableAccesses.DeclaredVariables
          .Concat(variableAccesses.ReadArrays.Concat(variableAccesses.WrittenArrays).Select(array => array.Name))
          .ToImmutableHashSet();

        var variables = variableAccesses.ReadVariables
          .Union(variableAccesses.WrittenVariables)
          .Except(filtered)
          .Select(variableName => new VariableAlias(variableName, new VariableExpression(variableName)))
          .Concat(ExternalArrayAliasCollector.Collect(_semanticModel, loopStatement, variableAccesses))
          .ToImmutableHashSet();

        return variables;
      }

      private void _ReportLoopForParallelization(TLoopStatementSyntax loopStatement, Location location) {
        var diagnostic = Diagnostic.Create(_rule, location, ImmutableList.Create(loopStatement.GetLocation()));
        _context.ReportDiagnostic(diagnostic);
      }
    }
  }
}
