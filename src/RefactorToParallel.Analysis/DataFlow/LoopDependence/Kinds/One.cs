namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies that the value of the variable is one.
  /// </summary>
  public class One : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static One Instance { get; } = new One();

    private One() { }

    public override string ToString() {
      return " = 1";
    }
  }
}
