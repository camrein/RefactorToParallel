using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This part of the transfer rule applies the rules for variables.
  /// </summary>
  public class VariableTransferRule : TransferRule<VariableExpression> {
    protected override ISet<DescriptorKind> Transfer(VariableExpression expression, RuleEngine engine) {
      var variableName = expression.Name;
      return engine.Input
        .Where(descriptor => descriptor.Name.Equals(variableName))
        .Select(descriptor => descriptor.Kind)
        .WithoutKind<Definition>()
        .ToImmutableHashSet();
    }

    public ISet<DescriptorKind> Transfer(string variableName, RuleEngine engine) {
      return engine.Input
        .Where(descriptor => descriptor.Name.Equals(variableName))
        .Select(descriptor => descriptor.Kind)
        .WithoutKind<Definition>()
        .ToImmutableHashSet();
    }
  }
}
