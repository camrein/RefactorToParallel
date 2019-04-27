using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodeDataMap = System.Collections.Generic.IDictionary<RefactorToParallel.Analysis.ControlFlow.FlowNode, System.Collections.Generic.ISet<RefactorToParallel.Analysis.DataFlow.LoopDependence.VariableDescriptor>>;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence {
  /// <summary>
  /// Analysis implementation that estimates the dependence of variables on the current loop iteration.
  /// </summary>
  public class LoopDependenceAnalysis {
    /// <summary>
    /// Gets the collected information at the entry of the nodes.
    /// </summary>
    public NodeDataMap Entry { get; }

    /// <summary>
    /// Gets the collected information at the exit of the nodes.
    /// </summary>
    public NodeDataMap Exit { get; }

    private LoopDependenceAnalysis(NodeDataMap entry, NodeDataMap exit) {
      Entry = entry;
      Exit = exit;
    }

    /// <summary>
    /// Applies the analysis on the given control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph where the analysis should be applied.</param>
    /// <param name="loopIndex">The variable representing the loop index. <code>null</code> if no loop index is available.</param>
    /// <returns>The collected loop dependence information.</returns>
    public static LoopDependenceAnalysis Analyze(ControlFlowGraph cfg, string loopIndex) {
      var analysis = new Analyzer(cfg, loopIndex);
      var result = analysis.Solve();
      return new LoopDependenceAnalysis(result.MfpIn, result.MfpOut);
    }

    /// <summary>
    /// Applies the analysis on the given control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph where the analysis should be applied.</param>
    /// <param name="loopIndex">The variable representing the loop index.</param>
    /// <param name="callGraph">The graph identifying the calls of methods for interprocedural analysis.</param>
    /// <param name="procedures">The control flow graphs of the invoked methods.</param>
    /// <returns>The collected loop dependence information.</returns>
    public static LoopDependenceAnalysis Analyze(ControlFlowGraph cfg, string loopIndex, CallGraph callGraph, IEnumerable<ControlFlowGraph> procedures) {
      var analysis = new Analyzer(cfg, loopIndex, callGraph,  procedures);
      var result = analysis.SolveInterprocedural();
      return new LoopDependenceAnalysis(result.MfpIn, result.MfpOut);
    }

    private class Analyzer : ForwardFlowAnalysis<VariableDescriptor> {
      private static readonly ISet<VariableDescriptor> _emptySet = ImmutableHashSet<VariableDescriptor>.Empty;

      protected override ISet<VariableDescriptor> ExtremalValues { get; }

      protected override ISet<VariableDescriptor> LeastValues => _emptySet;

      public Analyzer(ControlFlowGraph cfg, string loopIndex) : base(cfg) {
        ExtremalValues = _CreateExtremalValues(cfg, loopIndex);
      }

      public Analyzer(ControlFlowGraph cfg, string loopIndex, CallGraph callGraph, IEnumerable<ControlFlowGraph> procedures) : base(cfg, callGraph, procedures) {
        ExtremalValues = _CreateExtremalValues(cfg, loopIndex);
      }

      private static ISet<VariableDescriptor> _CreateExtremalValues(ControlFlowGraph cfg, string loopIndex) {
        return loopIndex == null ? _emptySet : ImmutableHashSet.Create(
          new VariableDescriptor(loopIndex, LoopDependent.Instance),
          new VariableDescriptor(loopIndex, Positive.Instance),
          new VariableDescriptor(loopIndex, NotZero.Instance),
          new VariableDescriptor(loopIndex, new Definition(cfg.Start))
        );
      }

      protected override bool IncreasesAnalysisKnowledge(ISet<VariableDescriptor> transferred, ISet<VariableDescriptor> currentKnowledge) {
        return !transferred.SetEquals(currentKnowledge);
      }

      protected override ISet<VariableDescriptor> Merge(ISet<VariableDescriptor> transferred, ISet<VariableDescriptor> currentKnowledge) {
        throw new NotImplementedException("custom solver has to be used");
      }

      private ISet<string> _GetConflictingVariables(ISet<VariableDescriptor> merged) {
        return merged
          .OnlyWithKind<Definition>()
          .GroupBy(descriptor => descriptor.Name)
          .Where(entry => entry.Count() > 1)
          .Select(entry => entry.Key)
          .ToImmutableHashSet();
      }

      protected override ISet<VariableDescriptor> Transfer(FlowNode node, ISet<VariableDescriptor> data) {
        return _TransferRegular(node, data, false);
      }

      protected override ISet<VariableDescriptor> TransferInterprocedural(FlowNode node, ISet<VariableDescriptor> data) {
        ISet<VariableDescriptor> transferred = null;
        if(node is FlowTransfer transfer) {
          // TODO extract this into separate instructions by the monotone framework?
          if(transfer.TransfersTo) {
            transferred = data.Where(descriptor => InvocationExpression.IsParameterIdentifierOf(transfer.TargetMethod, descriptor.Name)).ToImmutableHashSet();
          } else {
            transferred = data.Where(descriptor => InvocationExpression.IsResultIdentifierOf(transfer.SourceMethod, descriptor.Name)).ToImmutableHashSet();
          }
        }

        if(transferred == null) {
          transferred = _TransferRegular(node, data, true);
        }

        return transferred;
      }

      private ISet<VariableDescriptor> _TransferRegular(FlowNode node, ISet<VariableDescriptor> data, bool interprocedural) {
        return new NodeTransfer(data, node, interprocedural).Apply();
      }

      private (IDictionary<FlowNode, ISet<FlowNode>> Predecessors, IDictionary<FlowNode, ISet<FlowNode>> Successors) _ConstructTransitions() {
        var predecessors = Nodes.ToDictionary(node => node, node => (ISet<FlowNode>)new HashSet<FlowNode>());
        var successors = Nodes.ToDictionary(node => node, node => (ISet<FlowNode>)new HashSet<FlowNode>());
        foreach(var transition in Flow) {
          predecessors[transition.Second].Add(transition.First);
          successors[transition.First].Add(transition.Second);
        }

        return (predecessors, successors);
      }

      private ISet<VariableDescriptor> _Merge(ISet<VariableDescriptor> transferred) {
        var conflicting = _GetConflictingVariables(transferred);
        return transferred
          .Where(descriptor => !descriptor.IsNotOfKind<Definition>() || !conflicting.Contains(descriptor.Name))
          .ToImmutableHashSet();
      }

      internal override (NodeDataMap MfpIn, NodeDataMap MfpOut) _Solve(Func<FlowNode, ISet<VariableDescriptor>, ISet<VariableDescriptor>> transferFunction) {
        // Custom solver is necessary because of the side-effects of the merge function
        var worklist = new Queue<FlowNode>(Nodes);
        var mfpIn = Nodes.ToDictionary(node => node, node => _emptySet);
        var mfpOut = Nodes.ToDictionary(node => node, node => _emptySet);
        var transitions = _ConstructTransitions();

        while(worklist.Count > 0) {
          var node = worklist.Dequeue();
          var merged = ExtremalNodes.Contains(node) ? ExtremalValues : _Merge(transitions.Predecessors[node].SelectMany(predecessor => mfpOut[predecessor]).ToImmutableHashSet());
          var transferred = transferFunction(node, merged);

          // Merging may have changed the data, thus it is always updated (not necessary to check for changes). 
          // But only for cases wherethe data after transfer changed make it necessary to notify the successors about the change.
          mfpIn[node] = merged;
          if(!IncreasesAnalysisKnowledge(transferred, mfpOut[node])) {
            continue;
          }

          mfpOut[node] = transferred;
          worklist.EnqueueAll(transitions.Successors[node]);
        }

        return (mfpIn, mfpOut);
      }
    }
  }
}
