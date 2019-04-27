using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Kinds {
  [TestClass]
  public class DefinitionTest {
    private static FlowNode _CreateFlowNode() {
      return new FlowNode(new Label());
    }

    [TestMethod]
    public void InstanceIsNotEqualToNull() {
      var definition = new Definition(_CreateFlowNode());
      Assert.AreNotEqual(definition, null);
    }

    [TestMethod]
    public void InstanceIsEqualToItself() {
      var definition = new Definition(_CreateFlowNode());
      Assert.AreEqual(definition, definition);
    }

    [TestMethod]
    public void InstanceIsEqualToOtherWithSameNode() {
      var node = _CreateFlowNode();
      var definition1 = new Definition(node);
      var definition2 = new Definition(node);
      Assert.AreEqual(definition1, definition2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentNode() {
      var definition1 = new Definition(_CreateFlowNode());
      var definition2 = new Definition(_CreateFlowNode());
      Assert.AreNotEqual(definition1, definition2);
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsItself() {
      var definition = new Definition(_CreateFlowNode());
      Assert.AreEqual(definition.GetHashCode(), definition.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsOtherWithSameNode() {
      var node = _CreateFlowNode();
      var definition1 = new Definition(node);
      var definition2 = new Definition(node);
      Assert.AreEqual(definition1.GetHashCode(), definition2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentNode() {
      var definition1 = new Definition(_CreateFlowNode());
      var definition2 = new Definition(_CreateFlowNode());
      Assert.AreNotEqual(definition1.GetHashCode(), definition2.GetHashCode());
    }
  }
}
