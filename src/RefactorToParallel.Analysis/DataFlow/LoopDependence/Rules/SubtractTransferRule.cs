using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for subtractions.
  /// </summary>
  public class SubtractTransferRule : TransferRule<SubtractExpression> {
    protected override ISet<DescriptorKind> Transfer(SubtractExpression expression, RuleEngine engine) {
      var left = engine.Visit(expression.Left);
      var right = engine.Visit(expression.Right);

      if(right.Contains(Zero.Instance)) {
        return left;
      }

      var result = new HashSet<DescriptorKind>();
      if(_IsLoopDependent(left, right)) {
        result.Add(LoopDependent.Instance);
      }

      if(left.Contains(Positive.Instance) && right.Contains(Negative.Instance)) {
        result.Add(Positive.Instance);
        result.Add(NotZero.Instance);
      } else if(left.Contains(Negative.Instance) && right.Contains(Positive.Instance)) {
        result.Add(Negative.Instance);
        result.Add(NotZero.Instance);
      }

      if(AllContainRule(LoopIndependent.Instance, left, right)) {
        result.Add(LoopIndependent.Instance);
      }

      return result;
    }

    private static bool _IsLoopDependent(ISet<DescriptorKind> left, ISet<DescriptorKind> right) {
      return (AnyContainsRule(LoopDependent.Instance, left, right) && AnyContainsRule(LoopIndependent.Instance, left, right))
        || _CanTransferLoopDependance(left, right)
        || _CanTransferLoopDependance(right, left);
    }

    private static bool _CanTransferLoopDependance(ISet<DescriptorKind> a, ISet<DescriptorKind> b) {
      return (a.Contains(LoopDependent.Instance) && a.Contains(Positive.Instance) && b.Contains(Negative.Instance))
        || (a.Contains(LoopDependent.Instance) && a.Contains(Negative.Instance) && b.Contains(Positive.Instance));
    }
  }
}
