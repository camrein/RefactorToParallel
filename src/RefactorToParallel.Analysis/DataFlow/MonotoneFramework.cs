using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow {
  /// <summary>
  /// The monotone framework for data flow analysis.
  /// </summary>
  /// <typeparam name="TData">The type of the data produced by the analysis.</typeparam>
  public abstract class MonotoneFramework<TData> {
    /// <summary>
    /// Gets the flow to be analyzed.
    /// </summary>
    protected abstract IReadOnlyList<FlowTransition> Flow { get; }

    /// <summary>
    /// Gets the extremal nodes of the analysis.
    /// </summary>
    protected abstract ISet<FlowNode> ExtremalNodes { get; }

    /// <summary>
    /// Gets the extremal values of the analysis.
    /// </summary>
    protected abstract ISet<TData> ExtremalValues { get; }

    /// <summary>
    /// Gets the least values of the analysis.
    /// </summary>
    protected abstract ISet<TData> LeastValues { get; }

    /// <summary>
    /// Gets the nodes of the analysis.
    /// </summary>
    protected ISet<FlowNode> Nodes { get; }

    /// <summary>
    /// Creates a new instance of the monotone framework with the given nodes.
    /// </summary>
    /// <param name="nodes">The nodes of the analysis.</param>
    protected MonotoneFramework(IEnumerable<FlowNode> nodes) {
      Nodes = nodes.ToImmutableHashSet();
    }

    /// <summary>
    /// Checks if the current analysis with the applied transfer function increases the knowledge.
    /// </summary>
    /// <param name="transferred">The data with the applied transfer function.</param>
    /// <param name="currentKnowledge">The current knowledge.</param>
    /// <returns><code>True</code> if the transferred data improves the data of the successor.</returns>
    protected abstract bool IncreasesAnalysisKnowledge(ISet<TData> transferred, ISet<TData> currentKnowledge);

    /// <summary>
    /// Merges the data with the applied transfer function with the current konwledge.
    /// </summary>
    /// <param name="transferred">The data with the applied transfer function.</param>
    /// <param name="currentKnowledge">The current knowledge.</param>
    /// <returns>The updated knowledge.</returns>
    protected abstract ISet<TData> Merge(ISet<TData> transferred, ISet<TData> currentKnowledge);

    /// <summary>
    /// Applies the transfer function for the given node with the given data.
    /// </summary>
    /// <param name="node">The node to apply the transfer function on.</param>
    /// <param name="data">The data to transfer.</param>
    /// <returns>The transferred data.</returns>
    protected abstract ISet<TData> Transfer(FlowNode node, ISet<TData> data);

    /// <summary>
    /// Applies the transfer function for the given node with the given data in an intra-procedural manner.
    /// </summary>
    /// <param name="node">The node to apply the transfer function on.</param>
    /// <param name="data">The data to transfer.</param>
    /// <returns>A tuple containing the transferred data and the procedure that should be processed or <code>null</code> if none.</returns>
    protected virtual ISet<TData> TransferInterprocedural(FlowNode node, ISet<TData> data) {
      return Transfer(node, data);
    }

    /// <summary>
    /// Solves the instance of the monotone framework.
    /// </summary>
    /// <returns>
    /// A tuple whereas <code>MfpIn</code> are the nodes without and <code>MfpOut</code> are the nodes
    /// with the application of the transfer function.
    /// </returns>
    internal (IDictionary<FlowNode, ISet<TData>> MfpIn, IDictionary<FlowNode, ISet<TData>> MfpOut) Solve() {
      return _Solve(Transfer);
    }

    internal (IDictionary<FlowNode, ISet<TData>> MfpIn, IDictionary<FlowNode, ISet<TData>> MfpOut) SolveInterprocedural() {
      return _Solve(TransferInterprocedural);
    }
    
    internal virtual (IDictionary<FlowNode, ISet<TData>> MfpIn, IDictionary<FlowNode, ISet<TData>> MfpOut) _Solve(Func<FlowNode, ISet<TData>, ISet<TData>> transferFunction) {
      var worklist = new Queue<FlowTransition>(Flow);
      var analysis = Nodes.ToDictionary(node => node, node => ExtremalNodes.Contains(node) ? ExtremalValues : LeastValues);

      while(worklist.Count > 0) {
        var transition = worklist.Dequeue();
        var transferred = transferFunction(transition.First, analysis[transition.First]);

        if(!IncreasesAnalysisKnowledge(transferred, analysis[transition.Second])) {
          continue;
        }

        analysis[transition.Second] = Merge(transferred, analysis[transition.Second]);
        //worklist.EnqueueAll(Flow.Where(flow => flow.First.Equals(transition.Second)));
        worklist.EnqueueAll(Flow.Where(flow => flow.First == transition.Second));
      }

      return (analysis, analysis.ToDictionary(entry => entry.Key, entry => Transfer(entry.Key, entry.Value)));
    }
  }
}
