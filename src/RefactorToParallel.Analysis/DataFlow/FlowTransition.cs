using RefactorToParallel.Analysis.ControlFlow;

namespace RefactorToParallel.Analysis.DataFlow {
  /// <summary>
  /// Describes a data flow transition between two nodes.
  /// </summary>
  public class FlowTransition {
    /// <summary>
    /// Gets the node coming first in the flow.
    /// </summary>
    internal FlowNode First { get; }

    /// <summary>
    /// Gets the node coming second in the flow.
    /// </summary>
    internal FlowNode Second { get; }

    /// <summary>
    /// Creates a new flow transition between two nodes.
    /// </summary>
    /// <param name="first">The node coming first.</param>
    /// <param name="second">The node coming second.</param>
    internal FlowTransition(FlowNode first, FlowNode second) {
      First = first;
      Second = second;
    }
  }
}
