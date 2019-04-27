namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  // TODO rename to IterationDependant?
  /// <summary>
  /// This variable descriptor identifies that the variable is loop dependent.
  /// </summary>
  public class LoopDependent : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static LoopDependent Instance { get; } = new LoopDependent();

    private LoopDependent() { }

    public override string ToString() {
      return "|";
    }
  }
}
