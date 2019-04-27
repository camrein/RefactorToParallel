namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies that the value of the variable is negative.
  /// </summary>
  public class Negative : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static Negative Instance { get; } = new Negative();

    private Negative() { }

    public override string ToString() {
      return "-";
    }
  }
}
