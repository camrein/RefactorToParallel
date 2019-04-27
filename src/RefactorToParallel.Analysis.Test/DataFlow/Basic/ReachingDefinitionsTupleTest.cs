using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class ReachingDefinitionsTupleTest {
    private static FlowNode _CreateFlowNode() {
      return new FlowNode(new Label());
    }

    [TestMethod]
    public void InstanceIsNotEqualToNull() {
      var tuple = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      Assert.AreNotEqual(tuple, null);
    }

    [TestMethod]
    public void InstanceIsEqualToItself() {
      var tuple = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      Assert.AreEqual(tuple, tuple);
    }

    [TestMethod]
    public void InstanceIsEqualToOtherWithSameInformation() {
      var node = _CreateFlowNode();
      var tuple1 = new ReachingDefinitionsTuple("x", node);
      var tuple2 = new ReachingDefinitionsTuple("x", node);
      Assert.AreEqual(tuple1, tuple2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentVariable() {
      var node = _CreateFlowNode();
      var tuple1 = new ReachingDefinitionsTuple("x", node);
      var tuple2 = new ReachingDefinitionsTuple("y", node);
      Assert.AreNotEqual(tuple1, tuple2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentDefiningNode() {
      var tuple1 = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      var tuple2 = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      Assert.AreNotEqual(tuple1, tuple2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToCompletelyDifferent() {
      var tuple1 = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      var tuple2 = new ReachingDefinitionsTuple("y", _CreateFlowNode());
      Assert.AreNotEqual(tuple1, tuple2);
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsItself() {
      var tuple = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      Assert.AreEqual(tuple.GetHashCode(), tuple.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsOtherWithSameInformation() {
      var node = _CreateFlowNode();
      var tuple1 = new ReachingDefinitionsTuple("x", node);
      var tuple2 = new ReachingDefinitionsTuple("x", node);
      Assert.AreEqual(tuple1.GetHashCode(), tuple2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentVariable() {
      var node = _CreateFlowNode();
      var tuple1 = new ReachingDefinitionsTuple("x", node);
      var tuple2 = new ReachingDefinitionsTuple("y", node);
      Assert.AreNotEqual(tuple1.GetHashCode(), tuple2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentDefiningNode() {
      var tuple1 = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      var tuple2 = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      Assert.AreNotEqual(tuple1.GetHashCode(), tuple2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsCompletelyDifferent() {
      var tuple1 = new ReachingDefinitionsTuple("x", _CreateFlowNode());
      var tuple2 = new ReachingDefinitionsTuple("y", _CreateFlowNode());
      Assert.AreNotEqual(tuple1.GetHashCode(), tuple2.GetHashCode());
    }
  }
}
