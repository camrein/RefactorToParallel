using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for divisions.
  /// </summary>
  public class DivideTransferRule : TransferRule<DivideExpression> {
    private static readonly ISet<DescriptorKind> _emptySet = ImmutableHashSet<DescriptorKind>.Empty;
    private static readonly ISet<DescriptorKind> _loopIndependentSet = ImmutableHashSet.Create<DescriptorKind>(LoopIndependent.Instance);

    protected override ISet<DescriptorKind> Transfer(DivideExpression expression, RuleEngine engine) {
      var left = engine.Visit(expression.Left);
      var right = engine.Visit(expression.Right);

      if(right.Contains(One.Instance) || left.Contains(Zero.Instance)) {
        return left;
      }

      if(AllContainRule(LoopIndependent.Instance, left, right)) {
        return _loopIndependentSet;
      }

      return _emptySet;
    }
  }
}
