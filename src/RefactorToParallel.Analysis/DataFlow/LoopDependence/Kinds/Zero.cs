namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies that the value of the variable is zero.
  /// </summary>
  public class Zero : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static Zero Instance { get; } = new Zero();

    private Zero() { }

    public override string ToString() {
      return " = 0";
    }
  }
}
