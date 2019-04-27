using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence {
  /// <summary>
  /// This class is used to apply the transfer function of a single node within the control flow graph.
  /// </summary>
  internal class NodeTransfer : IInstructionVisitor<ISet<VariableDescriptor>> {
    private readonly ISet<VariableDescriptor> _inputKnowledge;
    private readonly FlowNode _flowNode;
    private readonly RuleEngine _ruleEngine;

    /// <summary>
    /// Creates a new instance with the given input knowledge and the node where the transfer is applied to.
    /// </summary>
    /// <param name="inputKnowledge">The input knowledge at the current node.</param>
    /// <param name="node">The node where the transfer is applied to.</param>
    /// <param name="interprocedural"><code>True</code> if this is an interprocedural analysis.</param>
    internal NodeTransfer(ISet<VariableDescriptor> inputKnowledge, FlowNode node, bool interprocedural) {
      _inputKnowledge = inputKnowledge;
      _flowNode = node;
      _ruleEngine = new RuleEngine(inputKnowledge, interprocedural);
    }

    /// <summary>
    /// Applies the transfer.
    /// </summary>
    /// <returns>The resulting knowledge after applying the transfer.</returns>
    internal ISet<VariableDescriptor> Apply() {
      return _flowNode.Instruction.Accept(this);
    }

    public ISet<VariableDescriptor> Visit(Instruction instruction) {
      return instruction.Accept(this);
    }

    public ISet<VariableDescriptor> Visit(Assignment instruction) {
      if(instruction.Left is VariableExpression variable) {
        return _ApplyWriteAccess(variable.Name, instruction.Right);
      }

      return _inputKnowledge;
    }

    public ISet<VariableDescriptor> Visit(Declaration instruction) {
      return _ApplyWriteAccess(instruction.Name, null);
    }

    private ISet<VariableDescriptor> _ApplyWriteAccess(string variableName, Expression expression) {
      var cleaned = _inputKnowledge.Where(descriptor => !descriptor.Name.Equals(variableName));
      
      var written = new HashSet<VariableDescriptor>(cleaned) {
        new VariableDescriptor(variableName, new Definition(_flowNode))
      };

      if(expression != null) {
        written.UnionWith(_ruleEngine.Visit(expression).Select(kind => new VariableDescriptor(variableName, kind)));
      }

      return written;
    }

    public ISet<VariableDescriptor> Visit(Label instruction) {
      return _inputKnowledge;
    }

    public ISet<VariableDescriptor> Visit(Jump instruction) {
      return _inputKnowledge;
    }

    public ISet<VariableDescriptor> Visit(ConditionalJump instruction) {
      return _inputKnowledge;
    }

    public ISet<VariableDescriptor> Visit(Invocation instruction) {
      return _inputKnowledge;
    }
  }
}
