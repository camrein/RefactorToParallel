using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class VariableTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void ZeroVariable() {
      var input = CreateVariable("x", Zero.Instance);
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred = engine.Visit(input.Variable);

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void PositiveConstantVariable() {
      var input = CreateVariable("x", NotZero.Instance, Positive.Instance);
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred = engine.Visit(input.Variable);

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void NegativeLoopDependantVariable() {
      var input = CreateVariable("x", NotZero.Instance, Negative.Instance, LoopDependent.Instance);
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred = engine.Visit(input.Variable);

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void UnknownVariable() {
      var input = CreateVariable("x", NotZero.Instance, Positive.Instance);
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds();
      var transferred = engine.Visit(new VariableExpression("y"));

      Assert.IsTrue(transferred.SetEquals(expected));
    }

    [TestMethod]
    public void MultipleDistinctVariables() {
      var inputs = new [] {
        CreateVariable("x", NotZero.Instance, Positive.Instance),
        CreateVariable("y", Negative.Instance, NotZero.Instance),
        CreateVariable("z", Zero.Instance),
        CreateVariable("i", NotZero.Instance, Positive.Instance, LoopDependent.Instance)
      };

      var knowledge = inputs.SelectMany(input => input.Knowledge).ToImmutableHashSet();
      var engine = new RuleEngine(knowledge);

      foreach(var input in inputs) {
        var expected = input.Knowledge.Select(descriptor => descriptor.Kind);
        var transferred = engine.Visit(input.Variable);
        Assert.IsTrue(transferred.SetEquals(expected));
      }
    }
  }
}
