using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using RefactorToParallel.Core.Extensions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Control flow graph generated out of a given syntax tree.
  /// </summary>
  public class ControlFlowGraph {
    /// <summary>
    /// Gets the start node of the control flow graph.
    /// </summary>
    public FlowBoundary Start { get; }

    /// <summary>
    /// Gets the end node of the control flow graph.
    /// </summary>
    public FlowBoundary End { get; }

    /// <summary>
    /// Gets the edges within the control flow graph.
    /// </summary>
    public ISet<FlowEdge> Edges { get; } = new HashSet<FlowEdge>();

    /// <summary>
    /// Gets the basic blocks within the control flow graph.
    /// </summary>
    public ISet<FlowNode> Nodes { get; }

    /// <summary>
    /// Gets the mapping dictionary containing a mapping from an instruction to its node in the graph.
    /// </summary>
    public IDictionary<Instruction, FlowNode> Mappings { get; } = new Dictionary<Instruction, FlowNode>();

    /// <summary>
    /// Creates a new control flow graph.
    /// </summary>
    internal ControlFlowGraph() : this(null) { }

    /// <summary>
    /// Creates a new control flow graph with the given method name.
    /// </summary>
    /// <param name="methodName">The name of the method this control flow graph belongs to.</param>
    internal ControlFlowGraph(string methodName) {
      Start = new FlowBoundary(methodName, FlowKind.Start);
      End = new FlowBoundary(methodName, FlowKind.End);
      Nodes = new HashSet<FlowNode> { Start, End };
    }

    /// <summary>
    /// Registers an instruction to the control flow graph.
    /// </summary>
    /// <param name="instruction">The instruction to register.</param>
    /// <returns>The node that was mapped to the given instruction.</returns>
    internal FlowNode RegisterInstruction(Instruction instruction) {
      var node = Mappings.GetOrCreate(instruction, () => new FlowNode(instruction));
      Nodes.Add(node);
      return node;
    }

    /// <summary>
    /// Registers an invocation to the control flow graph.
    /// </summary>
    /// <param name="expression">The invocation expression.</param>
    /// <returns>The node that was mapped to the given invocation.</returns>
    internal FlowNode RegisterInvocation(InvocationExpression expression) {
      var node = new FlowInvocation(expression);
      Nodes.Add(node);
      return node;
    }

    /// <summary>
    /// Registers a syntax node as the successor of the given predecessor.
    /// </summary>
    /// <param name="from">The source node of the edge.</param>
    /// <param name="to">The target node of the edge.</param>
    internal void AddEdge(FlowNode from, FlowNode to) {
      Edges.Add(new FlowEdge(from, to));

      from.Successors.Add(to);
      to.Predecessors.Add(from);
    }
  }
}
