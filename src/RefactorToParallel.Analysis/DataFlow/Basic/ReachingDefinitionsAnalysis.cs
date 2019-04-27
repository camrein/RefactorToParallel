using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodeDataMap = System.Collections.Generic.IDictionary<RefactorToParallel.Analysis.ControlFlow.FlowNode, System.Collections.Generic.ISet<RefactorToParallel.Analysis.DataFlow.Basic.ReachingDefinitionsTuple>>;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Reaching definitions analysis
  /// </summary>
  public class ReachingDefinitionsAnalysis {
    /// <summary>
    /// Gets the data states of the nodes.
    /// </summary>
    public NodeDataMap Entry { get; }

    /// <summary>
    /// Gets the exit states of the nodes.
    /// </summary>
    public NodeDataMap Exit { get; }

    private ReachingDefinitionsAnalysis(NodeDataMap entry, NodeDataMap exit) {
      Entry = entry;
      Exit = exit;
    }

    /// <summary>
    /// Applies the reaching definitions analysis on the desired control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to apply the analysis on.</param>
    /// <returns>The result of the reaching definitions analysis.</returns>
    public static ReachingDefinitionsAnalysis Analyze(ControlFlowGraph cfg) {
      var instance = new Analyzer(cfg);
      instance.Initialize();
      var result = instance.Solve();
      return new ReachingDefinitionsAnalysis(result.MfpIn, result.MfpOut);
    }

    private class Analyzer : ForwardFlowAnalysis<ReachingDefinitionsTuple> {
      private static readonly ISet<ReachingDefinitionsTuple> _emptySet = ImmutableHashSet<ReachingDefinitionsTuple>.Empty;

      private NodeDataMap _kill;
      private NodeDataMap _gen;
      private IDictionary<string, ISet<ReachingDefinitionsTuple>> _generatedTuples;

      protected override ISet<ReachingDefinitionsTuple> ExtremalValues => _emptySet;
      protected override ISet<ReachingDefinitionsTuple> LeastValues => _emptySet;

      public Analyzer(ControlFlowGraph cfg) : base(cfg) { }

      protected override bool IncreasesAnalysisKnowledge(ISet<ReachingDefinitionsTuple> transferred, ISet<ReachingDefinitionsTuple> currentKnowledge) {
        return !transferred.IsSubsetOf(currentKnowledge);
      }

      protected override ISet<ReachingDefinitionsTuple> Merge(ISet<ReachingDefinitionsTuple> transferred, ISet<ReachingDefinitionsTuple> currentKnowledge) {
        return transferred.Union(currentKnowledge).ToImmutableHashSet();
      }

      protected override ISet<ReachingDefinitionsTuple> Transfer(FlowNode node, ISet<ReachingDefinitionsTuple> data) {
        return data.Except(_kill[node]).Union(_gen[node]).ToImmutableHashSet();
      }

      public void Initialize() {
        _InitializeGen();
        _InitializeGeneratedTuples();
        _InitializeKill();
      }

      private void _InitializeGen() {
        _gen = Nodes.ToImmutableDictionary(node => node, node => _CollectGenForNode(node));
      }

      private ISet<ReachingDefinitionsTuple> _CollectGenForNode(FlowNode node) {
        var variable = (node.Instruction as Assignment)?.Left as VariableExpression;
        if(variable == null) {
          return _emptySet;
        }

        return ImmutableHashSet.Create(new ReachingDefinitionsTuple(variable.Name, node));
      }

      private void _InitializeGeneratedTuples() {
        _generatedTuples = _gen.Values.SelectMany(tuples => tuples)
          .GroupBy(tuple => tuple.Variable)
          .ToImmutableDictionary(
            entry => entry.Key,
            entry => (ISet<ReachingDefinitionsTuple>)entry.ToImmutableHashSet()
          );
      }

      private void _InitializeKill() {
        _kill = Nodes.ToImmutableDictionary(
            node => node,
            node => (ISet<ReachingDefinitionsTuple>)_GetGeneratedVariables(node).SelectMany(killedVariable => _generatedTuples[killedVariable]).ToImmutableHashSet()
          );
      }

      private IEnumerable<string> _GetGeneratedVariables(FlowNode node) {
        return _gen[node].Select(tuple => tuple.Variable).Distinct();
      }
    }
  }
}
