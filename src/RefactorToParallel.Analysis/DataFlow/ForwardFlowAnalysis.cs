using RefactorToParallel.Analysis.ControlFlow;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow {
  /// <summary>
  /// Describes a forward analysis as an instance of the monotone framework.
  /// </summary>
  /// <typeparam name="TData">The type of the data produced by the analysis.</typeparam>
  public abstract class ForwardFlowAnalysis<TData> : MonotoneFramework<TData> {
    /// <summary>
    /// Gets the forward flow of the analysis.
    /// </summary>
    protected override IReadOnlyList<FlowTransition> Flow { get; }

    /// <summary>
    /// Gets the extremal nodes of the analysis.
    /// </summary>
    protected override ISet<FlowNode> ExtremalNodes { get; }

    /// <summary>
    /// Creates a new forward analysis for the given control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to create the forward analysis of.</param>
    protected ForwardFlowAnalysis(ControlFlowGraph cfg) : base(cfg.Nodes) {
      Flow = cfg.Edges.Select(edge => new FlowTransition(edge.From, edge.To)).ToImmutableList();
      ExtremalNodes = ImmutableHashSet.Create<FlowNode>(cfg.Start);
    }

    /// <summary>
    /// Creates a new forward analysis for the given control flow graph.
    /// </summary>
    /// <param name="rootCfg">The root control flow graph to create the analysis of.</param>
    /// <param name="callGraph">The graph identifying the calls of methods for interprocedural analysis.</param>
    /// <param name="procedures">The control flow graphs of the invoked methods.</param>
    protected ForwardFlowAnalysis(ControlFlowGraph rootCfg, CallGraph callGraph, IEnumerable<ControlFlowGraph> procedures)
        : base(rootCfg.Nodes.Concat(procedures.SelectMany(procedure => procedure.Nodes)).Concat(callGraph.Nodes)) {
      Flow = rootCfg.Edges
        .Concat(procedures.SelectMany(procedure => procedure.Edges))
        .Concat(callGraph.Edges)
        .Select(edge => new FlowTransition(edge.From, edge.To))
        .ToImmutableList();

      ExtremalNodes = ImmutableHashSet.Create<FlowNode>(rootCfg.Start);
    }
  }
}
