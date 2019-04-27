using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.IR {
  // TODO add tests for aliasing
  [TestClass]
  public class ExpressionEqualityTest {
    private ExpressionEquality _instance;

    [TestInitialize]
    public void Initialize() {
      _instance = new ExpressionEquality();
    }

    [TestMethod]
    public void SameInstancesAreEqual() {
      var variable = new VariableExpression("a");
      Assert.IsTrue(_instance.AreEqual(variable, variable));
    }

    [TestMethod]
    public void SameIntegersAreEqual() {
      Assert.IsTrue(_instance.AreEqual(new IntegerExpression(5), new IntegerExpression(5)));
    }

    [TestMethod]
    public void DistinctIntegersAreNotEqual() {
      Assert.IsFalse(_instance.AreEqual(new IntegerExpression(5), new IntegerExpression(3)));
    }

    [TestMethod]
    public void SameVariablesAreEqual() {
      Assert.IsTrue(_instance.AreEqual(new IntegerExpression(5), new IntegerExpression(5)));
    }

    [TestMethod]
    public void DistinctVariablesAreNotEqual() {
      Assert.IsFalse(_instance.AreEqual(new VariableExpression("a"), new VariableExpression("b")));
    }

    [TestMethod]
    public void SameNegationsAreEqual() {
      Assert.IsTrue(_instance.AreEqual(new UnaryMinusExpression(new IntegerExpression(5)), new UnaryMinusExpression(new IntegerExpression(5))));
    }

    [TestMethod]
    public void DistinctNegationsAreNotEqual() {
      Assert.IsFalse(_instance.AreEqual(new UnaryMinusExpression(new IntegerExpression(5)), new UnaryMinusExpression(new IntegerExpression(4))));
    }

    [TestMethod]
    public void EqualAddExpressionsSameOrder() {
      var a = new AddExpression(new VariableExpression("a"), new IntegerExpression(5));
      var b = new AddExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void EqualAddExpressionsMirrored() {
      var a = new AddExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new AddExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void DistinctAddExpressions() {
      var a = new AddExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new AddExpression(new VariableExpression("a"), new VariableExpression("a"));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void EqualMultiplyExpressionsSameOrder() {
      var a = new MultiplyExpression(new VariableExpression("a"), new IntegerExpression(5));
      var b = new MultiplyExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void EqualMultiplyExpressionsMirrored() {
      var a = new MultiplyExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new MultiplyExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void DistinctMultiplyExpressions() {
      var a = new MultiplyExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new MultiplyExpression(new VariableExpression("a"), new VariableExpression("a"));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void EqualSubtractExpressionsSameOrder() {
      var a = new SubtractExpression(new VariableExpression("a"), new IntegerExpression(5));
      var b = new SubtractExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void MirroredSubtractExpressionsAreDistinct() {
      var a = new SubtractExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new SubtractExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void DistinctSubtractExpressions() {
      var a = new SubtractExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new SubtractExpression(new VariableExpression("a"), new VariableExpression("a"));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void EqualDivideExpressionsSameOrder() {
      var a = new DivideExpression(new VariableExpression("a"), new IntegerExpression(5));
      var b = new DivideExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void MirroredDivideExpressionsAreDistinct() {
      var a = new DivideExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new DivideExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void DistinctDivideExpressions() {
      var a = new DivideExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new DivideExpression(new VariableExpression("a"), new VariableExpression("a"));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void EqualModuloExpressionsSameOrder() {
      var a = new ModuloExpression(new VariableExpression("a"), new IntegerExpression(5));
      var b = new ModuloExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsTrue(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void MirroredModuloExpressionsAreDistinct() {
      var a = new ModuloExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new ModuloExpression(new VariableExpression("a"), new IntegerExpression(5));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }

    [TestMethod]
    public void DistinctModuloExpressions() {
      var a = new ModuloExpression(new IntegerExpression(5), new VariableExpression("a"));
      var b = new ModuloExpression(new VariableExpression("a"), new VariableExpression("a"));
      Assert.IsFalse(_instance.AreEqual(a, b));
    }
  }
}
