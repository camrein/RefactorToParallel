using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Represents a node within the control flow graph.
  /// </summary>
  public class FlowNode {
    /// <summary>
    /// Gets the predecessors of the node.
    /// </summary>
    public ISet<FlowNode> Predecessors { get; } = new HashSet<FlowNode>();

    /// <summary>
    /// Gets the successors of the node.
    /// </summary>
    public ISet<FlowNode> Successors { get; } = new HashSet<FlowNode>();

    /// <summary>
    /// Gets the IR instruction represented by this control flow graph node.
    /// </summary>
    public Instruction Instruction { get; }

    /// <summary>
    /// Gets the kind of flow this node represents.
    /// </summary>
    public FlowKind Kind { get; }

    /// <summary>
    /// Creates a new node in the control flow graph with the given IR instruction to represent.
    /// </summary>
    /// <param name="instruction">The IR instruction to represent.</param>
    public FlowNode(Instruction instruction) : this(instruction, FlowKind.Regular) {}

    /// <summary>
    /// Creates a new node in the control flow graph with the given IR instruction to represent
    /// and the given flow kind.
    /// </summary>
    /// <param name="instruction">The IR instruction to represent.</param>
    /// <param name="kind">The kind of flow represented by this instance.</param>
    public FlowNode(Instruction instruction, FlowKind kind) {
      Instruction = instruction;
      Kind = kind;
    }

    public override string ToString() {
      return Instruction.ToString();
    }
  }
}
