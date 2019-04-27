using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  public class TransferTestBase {
    public (ISet<VariableDescriptor> Knowledge, VariableExpression Variable) CreateVariable(string variableName, params DescriptorKind[] kinds) {
      return (kinds.Select(kind => new VariableDescriptor(variableName, kind)).ToImmutableHashSet(), new VariableExpression(variableName));
    }

    public ISet<DescriptorKind> GetKinds(params DescriptorKind[] kinds) {
      return kinds.ToImmutableHashSet();
    }

    public ISet<DescriptorKind> GetKinds(IEnumerable<VariableDescriptor> descriptors) {
      return descriptors.Select(descriptor => descriptor.Kind).ToImmutableHashSet();
    }

    public (ISet<VariableDescriptor> Knowledge, VariableExpression Variable) CreatePositiveLoopVariable(string variableName) {
      return CreateVariable(variableName, NotZero.Instance, Positive.Instance, LoopDependent.Instance);
    }

    public (ISet<VariableDescriptor> Knowledge, VariableExpression Variable) CreateNegativeLoopVariable(string variableName) {
      return CreateVariable(variableName, NotZero.Instance, Negative.Instance, LoopDependent.Instance);
    }
  }
}
