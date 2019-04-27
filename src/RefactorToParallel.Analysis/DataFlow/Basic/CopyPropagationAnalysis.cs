using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodeDataMap = System.Collections.Generic.IDictionary<RefactorToParallel.Analysis.ControlFlow.FlowNode, System.Collections.Generic.ISet<RefactorToParallel.Analysis.DataFlow.Basic.VariableCopy>>;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Copy propagation analysis
  /// </summary>
  public class CopyPropagationAnalysis {
    /// <summary>
    /// Gets the states at the entry of a node.
    /// </summary>
    public NodeDataMap Entry { get; }

    /// <summary>
    /// Gets the states at the exit of a node.
    /// </summary>
    public NodeDataMap Exit { get; }

    private CopyPropagationAnalysis(NodeDataMap entry, NodeDataMap exit) {
      Entry = entry;
      Exit = exit;
    }

    /// <summary>
    /// Applies the copy propagation analysis on the desired control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to apply the analysis on.</param>
    /// <returns>The result of the copy propagation analysis.</returns>
    public static CopyPropagationAnalysis Analyze(ControlFlowGraph cfg) {
      var instance = new Analyzer(cfg);
      instance.Initialize();
      var result = instance.Solve();
      return new CopyPropagationAnalysis(result.MfpIn, result.MfpOut);
    }

    private class Analyzer : ForwardFlowAnalysis<VariableCopy> {
      private static readonly ISet<VariableCopy> _emptySet = ImmutableHashSet<VariableCopy>.Empty;

      private NodeDataMap _kill;
      private NodeDataMap _gen;
      private ISet<VariableCopy> _copies;
      private IDictionary<string, ISet<VariableCopy>> _variableToUsingCopiesMapping;

      protected override ISet<VariableCopy> ExtremalValues => _emptySet;
      protected override ISet<VariableCopy> LeastValues => _copies;

      public Analyzer(ControlFlowGraph cfg) : base(cfg) { }

      protected override bool IncreasesAnalysisKnowledge(ISet<VariableCopy> transferred, ISet<VariableCopy> currentKnowledge) {
        return !transferred.IsSupersetOf(currentKnowledge);
      }

      protected override ISet<VariableCopy> Merge(ISet<VariableCopy> transferred, ISet<VariableCopy> currentKnowledge) {
        return transferred.Intersect(currentKnowledge).ToImmutableHashSet();
      }

      protected override ISet<VariableCopy> Transfer(FlowNode node, ISet<VariableCopy> data) {
        return data.Except(_kill[node]).Union(_gen[node]).ToImmutableHashSet();
      }

      public void Initialize() {
        _InitializeGen();
        _InitializeCopies();
        _InitializeVariableUsage();
        _InitializeKill();
      }

      private void _InitializeGen() {
        _gen = Nodes.ToImmutableDictionary(node => node, node => _CollectGenForNode(node));
      }

      private ISet<VariableCopy> _CollectGenForNode(FlowNode node) {
        var assignment = node.Instruction as Assignment;
        if(assignment == null) {
          return _emptySet;
        }

        var left = assignment.Left as VariableExpression;
        var right = assignment.Right as VariableExpression;
        if(left == null || right == null) {
          return _emptySet;
        }

        return ImmutableHashSet.Create(new VariableCopy(node, left.Name, right.Name));
      }

      private void _InitializeCopies() {
        _copies = _gen.Values.SelectMany(copy => copy).ToImmutableHashSet();
      }

      private void _InitializeVariableUsage() {
        // Any redefinition of 'x' or 'y' in the copies 'x = y' and 'y = x' has to be in the kill-set.
        _variableToUsingCopiesMapping = _copies
          .Select(copy => new { Copy = copy, Variable = copy.SourceVariable })
          .Concat(_copies.Select(copy => new { Copy = copy, Variable = copy.TargetVariable }))
          .GroupBy(tuple => tuple.Variable)
          .ToImmutableDictionary(
            entry => entry.Key,
            entry => (ISet<VariableCopy>)entry.Select(tuple => tuple.Copy).ToImmutableHashSet()
          );
      }

      private void _InitializeKill() {
        _kill = Nodes.ToImmutableDictionary(node => node, node => _GetKillSet(node));
      }

      private ISet<VariableCopy> _GetKillSet(FlowNode node) {
        var assignment = node.Instruction as Assignment;
        if(assignment == null) {
          return _emptySet;
        }

        var variableName = (assignment.Left as VariableExpression)?.Name;
        if(variableName == null) {
          return _emptySet;
        }

        if(variableName != null && _variableToUsingCopiesMapping.TryGetValue(variableName, out var killed)) {
          return killed;
        }

        return _emptySet;
      }
    }
  }
}
