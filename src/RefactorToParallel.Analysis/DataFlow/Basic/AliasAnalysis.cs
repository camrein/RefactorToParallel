using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NodeDataMap = System.Collections.Generic.IDictionary<RefactorToParallel.Analysis.ControlFlow.FlowNode, System.Collections.Generic.ISet<RefactorToParallel.Analysis.DataFlow.Basic.VariableAlias>>;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Alias analysis
  /// </summary>
  public class AliasAnalysis {
    /// <summary>
    /// Gets the states at the entry of a node.
    /// </summary>
    public NodeDataMap Entry { get; }

    /// <summary>
    /// Gets the states at the exit of a node.
    /// </summary>
    public NodeDataMap Exit { get; }

    private AliasAnalysis(NodeDataMap entry, NodeDataMap exit) {
      Entry = entry;
      Exit = exit;
    }

    /// <summary>
    /// Applies the alias analysis on the desired control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to apply the analysis on.</param>
    /// <param name="input">The aliases to provide as an input knowledge.</param>
    /// <returns>The result of the alias analysis.</returns>
    public static AliasAnalysis Analyze(ControlFlowGraph cfg, params VariableAlias[] input) {
      return Analyze(cfg, input.AsEnumerable());
    }

    /// <summary>
    /// Applies the alias analysis on the desired control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to apply the analysis on.</param>
    /// <param name="input">The aliases to provide as an input knowledge.</param>
    /// <returns>The result of the alias analysis.</returns>
    public static AliasAnalysis Analyze(ControlFlowGraph cfg, IEnumerable<VariableAlias> input) {
      var instance = new Analyzer(cfg, input);
      var result = instance.Solve();
      return new AliasAnalysis(result.MfpIn, result.MfpOut);
    }

    /// <summary>
    /// Applies the alias analysis on the desired control flow graph.
    /// </summary>
    /// <param name="rootCfg">The root control flow graph to create the analysis of.</param>
    /// <param name="callGraph">The graph identifying the calls of methods for interprocedural analysis.</param>
    /// <param name="procedures">The control flow graphs of the invoked methods.</param>
    /// <param name="input">The aliases to provide as an input knowledge.</param>
    /// <returns>The result of the alias analysis.</returns>
    public static AliasAnalysis Analyze(ControlFlowGraph rootCfg, CallGraph callGraph, IEnumerable<ControlFlowGraph> procedures, IEnumerable<VariableAlias> input) {
      var instance = new Analyzer(rootCfg, callGraph, procedures, input);
      var result = instance.SolveInterprocedural();
      return new AliasAnalysis(result.MfpIn, result.MfpOut);
    }

    /// <summary>
    /// Gets the alias information of a variable at the entry of the given node.
    /// </summary>
    /// <param name="variableName">The name of the variable to get the alias information of.</param>
    /// <param name="node">The node where to get the alias information of.</param>
    /// <returns>The alias information.</returns>
    public IEnumerable<VariableAlias> GetAliasesOfVariableBefore(string variableName, FlowNode node) {
      return _GetAliases(Entry[node], variableName);
    }

    /// <summary>
    /// Gets the alias information of a variable at the exit of the given node.
    /// </summary>
    /// <param name="variableName">The name of the variable to get the alias information of.</param>
    /// <param name="node">The node where to get the alias information of.</param>
    /// <returns>The alias information.</returns>
    public IEnumerable<VariableAlias> GetAliasesOfVariableAfter(string variableName, FlowNode node) {
      return _GetAliases(Exit[node], variableName);
    }

    private IEnumerable<VariableAlias> _GetAliases(IEnumerable<VariableAlias> data, string variableName) {
      return data.Where(alias => alias.Source.Equals(variableName));
    }

    private class Analyzer : ForwardFlowAnalysis<VariableAlias> {
      private static readonly ISet<VariableAlias> _emptySet = ImmutableHashSet<VariableAlias>.Empty;

      protected override ISet<VariableAlias> ExtremalValues { get; }
      protected override ISet<VariableAlias> LeastValues => _emptySet;

      public Analyzer(ControlFlowGraph cfg, IEnumerable<VariableAlias> input) : base(cfg) {
        ExtremalValues = input.ToImmutableHashSet();
      }

      public Analyzer(ControlFlowGraph rootCfg, CallGraph callGraph, IEnumerable<ControlFlowGraph> procedures, IEnumerable<VariableAlias> input) : base(rootCfg, callGraph, procedures) {
        ExtremalValues = input.ToImmutableHashSet();
      }

      protected override bool IncreasesAnalysisKnowledge(ISet<VariableAlias> transferred, ISet<VariableAlias> currentKnowledge) {
        return !transferred.IsSubsetOf(currentKnowledge);
      }

      protected override ISet<VariableAlias> Merge(ISet<VariableAlias> transferred, ISet<VariableAlias> currentKnowledge) {
        return transferred.Concat(currentKnowledge).ToImmutableHashSet();
      }

      protected override ISet<VariableAlias> Transfer(FlowNode node, ISet<VariableAlias> data) {
        return _TransferRegular(node, data, false);
      }

      protected override ISet<VariableAlias> TransferInterprocedural(FlowNode node, ISet<VariableAlias> data) {
        // Important to remember: the node passed here is always a predecessor (First) of the node that receives the transferred information (Second).
        ISet<VariableAlias> transferred = null;
        if(node is FlowTransfer transfer) {
          // TODO extract this into separate instructions by the monotone framework?
          if(transfer.TransfersTo) {
            // TODO extremal nodes & values per method
            transferred = data.Where(alias => InvocationExpression.IsParameterIdentifierOf(transfer.TargetMethod, alias.Source)).ToImmutableHashSet();
          } else {
            transferred = data.Where(alias => InvocationExpression.IsResultIdentifierOf(transfer.SourceMethod, alias.Source)).ToImmutableHashSet();
          }
        }

        if(transferred == null) {
          transferred = _TransferRegular(node, data, true);
        }

        return transferred;
      }

      private ISet<VariableAlias> _TransferRegular(FlowNode node, ISet<VariableAlias> data, bool interprocedural) {
        var visitor = new InstructionVisitor(data, interprocedural);
        visitor.Visit(node.Instruction);
        return visitor.State;
      }
    }

    private class InstructionVisitor : IInstructionVisitor {
      private readonly bool _interprocedural;

      public ISet<VariableAlias> State { get; }

      public InstructionVisitor(IEnumerable<VariableAlias> state, bool interprocedural) {
        State = new HashSet<VariableAlias>(state);
        _interprocedural = interprocedural;
      }

      private void _UpdateAliases(string variableName, Expression value) {
        State.ExceptWith(State.Where(alias => alias.Source.Equals(variableName)).ToImmutableHashSet());

        string aliasedVariableName = null;
        switch(value) {
        case VariableExpression variable:
          aliasedVariableName = variable.Name;
          break;
        case InvocationExpression invocation:
          if(_interprocedural) {
            // As the analysis is rather imprecise in terms of method invocations, any available
            // instance of the result identifier contains exactly the same information.
            aliasedVariableName = InvocationExpression.GetResultIdentifier(invocation.Name);
          }
          break;
        }

        if(aliasedVariableName == null) {
          State.Add(new VariableAlias(variableName, value));
          return;
        }

        var aliases = State.Where(alias => alias.Source.Equals(aliasedVariableName))
          .Select(alias => new VariableAlias(variableName, alias.Target))
          .ToImmutableHashSet();

        State.UnionWith(aliases);
      }

      public void Visit(Instruction node) {
        node.Accept(this);
      }

      public void Visit(Assignment node) {
        if(node.Left is VariableExpression variable) {
          _UpdateAliases(variable.Name, node.Right);
        }
      }

      public void Visit(Declaration node) { }

      public void Visit(Label node) { }

      public void Visit(Jump node) { }

      public void Visit(ConditionalJump node) { }

      public void Visit(Invocation instruction) { }
    }
  }
}
