using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Collectors;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.Collectors {
  [TestClass]
  public class ArrayAccessTest {
    private static ArrayExpression _CreateArrayExpression() {
      return new ArrayExpression("x", new[] { new IntegerExpression(1) });
    }

    private static FlowNode _CreateFlowNode() {
      return new FlowNode(new Label());
    }

    [TestMethod]
    public void InstanceIsNotEqualToNull() {
      var access = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), false);
      Assert.AreNotEqual(access, null);
    }

    [TestMethod]
    public void InstanceIsEqualToItself() {
      var access = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), false);
      Assert.AreEqual(access, access);
    }

    [TestMethod]
    public void InstanceIsEqualToOtherWithSameInformation() {
      var expression = _CreateArrayExpression();
      var node = _CreateFlowNode();
      var access1 = new ArrayAccess(node, expression, true);
      var access2 = new ArrayAccess(node, expression, true);
      Assert.AreEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentNode() {
      var expression = _CreateArrayExpression();
      var access1 = new ArrayAccess(_CreateFlowNode(), expression, true);
      var access2 = new ArrayAccess(_CreateFlowNode(), expression, true);
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentExpression() {
      var node = _CreateFlowNode();
      var access1 = new ArrayAccess(node, _CreateArrayExpression(), false);
      var access2 = new ArrayAccess(node, _CreateArrayExpression(), false);
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualIfOneIsWritingAndOtherNot() {
      var expression = _CreateArrayExpression();
      var node = _CreateFlowNode();
      var access1 = new ArrayAccess(node, expression, false);
      var access2 = new ArrayAccess(node, expression, true);
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToCompletelyDifferent() {
      var access1 = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), true);
      var access2 = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), false);
      Assert.AreNotEqual(access1, access2);
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsItself() {
      var access = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), false);
      Assert.AreEqual(access.GetHashCode(), access.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsOtherWithSameInformation() {
      var expression = _CreateArrayExpression();
      var node = _CreateFlowNode();
      var access1 = new ArrayAccess(node, expression, true);
      var access2 = new ArrayAccess(node, expression, true);
      Assert.AreEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentNode() {
      var expression = _CreateArrayExpression();
      var access1 = new ArrayAccess(_CreateFlowNode(), expression, true);
      var access2 = new ArrayAccess(_CreateFlowNode(), expression, true);
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenExpression() {
      var node = _CreateFlowNode();
      var access1 = new ArrayAccess(node, _CreateArrayExpression(), false);
      var access2 = new ArrayAccess(node, _CreateArrayExpression(), false);
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstancesHaveNotSameHashCodeIfOneIsWritingAndOtherNot() {
      var expression = _CreateArrayExpression();
      var node = _CreateFlowNode();
      var access1 = new ArrayAccess(node, expression, false);
      var access2 = new ArrayAccess(node, expression, true);
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsCompletelyDifferent() {
      var access1 = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), true);
      var access2 = new ArrayAccess(_CreateFlowNode(), _CreateArrayExpression(), false);
      Assert.AreNotEqual(access1.GetHashCode(), access2.GetHashCode());
    }
  }
}
