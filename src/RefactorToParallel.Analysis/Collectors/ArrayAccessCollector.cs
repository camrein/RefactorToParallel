using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.Collectors {
  /// <summary>
  /// Collects all array accesses within a given control flow graph.
  /// </summary>
  public class ArrayAccessCollector : IInstructionVisitor {
    private readonly ISet<ArrayAccess> _accesses = new HashSet<ArrayAccess>();

    private FlowNode _node;

    /// <summary>
    /// Collects all array accesses within the given control flow graph.
    /// </summary>
    /// <param name="controlFlowGraph">The control flow graph to collect the accesses from.</param>
    /// <returns>The array accesses within the control flow graph.</returns>
    public static ISet<ArrayAccess> Collect(ControlFlowGraph controlFlowGraph) {
      return Collect(controlFlowGraph.Nodes);
    }

    /// <summary>
    /// Collects all array accesses within the given control flow graph nodes.
    /// </summary>
    /// <param name="nodes">The the nodes to collect the array accesses of.</param>
    /// <returns>The array accesses within the nodes.</returns>
    public static ISet<ArrayAccess> Collect(IEnumerable<FlowNode> nodes) {
      var collector = new ArrayAccessCollector();
      foreach(var node in nodes) {
        collector._Visit(node);
      }
      return collector._accesses;
    }

    private void _Visit(FlowNode node) {
      _node = node;
      Visit(node.Instruction);
    }

    public void Visit(Instruction instruction) {
      instruction.Accept(this);
    }

    public void Visit(Assignment instruction) {
      if(instruction.Left is ArrayExpression array) {
        _accesses.Add(new ArrayAccess(_node, array, true));
        _CollectReadAccesses(array.Descendants());
      }

      _CollectReadAccesses(instruction.Right);
    }

    public void Visit(Declaration instruction) { }

    public void Visit(Label instruction) { }

    public void Visit(Jump instruction) { }

    public void Visit(ConditionalJump instruction) {
      _CollectReadAccesses(instruction.Condition);
    }

    public void Visit(Invocation instruction) {
      _CollectReadAccesses(instruction.Expression);
    }

    private void _CollectReadAccesses(Expression expression) {
      _CollectReadAccesses(expression.DescendantsAndSelf());
    }

    private void _CollectReadAccesses(IEnumerable<Expression> expressions) {
      _accesses.UnionWith(expressions.OfType<ArrayExpression>().Select(array => new ArrayAccess(_node, array, false)));
    }
  }
}
