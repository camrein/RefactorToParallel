using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class MultiplyTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void MultiplicationOfPositiveLoopDependenceAndConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Positive.Instance, NotZero.Instance, LoopDependent.Instance);
      var transferred1 = engine.Visit(new MultiplyExpression(input.Variable, new IntegerExpression(5)));
      var transferred2 = engine.Visit(new MultiplyExpression(new IntegerExpression(5), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void MultiplicationOfNegativeLoopDependenceAndConstant() {
      var input = CreateNegativeLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Positive.Instance, NotZero.Instance, LoopDependent.Instance);
      var transferred1 = engine.Visit(new MultiplyExpression(input.Variable, new IntegerExpression(-3)));
      var transferred2 = engine.Visit(new MultiplyExpression(new IntegerExpression(-3), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void MultiplicationOfNegativeAndPositiveConstants() {
      var input = CreateNegativeLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Negative.Instance, LoopIndependent.Instance);
      var transferred1 = engine.Visit(new MultiplyExpression(new IntegerExpression(5), new IntegerExpression(-3)));
      var transferred2 = engine.Visit(new MultiplyExpression(new IntegerExpression(-3), new IntegerExpression(5)));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void MultiplicationOfPositiveLoopVariableWithNegativeConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(NotZero.Instance, Negative.Instance, LoopDependent.Instance);
      var transferred1 = engine.Visit(new MultiplyExpression(input.Variable, new IntegerExpression(-3)));
      var transferred2 = engine.Visit(new MultiplyExpression(new IntegerExpression(-3), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void MultiplicationWithZero() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Zero.Instance);
      var transferred1 = engine.Visit(new MultiplyExpression(input.Variable, new IntegerExpression(0)));
      var transferred2 = engine.Visit(new MultiplyExpression(new IntegerExpression(0), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void MultiplicationWithOne() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred1 = engine.Visit(new MultiplyExpression(input.Variable, new IntegerExpression(1)));
      var transferred2 = engine.Visit(new MultiplyExpression(new IntegerExpression(1), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }
  }
}
