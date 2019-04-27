using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for integers.
  /// </summary>
  public class IntegerTransferRule : TransferRule<IntegerExpression> {
    private static readonly ISet<DescriptorKind> _zero = ImmutableHashSet.Create<DescriptorKind>(Zero.Instance, LoopIndependent.Instance);
    private static readonly ISet<DescriptorKind> _one = ImmutableHashSet.Create<DescriptorKind>(One.Instance, Positive.Instance, NotZero.Instance, LoopIndependent.Instance);
    private static readonly ISet<DescriptorKind> _positive = ImmutableHashSet.Create<DescriptorKind>(Positive.Instance, NotZero.Instance, LoopIndependent.Instance);
    private static readonly ISet<DescriptorKind> _negative = ImmutableHashSet.Create<DescriptorKind>(Negative.Instance, NotZero.Instance, LoopIndependent.Instance);

    protected override ISet<DescriptorKind> Transfer(IntegerExpression expression, RuleEngine engine) {
      if(expression.Value == 0) {
        return _zero;
      }

      if(expression.Value == 1) {
        return _one;
      }

      if(expression.Value > 0) {
        return _positive;
      }

      return _negative;
    }
  }
}
