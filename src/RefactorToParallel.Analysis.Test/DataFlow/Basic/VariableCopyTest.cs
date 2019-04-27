using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class VariableCopyTest {
    private static FlowNode _CreateFlowNode() {
      return new FlowNode(new Label());
    }

    [TestMethod]
    public void InstanceIsNotEqualToNull() {
      var access = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreNotEqual(access, null);
    }

    [TestMethod]
    public void InstanceIsEqualToItself() {
      var access = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreEqual(access, access);
    }

    [TestMethod]
    public void InstanceIsEqualToOtherWithSameInformation() {
      var node = _CreateFlowNode();
      var access1 = new VariableCopy(node, "x", "y");
      var access2 = new VariableCopy(node, "x", "y");
      Assert.AreEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentNode() {
      var access1 = new VariableCopy(_CreateFlowNode(), "x", "y");
      var access2 = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentSourceVariable() {
      var node = _CreateFlowNode();
      var access1 = new VariableCopy(node, "x", "y");
      var access2 = new VariableCopy(node, "x", "z");
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentTargetVariable() {
      var node = _CreateFlowNode();
      var access1 = new VariableCopy(node, "x", "y");
      var access2 = new VariableCopy(node, "z", "y");
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToCompletelyDifferent() {
      var access1 = new VariableCopy(_CreateFlowNode(), "a", "b");
      var access2 = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsItself() {
      var access = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreEqual(access.GetHashCode(), access.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsOtherWithSameInformation() {
      var node = _CreateFlowNode();
      var access1 = new VariableCopy(node, "x", "y");
      var access2 = new VariableCopy(node, "x", "y");
      Assert.AreEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentNode() {
      var access1 = new VariableCopy(_CreateFlowNode(), "x", "y");
      var access2 = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenSourceVariable() {
      var node = _CreateFlowNode();
      var access1 = new VariableCopy(node, "x", "y");
      var access2 = new VariableCopy(node, "x", "z");
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenTargetVariable() {
      var node = _CreateFlowNode();
      var access1 = new VariableCopy(node, "x", "y");
      var access2 = new VariableCopy(node, "z", "y");
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsCompletelyDifferent() {
      var access1 = new VariableCopy(_CreateFlowNode(), "a", "b");
      var access2 = new VariableCopy(_CreateFlowNode(), "x", "y");
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }
  }
}
