using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class AvailableExpressionTest {
    private static FlowNode _CreateFlowNode() {
      return new FlowNode(new Label());
    }

    [TestMethod]
    public void InstanceIsNotEqualToNull() {
      var expression = new MultiplyExpression(new IntegerExpression(1), new VariableExpression("x"));
      var available = new AvailableExpression(_CreateFlowNode(), expression);
      Assert.AreNotEqual(available, null);
    }

    [TestMethod]
    public void InstanceIsEqualToItself() {
      var expression = new AddExpression(new IntegerExpression(5), new VariableExpression("y"));
      Assert.AreEqual(expression, expression);
    }

    [TestMethod]
    public void InstanceIsEqualToOtherWithSameInformation() {
      var node = _CreateFlowNode();
      var expression1 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var expression2 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreEqual(available1, available2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithOperator() {
      var node = _CreateFlowNode();
      var expression1 = new AddExpression(new IntegerExpression(5), new VariableExpression("y"));
      var expression2 = new MultiplyExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreNotEqual(available1, available2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentLeftOperand() {
      var node = _CreateFlowNode();
      var expression1 = new AddExpression(new IntegerExpression(3), new VariableExpression("y"));
      var expression2 = new AddExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreNotEqual(available1, available2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentRightOperand() {
      var node = _CreateFlowNode();
      var expression1 = new AddExpression(new VariableExpression("y"), new VariableExpression("y"));
      var expression2 = new AddExpression(new VariableExpression("y"), new VariableExpression("z"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreNotEqual(available1, available2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentNode() {
      var expression1 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var expression2 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(_CreateFlowNode(), expression1);
      var available2 = new AvailableExpression(_CreateFlowNode(), expression2);
      Assert.AreNotEqual(available1, available2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToCompletelyDifferent() {
      var expression1 = new AddExpression(new IntegerExpression(1), new IntegerExpression(2));
      var expression2 = new ModuloExpression(new VariableExpression("y"), new VariableExpression("z"));
      var available1 = new AvailableExpression(_CreateFlowNode(), expression1);
      var available2 = new AvailableExpression(_CreateFlowNode(), expression2);
      Assert.AreNotEqual(available1, available2);
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsItself() {
      var expression = new MultiplyExpression(new IntegerExpression(1), new VariableExpression("x"));
      var available = new AvailableExpression(_CreateFlowNode(), expression);
      Assert.AreEqual(available.GetHashCode(), available.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsOtherWithSameInformation() {
      var node = _CreateFlowNode();
      var expression1 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var expression2 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreEqual(available1.GetHashCode(), available2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentOperator() {
      var node = _CreateFlowNode();
      var expression1 = new AddExpression(new IntegerExpression(5), new VariableExpression("y"));
      var expression2 = new MultiplyExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreNotEqual(available1.GetHashCode(), available2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenLeftOperand() {
      var node = _CreateFlowNode();
      var expression1 = new AddExpression(new IntegerExpression(3), new VariableExpression("y"));
      var expression2 = new AddExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreNotEqual(available1.GetHashCode(), available2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenLeftRight() {
      var node = _CreateFlowNode();
      var expression1 = new ModuloExpression(new IntegerExpression(3), new VariableExpression("y"));
      var expression2 = new ModuloExpression(new IntegerExpression(3), new VariableExpression("z"));
      var available1 = new AvailableExpression(node, expression1);
      var available2 = new AvailableExpression(node, expression2);
      Assert.AreNotEqual(available1.GetHashCode(), available2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenNode() {
      var expression1 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var expression2 = new DivideExpression(new IntegerExpression(5), new VariableExpression("y"));
      var available1 = new AvailableExpression(_CreateFlowNode(), expression1);
      var available2 = new AvailableExpression(_CreateFlowNode(), expression2);
      Assert.AreNotEqual(available1.GetHashCode(), available2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsCompletelyDifferent() {
      var expression1 = new ModuloExpression(new IntegerExpression(3), new VariableExpression("y"));
      var expression2 = new DivideExpression(new IntegerExpression(4), new VariableExpression("z"));
      var available1 = new AvailableExpression(_CreateFlowNode(), expression1);
      var available2 = new AvailableExpression(_CreateFlowNode(), expression2);
      Assert.AreNotEqual(available1.GetHashCode(), available2.GetHashCode());
    }
  }
}
