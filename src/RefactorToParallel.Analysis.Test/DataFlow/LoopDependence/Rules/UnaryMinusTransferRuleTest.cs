using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class UnaryMinusTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void ZeroConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Zero.Instance, LoopIndependent.Instance);
      var transferred = engine.Visit(new UnaryMinusExpression(new IntegerExpression(0)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void OneConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Negative.Instance, One.Instance, LoopIndependent.Instance);
      var transferred = engine.Visit(new UnaryMinusExpression(new IntegerExpression(1)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void PositiveNotOneConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Negative.Instance, LoopIndependent.Instance);
      var transferred = engine.Visit(new UnaryMinusExpression(new IntegerExpression(5)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void NegativeConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Positive.Instance, LoopIndependent.Instance);
      var transferred = engine.Visit(new UnaryMinusExpression(new IntegerExpression(-10)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void NegationOfLoopIndex() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Negative.Instance, LoopDependent.Instance);
      var transferred = engine.Visit(new UnaryMinusExpression(new VariableExpression("i")));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void DoubleNegationOfLoopIndex() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Positive.Instance, LoopDependent.Instance);
      var transferred = engine.Visit(new UnaryMinusExpression(new UnaryMinusExpression(new VariableExpression("i"))));

      Assert.IsTrue(transferred.SetEquals(expected));
    }
  }
}
