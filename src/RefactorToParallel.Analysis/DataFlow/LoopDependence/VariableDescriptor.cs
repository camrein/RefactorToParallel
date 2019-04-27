using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Core.Util;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence {
  /// <summary>
  /// A variable descriptor attaches information about a variable to said variable.
  /// </summary>
  public class VariableDescriptor {
    private readonly int _hashCode;

    /// <summary>
    /// Gets the name of the variable the information was attached to.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the kind of information expressed by this descriptor.
    /// </summary>
    public DescriptorKind Kind { get; }

    /// <summary>
    /// Creates a new instance with the given variable name.
    /// </summary>
    /// <param name="name">The name to attach this property to.</param>
    /// <param name="kind">The kind of the property to attach.</param>
    public VariableDescriptor(string name, DescriptorKind kind) {
      Name = name;
      Kind = kind;
      _hashCode = Hash.With(name).And(kind).Get();
    }

    public override bool Equals(object obj) {
      var other = obj as VariableDescriptor;
      return other != null && Equals(Kind, other.Kind) && Equals(Name, other.Name);
    }

    public override int GetHashCode() {
      return _hashCode;
    }

    public override string ToString() {
      return $"{Name}{Kind}";
    }
  }
}
