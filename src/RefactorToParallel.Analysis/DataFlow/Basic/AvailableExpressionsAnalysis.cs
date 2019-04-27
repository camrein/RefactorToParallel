using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodeDataMap = System.Collections.Generic.IDictionary<RefactorToParallel.Analysis.ControlFlow.FlowNode, System.Collections.Generic.ISet<RefactorToParallel.Analysis.DataFlow.Basic.AvailableExpression>>;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Available expressions analysis
  /// </summary>
  public class AvailableExpressionsAnalysis {
    /// <summary>
    /// Gets the data states of the nodes.
    /// </summary>
    public NodeDataMap Entry { get; }

    /// <summary>
    /// Gets the exit states of the nodes.
    /// </summary>
    public NodeDataMap Exit { get; }

    private AvailableExpressionsAnalysis(NodeDataMap entry, NodeDataMap exit) {
      Entry = entry;
      Exit = exit;
    }

    /// <summary>
    /// Applies the available expressions analysis on the desired control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to apply the analysis on.</param>
    /// <returns>The result of the available expressions analysis.</returns>
    public static AvailableExpressionsAnalysis Analyze(ControlFlowGraph cfg) {
      var instance = new Analyzer(cfg);
      instance.Initialize();
      var result = instance.Solve();
      return new AvailableExpressionsAnalysis(result.MfpIn, result.MfpOut);
    }

    private class Analyzer : ForwardFlowAnalysis<AvailableExpression> {
      private static readonly ISet<AvailableExpression> _emptySet = ImmutableHashSet<AvailableExpression>.Empty;

      private NodeDataMap _kill;
      private NodeDataMap _gen;
      private ISet<AvailableExpression> _availableExpressions;
      private IDictionary<string, ISet<AvailableExpression>> _variableUsedByExpressions;

      protected override ISet<AvailableExpression> ExtremalValues => _emptySet;
      protected override ISet<AvailableExpression> LeastValues => _availableExpressions;

      public Analyzer(ControlFlowGraph cfg) : base(cfg) { }

      protected override bool IncreasesAnalysisKnowledge(ISet<AvailableExpression> transferred, ISet<AvailableExpression> currentKnowledge) {
        return !transferred.IsSupersetOf(currentKnowledge);
      }

      protected override ISet<AvailableExpression> Merge(ISet<AvailableExpression> transferred, ISet<AvailableExpression> currentKnowledge) {
        return transferred.Intersect(currentKnowledge).ToImmutableHashSet();
      }

      protected override ISet<AvailableExpression> Transfer(FlowNode node, ISet<AvailableExpression> data) {
        return data.Except(_kill[node]).Union(_gen[node]).ToImmutableHashSet();
      }

      public void Initialize() {
        _InitializeGen();
        _InitializeAvailableExpressions();
        _InitializeVariableUsage();
        _InitializeKill();
      }

      private void _InitializeGen() {
        _gen = Nodes.ToImmutableDictionary(node => node, node => _CollectGenForNode(node));
      }

      private ISet<AvailableExpression> _CollectGenForNode(FlowNode node) {
        switch(node.Instruction) {
        case Assignment assignment:
          // In the three-address code, array accessors are already extracted in temporary variables if they're binary expressions.
          // Therefore, the array accessors do not need to be checked.
          return _GetAvailableExpressions(node, (assignment.Left as VariableExpression)?.Name, assignment.Right);
        case ConditionalJump jump:
          return _GetAvailableExpressions(node, null, jump.Condition);
        default:
          return _emptySet;
        }
      }

      private ISet<AvailableExpression> _GetAvailableExpressions(FlowNode node, string variableName, Expression expression) {
        var available = _CreateAvailableExpression(node, expression);
        if(available == null) {
          return _emptySet;
        }

        if(variableName != null && available.AccessedVariables.Contains(variableName)) {
          return _emptySet;
        }

        return ImmutableHashSet.Create(available);
      }

      private AvailableExpression _CreateAvailableExpression(FlowNode node, Expression expression) {
        switch(expression) {
        case ComparisonExpression comparison:
          // TODO maybe it is useful to include comparisons into the analysis?
          break;
        case BinaryExpression binary:
          return new AvailableExpression(node, binary);
        case InvocationExpression invocation:
          return new AvailableExpression(node, invocation);
        }

        return null;
      }

      private void _InitializeAvailableExpressions() {
        _availableExpressions = _gen.Values.SelectMany(expressions => expressions).ToImmutableHashSet();
      }

      private void _InitializeVariableUsage() {
        _variableUsedByExpressions = _availableExpressions
          .SelectMany(expression => expression.AccessedVariables.Select(variable => new { Variable = variable, Expression = expression }))
          .GroupBy(tuple => tuple.Variable)
          .ToImmutableDictionary(
            tuple => tuple.Key,
            tuple => (ISet<AvailableExpression>)tuple.Select(entry => entry.Expression).ToImmutableHashSet()
          );
      }

      private void _InitializeKill() {
        _kill = Nodes.ToImmutableDictionary(node => node, node => _GetKilledExpressions(node));
      }

      private ISet<AvailableExpression> _GetKilledExpressions(FlowNode node) {
        string variable = null;
        switch(node.Instruction) {
        case Assignment assignment:
          variable = (assignment.Left as VariableExpression)?.Name;
          break;
        case Declaration declaration:
          variable = declaration.Name;
          break;
        }

        if(variable != null && _variableUsedByExpressions.TryGetValue(variable, out var killed)) {
          return killed;
        }

        return _emptySet;
      }
    }
  }
}

