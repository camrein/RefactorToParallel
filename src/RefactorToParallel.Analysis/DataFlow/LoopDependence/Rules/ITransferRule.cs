using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// An implementation of this interfaces applies the transfer rules for a specified
  /// expression kind.
  /// </summary>
  public interface ITransferRule {
    /// <summary>
    /// Transfers the information of the given expression to the resulting set.
    /// </summary>
    /// <param name="expression">The expression where the information should be transferred of.</param>
    /// <param name="engine">The engine applying the transfer.</param>
    /// <returns>The transferred information.</returns>
    ISet<DescriptorKind> Transfer(Expression expression, RuleEngine engine);
  }
}
