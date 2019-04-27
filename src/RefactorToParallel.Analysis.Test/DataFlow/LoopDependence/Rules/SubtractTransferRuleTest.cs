using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Diagnostics;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class SubtractTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void SubtractionOfNegativeConstantFromPositiveLoopDependence() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Positive.Instance, NotZero.Instance, LoopDependent.Instance);
      var transferred1 = engine.Visit(new SubtractExpression(input.Variable, new IntegerExpression(-5)));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }

    [TestMethod]
    public void SubtractionOfNegativeConstantFromNegativeLoopDependence() {
      var input = CreateNegativeLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopDependent.Instance);
      var transferred1 = engine.Visit(new SubtractExpression(input.Variable, new IntegerExpression(-5)));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }

    [TestMethod]
    public void SubtractionOfPositiveConstantFromNegativeLoopDependence() {
      var input = CreateNegativeLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Negative.Instance, NotZero.Instance, LoopDependent.Instance);
      Debug.WriteLine(string.Join(", ", expected));
      var transferred1 = engine.Visit(new SubtractExpression(input.Variable, new IntegerExpression(3)));
      Debug.WriteLine(string.Join(", ", transferred1));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }

    [TestMethod]
    public void SubtractionOfPositiveConstantFromPositiveLoopDependence() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopDependent.Instance);
      var transferred1 = engine.Visit(new SubtractExpression(input.Variable, new IntegerExpression(3)));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }

    [TestMethod]
    public void SubtractionOfPositiveConstantFromPositiveConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopIndependent.Instance);
      var transferred1 = engine.Visit(new SubtractExpression(new IntegerExpression(3), new IntegerExpression(3)));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }

    [TestMethod]
    public void SubtractionOfNegativeConstantFromNegativeConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopIndependent.Instance);
      var transferred1 = engine.Visit(new SubtractExpression(new IntegerExpression(-3), new IntegerExpression(-3)));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }

    [TestMethod]
    public void SubtractionOfZero() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred1 = engine.Visit(new SubtractExpression(input.Variable, new IntegerExpression(0)));

      Assert.IsTrue(transferred1.SetEquals(expected));
    }
  }
}
