using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence {
  /// <summary>
  /// LINQ filters that allow to filter variable descriptors by a specific kind.
  /// </summary>
  public static class DescriptorFilters {
    /// <summary>
    /// Filters a sequence of variable descriptors so that all kinds except the given are in the resulting sequence.
    /// </summary>
    /// <typeparam name="TKind">The type of the kind that should be removed from the sequence.</typeparam>
    /// <param name="source">The source sequence where the given kind should be stripped of.</param>
    /// <returns>A sequence of variable descriptors without the specified kind.</returns>
    public static IEnumerable<VariableDescriptor> WithoutKind<TKind>(this IEnumerable<VariableDescriptor> source) {
      return source.Where(IsNotOfKind<TKind>);
    }

    /// <summary>
    /// Filters a sequence of descriptor kinds so that all kinds except the given are in the resulting sequence.
    /// </summary>
    /// <typeparam name="TKind">The type of the kind that should be removed from the sequence.</typeparam>
    /// <param name="source">The source sequence where the given kind should be stripped of.</param>
    /// <returns>A sequence of descriptor kinds without the specified kind.</returns>
    public static IEnumerable<DescriptorKind> WithoutKind<TKind>(this IEnumerable<DescriptorKind> source) {
      return source.Where(kind => !(kind is TKind));
    }

    /// <summary>
    /// Filters a sequence of variable descriptors so that only the specified kind is within the resulting sequence.
    /// </summary>
    /// <typeparam name="TKind">The type of the kind that should be preserved within the sequence.</typeparam>
    /// <param name="source">The source sequence where the filter should be applied.</param>
    /// <returns>A sequence of variable descriptors where only the specified kind is kept.</returns>
    public static IEnumerable<VariableDescriptor> OnlyWithKind<TKind>(this IEnumerable<VariableDescriptor> source) {
      return source.Where(IsOfKind<TKind>);
    }

    /// <summary>
    /// Predicate that identifies if the given variable descriptor is of the specified kind.
    /// </summary>
    /// <typeparam name="TKind">The kind to check.</typeparam>
    /// <param name="descriptor">The variable descriptor to check.</param>
    /// <returns><code>True</code> if the given descriptor is of the specified kind.</returns>
    public static bool IsOfKind<TKind>(this VariableDescriptor descriptor) {
      return descriptor.Kind is TKind;
    }

    /// <summary>
    /// Predicate that identifies if the given variable descriptor is not of the specified kind.
    /// </summary>
    /// <typeparam name="TKind">The kind to check.</typeparam>
    /// <param name="descriptor">The variable descriptor to check.</param>
    /// <returns><code>True</code> if the given descriptor is not of the specified kind.</returns>
    public static bool IsNotOfKind<TKind>(this VariableDescriptor descriptor) {
      return !IsOfKind<TKind>(descriptor);
    }
  }
}
