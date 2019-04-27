using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class VariableAliasTest {
    private static Expression _CreateExpression() {
      return new IntegerExpression(1);
    }

    [TestMethod]
    public void InstanceIsNotEqualToNull() {
      var alias = new VariableAlias("x", _CreateExpression());
      Assert.AreNotEqual(alias, null);
    }

    [TestMethod]
    public void InstanceIsEqualToItself() {
      var alias = new VariableAlias("x", _CreateExpression());
      Assert.AreEqual(alias, alias);
    }

    [TestMethod]
    public void InstanceIsEqualToOtherWithSameInformation() {
      var expression = _CreateExpression();
      var alias1 = new VariableAlias("x", expression);
      var alias2 = new VariableAlias("x", expression);
      Assert.AreEqual(alias1, alias2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentVariable() {
      var expression = _CreateExpression();
      var alias1 = new VariableAlias("x", expression);
      var alias2 = new VariableAlias("y", expression);
      Assert.AreNotEqual(alias1, alias2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToOtherWithDifferentExpression() {
      var alias1 = new VariableAlias("x", _CreateExpression());
      var alias2 = new VariableAlias("x", _CreateExpression());
      Assert.AreNotEqual(alias1, alias2);
    }

    [TestMethod]
    public void InstanceIsNotEqualToCompletelyDifferent() {
      var alias1 = new VariableAlias("x", _CreateExpression());
      var alias2 = new VariableAlias("y", _CreateExpression());
      Assert.AreNotEqual(alias1, alias2);
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsItself() {
      var alias = new VariableAlias("x", _CreateExpression());
      Assert.AreEqual(alias.GetHashCode(), alias.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasSameHashCodeAsOtherWithSameInformation() {
      var expression = _CreateExpression();
      var alias1 = new VariableAlias("x", expression);
      var alias2 = new VariableAlias("x", expression);
      Assert.AreEqual(alias1.GetHashCode(), alias2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferentVariable() {
      var expression = _CreateExpression();
      var alias1 = new VariableAlias("x", expression);
      var alias2 = new VariableAlias("y", expression);
      Assert.AreNotEqual(alias1.GetHashCode(), alias2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsOtherWithDifferenExpression() {
      var alias1 = new VariableAlias("x", _CreateExpression());
      var alias2 = new VariableAlias("x", _CreateExpression());
      Assert.AreNotEqual(alias1.GetHashCode(), alias2.GetHashCode());
    }

    [TestMethod]
    public void InstanceHasNotSameHashCodeAsCompletelyDifferent() {
      var alias1 = new VariableAlias("x", _CreateExpression());
      var alias2 = new VariableAlias("y", _CreateExpression());
      Assert.AreNotEqual(alias1.GetHashCode(), alias2.GetHashCode());
    }
  }
}
