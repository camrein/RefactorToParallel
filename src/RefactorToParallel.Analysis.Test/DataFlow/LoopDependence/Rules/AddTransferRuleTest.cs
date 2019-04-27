using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence.Rules {
  [TestClass]
  public class AddTransferRuleTest : TransferTestBase {
    [TestMethod]
    public void AdditionOfPositiveLoopDependenceAndConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Positive.Instance, NotZero.Instance, LoopDependent.Instance);
      var transferred1 = engine.Visit(new AddExpression(input.Variable, new IntegerExpression(5)));
      var transferred2 = engine.Visit(new AddExpression(new IntegerExpression(5), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void AdditionOfNegativeLoopDependenceAndConstant() {
      var input = CreateNegativeLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Negative.Instance, NotZero.Instance, LoopDependent.Instance);
      var transferred1 = engine.Visit(new AddExpression(input.Variable, new IntegerExpression(-3)));
      var transferred2 = engine.Visit(new AddExpression(new IntegerExpression(-3), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void AdditionOfNegativeAndPositiveConstants() {
      var input = CreateNegativeLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopIndependent.Instance);
      var transferred1 = engine.Visit(new AddExpression(new IntegerExpression(5), new IntegerExpression(-3)));
      var transferred2 = engine.Visit(new AddExpression(new IntegerExpression(-3), new IntegerExpression(5)));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void AdditionOfPositiveLoopVariableWithNegativeConstant() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(LoopDependent.Instance);
      var transferred1 = engine.Visit(new AddExpression(input.Variable, new IntegerExpression(-3)));
      var transferred2 = engine.Visit(new AddExpression(new IntegerExpression(-3), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void AdditionOfPositiveLoopVariableWithNegativeLoopVariable() {
      var input1 = CreatePositiveLoopVariable("i");
      var input2 = CreateNegativeLoopVariable("x");
      var knowledge = new HashSet<VariableDescriptor>(input1.Knowledge.Concat(input2.Knowledge));
      var engine = new RuleEngine(knowledge);

      var expected = GetKinds();
      var transferred1 = engine.Visit(new AddExpression(input1.Variable, input2.Variable));
      var transferred2 = engine.Visit(new AddExpression(input2.Variable, input1.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void AdditionOfZero() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(input.Knowledge);
      var transferred1 = engine.Visit(new AddExpression(input.Variable, new IntegerExpression(0)));
      var transferred2 = engine.Visit(new AddExpression(new IntegerExpression(0), input.Variable));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }

    [TestMethod]
    public void AdditionOfOneToZero() {
      var input = CreatePositiveLoopVariable("i");
      var engine = new RuleEngine(input.Knowledge);

      var expected = GetKinds(Positive.Instance, NotZero.Instance, One.Instance, LoopIndependent.Instance);
      var transferred1 = engine.Visit(new AddExpression(new IntegerExpression(1), new IntegerExpression(0)));
      var transferred2 = engine.Visit(new AddExpression(new IntegerExpression(0), new IntegerExpression(1)));

      Assert.IsTrue(transferred1.SetEquals(expected));
      Assert.IsTrue(transferred2.SetEquals(expected));
    }
  }
}
