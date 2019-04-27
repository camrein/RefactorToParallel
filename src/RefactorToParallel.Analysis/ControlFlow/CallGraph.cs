using System.Collections.Generic;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Represents a call graph.
  /// </summary>
  public class CallGraph {
    /// <summary>
    /// Gets the edges within the control flow graph.
    /// </summary>
    public ISet<FlowEdge> Edges { get; } = new HashSet<FlowEdge>();

    /// <summary>
    /// Gets the basic blocks within the control flow graph.
    /// </summary>
    public ISet<FlowNode> Nodes { get; } = new HashSet<FlowNode>();

    /// <summary>
    /// Gets the methods invoked within this call graph.
    /// </summary>
    public ISet<string> Methods { get; } = new HashSet<string>();

    /// <summary>
    /// Creates a new call graph.
    /// </summary>
    internal CallGraph() { }

    /// <summary>
    /// Registers a syntax node as the successor of the given predecessor.
    /// </summary>
    /// <param name="from">The source node of the edge.</param>
    /// <param name="to">The target node of the edge.</param>
    internal void AddEdge(FlowNode from, FlowNode to) {
      if(from is FlowBoundary fromBoundary) {
        Methods.Add(fromBoundary.MethodName);
      }

      if(to is FlowBoundary toBoundary) {
        Methods.Add(toBoundary.MethodName);
      }

      Nodes.Add(from);
      Nodes.Add(to);
      Edges.Add(new FlowEdge(from, to));
    }
  }
}
