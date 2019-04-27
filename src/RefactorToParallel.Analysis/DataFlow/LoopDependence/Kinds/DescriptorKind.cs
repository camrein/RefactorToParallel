namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds {
  /// <summary>
  /// Describes the kind of the variable descriptor. This is mainly used to generate hashcodes that are
  /// distinct between the types. Furthermore, the number of GetHashCode() and Equals() implementations can be reduced.
  /// </summary>
  public abstract class DescriptorKind {
  }
}
