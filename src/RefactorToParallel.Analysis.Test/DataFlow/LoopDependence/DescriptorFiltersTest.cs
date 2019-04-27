using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using System.Collections.Generic;
using System.Linq;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence {
  [TestClass]
  public class DescriptorFiltersTest {
    [TestMethod]
    public void IsOfKindReturnsTrueForEqualKind() {
      var descriptor = new VariableDescriptor("test1", NotZero.Instance);
      Assert.IsTrue(descriptor.IsOfKind<NotZero>());
    }

    [TestMethod]
    public void IsOfKindReturnsFalseForUnequalKind() {
      var descriptor = new VariableDescriptor("test1", Positive.Instance);
      Assert.IsFalse(descriptor.IsOfKind<NotZero>());
    }

    [TestMethod]
    public void IsNotOfKindReturnsFalseForEqualKind() {
      var descriptor = new VariableDescriptor("test2", new Definition(new FlowNode(new Label())));
      Assert.IsFalse(descriptor.IsNotOfKind<Definition>());
    }

    [TestMethod]
    public void IsNotOfKindReturnsTrueForUnequalKind() {
      var descriptor = new VariableDescriptor("test2", Positive.Instance);
      Assert.IsTrue(descriptor.IsNotOfKind<NotZero>());
    }

    [TestMethod]
    public void WithoutKindRemovesSpecifiedKindFromDescriptors() {
      var input = new List<VariableDescriptor> {
        new VariableDescriptor("test1", One.Instance),
        new VariableDescriptor("test1", NotZero.Instance),
        new VariableDescriptor("test1", Positive.Instance),
        new VariableDescriptor("test2", One.Instance),
        new VariableDescriptor("test2", NotZero.Instance),
        new VariableDescriptor("test2", Positive.Instance)
      };

      var output = input.WithoutKind<One>().ToList();
      Assert.AreEqual(6, input.Count);
      Assert.AreEqual(4, output.Count);
      Assert.IsFalse(output.Any(descriptor => descriptor.Kind is One));
    }

    [TestMethod]
    public void WithoutKindRemovesSpecifiedKindFromKinds() {
      var input = new List<DescriptorKind> {
        One.Instance, NotZero.Instance, Positive.Instance,
        new Definition(new FlowNode(new Label()))
      };

      var output = input.WithoutKind<NotZero>().ToList();
      Assert.AreEqual(4, input.Count);
      Assert.AreEqual(3, output.Count);
      Assert.IsFalse(output.OfType<NotZero>().Any());
    }

    [TestMethod]
    public void OnlyWithKindRemovesAllOthersFromDescriptors() {
      var input = new List<VariableDescriptor> {
        new VariableDescriptor("test1", One.Instance),
        new VariableDescriptor("test1", NotZero.Instance),
        new VariableDescriptor("test1", Positive.Instance),
        new VariableDescriptor("test2", One.Instance),
        new VariableDescriptor("test2", NotZero.Instance),
        new VariableDescriptor("test2", Positive.Instance)
      };

      var output = input.OnlyWithKind<NotZero>().ToList();
      Assert.AreEqual(6, input.Count);
      Assert.AreEqual(2, output.Count);
      Assert.IsTrue(output.All(descriptor => descriptor.Kind is NotZero));
    }
  }
}
