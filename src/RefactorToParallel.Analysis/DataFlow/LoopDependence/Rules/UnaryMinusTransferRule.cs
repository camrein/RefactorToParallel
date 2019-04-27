using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for unary minus expressions (aka negations).
  /// </summary>
  public class UnaryMinusTransferRule : TransferRule<UnaryMinusExpression> {
    protected override ISet<DescriptorKind> Transfer(UnaryMinusExpression expression, RuleEngine engine) {
      var result = new HashSet<DescriptorKind>(engine.Visit(expression.Expression));

      if(result.Contains(Positive.Instance)) {
        result.Remove(Positive.Instance);
        result.Add(Negative.Instance);
      } else if(result.Contains(Negative.Instance)) {
        result.Remove(Negative.Instance);
        result.Add(Positive.Instance);
      }

      return result;
    }
  }
}
