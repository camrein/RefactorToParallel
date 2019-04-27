namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Identifies the kind of flow node.
  /// </summary>
  public enum FlowKind {
    /// <summary>
    /// A regular node within the control flow.
    /// </summary>
    Regular,

    /// <summary>
    /// The starting node of the control flow.
    /// </summary>
    Start,

    /// <summary>
    /// The end node of the control flow.
    /// </summary>
    End
  }
}
