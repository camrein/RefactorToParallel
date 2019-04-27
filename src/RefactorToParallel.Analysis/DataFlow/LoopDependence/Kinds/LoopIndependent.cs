namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies that the variable is loop independent.
  /// </summary>
  public class LoopIndependent : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static LoopIndependent Instance { get; } = new LoopIndependent();

    private LoopIndependent() { }

    public override string ToString() {
      return "/";
    }
  }
}
