using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for multiplications.
  /// </summary>
  public class MultiplyTransferRule : TransferRule<MultiplyExpression> {
    protected override ISet<DescriptorKind> Transfer(MultiplyExpression expression, RuleEngine engine) {
      var left = engine.Visit(expression.Left);
      var right = engine.Visit(expression.Right);

      if(left.Contains(One.Instance)) {
        return right;
      } else if(right.Contains(One.Instance)) {
        return left;
      }

      var result = new HashSet<DescriptorKind>();
      if(AnyContainsRule(Zero.Instance, left, right)) {
        result.Add(Zero.Instance);
        return result;
      }

      if(AnyContainsRule(LoopDependent.Instance, left, right) && AllContainRule(NotZero.Instance, left, right)) {
        result.Add(LoopDependent.Instance);
      }

      if(AllContainRule(Positive.Instance, left, right) || AllContainRule(Negative.Instance, left, right)) {
        result.Add(Positive.Instance);
      } else if(AnyContainsRule(Positive.Instance, left, right) && AnyContainsRule(Negative.Instance, left, right)) {
        result.Add(Negative.Instance);
      }

      if(AllContainRule(NotZero.Instance, left, right)) {
        result.Add(NotZero.Instance);
      }

      if(AllContainRule(LoopIndependent.Instance, left, right)) {
        result.Add(LoopIndependent.Instance);
      }

      return result;
    }
  }
}
