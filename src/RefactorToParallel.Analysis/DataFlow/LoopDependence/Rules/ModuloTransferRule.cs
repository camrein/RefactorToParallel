using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for modulo.
  /// </summary>
  public class ModuloTransferRule : TransferRule<ModuloExpression> {
    protected override ISet<DescriptorKind> Transfer(ModuloExpression expression, RuleEngine engine) {
      var left = engine.Visit(expression.Left);
      var right = engine.Visit(expression.Right);

      var result = new HashSet<DescriptorKind>();
      if(left.Contains(Zero.Instance) || right.Contains(One.Instance)) {
        result.Add(Zero.Instance);
      }

      if(AllContainRule(LoopIndependent.Instance, left, right)) {
        result.Add(LoopIndependent.Instance);
      }

      return result;
    }
  }
}
