using RefactorToParallel.Analysis.ControlFlow;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow {
  /// <summary>
  /// Describes a backward analysis as an instance of the monotone framework.
  /// </summary>
  /// <typeparam name="TData">The type of the data produced by the analysis.</typeparam>
  public abstract class BackwardFlowAnalysis<TData> : MonotoneFramework<TData> {
    /// <summary>
    /// Gets the forward flow of the analysis.
    /// </summary>
    protected override IReadOnlyList<FlowTransition> Flow { get; }

    /// <summary>
    /// Gets the extremal nodes of the analysis.
    /// </summary>
    protected override ISet<FlowNode> ExtremalNodes { get; }

    /// <summary>
    /// Creates a new backward analysis for the given control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to create the backward analysis of.</param>
    protected BackwardFlowAnalysis(ControlFlowGraph cfg) : base(cfg.Nodes) {
      Flow = cfg.Edges.Reverse().Select(edge => new FlowTransition(edge.To, edge.From)).ToImmutableList();
      ExtremalNodes = ImmutableHashSet.Create<FlowNode>(cfg.End);
    }
  }
}
