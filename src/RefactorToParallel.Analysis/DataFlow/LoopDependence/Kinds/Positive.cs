namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies that the value of the variable is positive.
  /// </summary>
  public class Positive : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static Positive Instance { get; } = new Positive();

    private Positive() { }

    public override string ToString() {
      return "+";
    }
  }
}
