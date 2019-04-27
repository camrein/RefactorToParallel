using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Core.Util;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Tuple holding the resolved alias information.
  /// </summary>
  public class VariableAlias {
    private readonly int _hashCode;

    /// <summary>
    /// Gets the source symbol.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the targetted object.
    /// </summary>
    public Expression Target { get; }

    /// <summary>
    /// Creates a new instance with the given source and target.
    /// </summary>
    /// <param name="source">The source variable name.</param>
    /// <param name="target">The target expression.</param>
    public VariableAlias(string source, Expression target) {
      Source = source;
      Target = target;
      _hashCode = Hash.With(source).And(target).Get();
    }

    public override bool Equals(object obj) {
      var other = obj as VariableAlias;
      return other != null && Equals(Source, other.Source) && Equals(Target, other.Target);
    }

    public override int GetHashCode() {
      return _hashCode;
    }

    public override string ToString() {
      return $"{Source} => {Target}";
    }
  }
}
