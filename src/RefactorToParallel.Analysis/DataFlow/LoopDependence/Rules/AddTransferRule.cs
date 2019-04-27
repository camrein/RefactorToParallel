using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for additions.
  /// </summary>
  public class AddTransferRule : TransferRule<AddExpression> {
    protected override ISet<DescriptorKind> Transfer(AddExpression expression, RuleEngine engine) {
      var left = engine.Visit(expression.Left);
      var right = engine.Visit(expression.Right);

      if(left.Contains(Zero.Instance)) {
        return right;
      } else if(right.Contains(Zero.Instance)) {
        return left;
      }

      var result = new HashSet<DescriptorKind>();
      if(_IsLoopDependent(left, right)) {
        result.Add(LoopDependent.Instance);
      }

      if(AllContainRule(Positive.Instance, left, right)) {
        result.Add(Positive.Instance);
        result.Add(NotZero.Instance);
      } else if(AllContainRule(Negative.Instance, left, right)) {
        result.Add(Negative.Instance);
        result.Add(NotZero.Instance);
      }

      if(AllContainRule(LoopIndependent.Instance, left, right)) {
        result.Add(LoopIndependent.Instance);
      }

      return result;
    }

    private static bool _IsLoopDependent(ISet<DescriptorKind> left, ISet<DescriptorKind> right) {
      return AnyContainsRule(LoopDependent.Instance, left, right) 
        && (AllContainRule(Positive.Instance, left, right) || AllContainRule(Negative.Instance, left, right) || AnyContainsRule(LoopIndependent.Instance, left, right));
    }
  }
}
