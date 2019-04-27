namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// This variable descriptor identifies that the value of the variable is not zero.
  /// </summary>
  public class NotZero : DescriptorKind {
    /// <summary>
    /// Gets an instance of this descriptor kind.
    /// </summary>
    public static NotZero Instance { get; } = new NotZero();

    private NotZero() { }

    public override string ToString() {
      return " != 0";
    }
  }
}
