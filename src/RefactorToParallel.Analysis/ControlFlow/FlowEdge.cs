namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Represents an edge between two nodes within the control flow graph.
  /// </summary>
  public class FlowEdge {
    /// <summary>
    /// Gets the source node.
    /// </summary>
    public FlowNode From { get; }

    /// <summary>
    /// Gets the target node.
    /// </summary>
    public FlowNode To { get; }

    /// <summary>
    /// Creates a new flow edge with the given nodes.
    /// </summary>
    /// <param name="from">The source node.</param>
    /// <param name="to">The target node.</param>
    public FlowEdge(FlowNode from, FlowNode to) {
      From = from;
      To = to;
    }

    public override string ToString() {
      return $"{From} => {To}";
    }
  }
}
