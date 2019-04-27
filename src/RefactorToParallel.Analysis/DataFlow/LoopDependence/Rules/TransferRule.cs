using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {

  /// <summary>
  /// An implementation of this interfaces applies the transfer rules for a specified
  /// expression kind.
  /// </summary>
  public abstract class TransferRule<TExpression> : ITransferRule where TExpression : Expression {
    public ISet<DescriptorKind> Transfer(Expression expression, RuleEngine engine) {
      return Transfer((TExpression)expression, engine);
    }

    /// <summary>
    /// Transfers the information of the given expression to the resulting set.
    /// </summary>
    /// <param name="expression">The expression where the information should be transferred of.</param>
    /// <param name="engine">The engine applying the transfer.</param>
    /// <returns>The transferred information.</returns>
    protected abstract ISet<DescriptorKind> Transfer(TExpression expression, RuleEngine engine);

    /// <summary>
    /// Checks if all passed sets contain the desired rule.
    /// </summary>
    /// <param name="rule">The rule that has to be contained in all sets.</param>
    /// <param name="sets">The sets to check.</param>
    /// <returns><code>True</code> if all sets contain the specified rules.</returns>
    public static bool AllContainRule(DescriptorKind rule, params ISet<DescriptorKind>[] sets) {
      return sets.All(set => set.Contains(rule));
    }

    /// <summary>
    /// Checks if any of the passed sets contains the desired rule.
    /// </summary>
    /// <param name="rule">The rule that has to be contained in any of the sets.</param>
    /// <param name="sets">The sets to check.</param>
    /// <returns><code>True</code> if any of the sets contains the specified rules.</returns>
    public static bool AnyContainsRule(DescriptorKind rule, params ISet<DescriptorKind>[] sets) {
      return sets.Any(set => set.Contains(rule));
    }
  }
}
