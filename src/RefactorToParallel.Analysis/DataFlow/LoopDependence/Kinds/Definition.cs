using RefactorToParallel.Analysis.ControlFlow;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies where the actual value was defined.
  /// </summary>
  public class Definition : DescriptorKind {
    /// <summary>
    /// Gets the name of the sibling variable.
    /// </summary>
    public FlowNode Node { get; }

    /// <summary>
    /// Creaes a new definition descriptor with the given defining node.
    /// </summary>
    /// <param name="node">The node that defines the current value.</param>
    public Definition(FlowNode node) {
      Node = node;
    }

    public override bool Equals(object obj) {
      return Equals(Node, (obj as Definition)?.Node);
    }

    public override int GetHashCode() {
      return Node.GetHashCode();
    }

    public override string ToString() {
      return "@";
    }
  }
}
