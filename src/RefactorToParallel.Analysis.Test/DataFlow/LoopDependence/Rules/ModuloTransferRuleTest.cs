using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class ModuloTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void ModuloOfArbitraryPositiveConstants() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopIndependent.Instance);
      var transferred = engine.Visit(new ModuloExpression(new IntegerExpression(5), new IntegerExpression(3)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void ModuloOfLoopIndexByConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds();
      var transferred = engine.Visit(new ModuloExpression(input.Variable, new IntegerExpression(3)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void ModuloOfZero() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Zero.Instance);
      var transferred = engine.Visit(new ModuloExpression(new IntegerExpression(0), input.Variable));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void ModuloByOne() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Zero.Instance);
      var transferred = engine.Visit(new ModuloExpression(input.Variable, new IntegerExpression(1)));

      Assert.IsTrue(transferred.SetEquals(expected));
    }
  }
}
