using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class DivideTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void DivisionOfArbitraryPositiveConstants() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopIndependent.Instance);
      var transferred = engine.Visit(new DivideExpression(new IntegerExpression(5), new IntegerExpression(3)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void DivisionOfLoopIndexByConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds();
      var transferred = engine.Visit(new DivideExpression(input.Variable, new IntegerExpression(3)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void DivisionOfZero() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Zero.Instance, LoopIndependent.Instance);
      var transferred = engine.Visit(new DivideExpression(new IntegerExpression(0), input.Variable));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void DivisionByOne() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred = engine.Visit(new DivideExpression(input.Variable, new IntegerExpression(1)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }
  }
}
