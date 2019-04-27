using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.Test.IR;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class AvailableExpressionsAnalysisTest {
    private static FlowNode _GetNodeWithSyntax(ControlFlowGraph cfg, string syntax) {
      return cfg.Nodes.FirstOrDefault(node => string.Equals(node.Instruction.ToString(), syntax));
    }

    public ControlFlowGraph CreateControlFlowGraph(string body) {
      var code = TestCodeFactory.CreateThreeAddressCode(body);
      Debug.WriteLine(CodeStringifier.Generate(code));
      return ControlFlowGraphFactory.Create(code);
    }

    [TestMethod]
    public void ConstantAssignmentYieldsNoAvailableExpression() {
      var source = @"x = 1;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var originalAssignment = _GetNodeWithSyntax(cfg, "Assignment: x = 1");

      Assert.AreEqual(0, analysis.Entry[originalAssignment].Count);
      Assert.AreEqual(0, analysis.Exit[originalAssignment].Count);
    }

    [TestMethod]
    public void BinaryAssignmentYieldsAvailableExpression() {
      var source = @"x = 1 * 2;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var originalAssignment = _GetNodeWithSyntax(cfg, "Assignment: x = 1 * 2");

      Assert.AreEqual(0, analysis.Entry[originalAssignment].Count);
      Assert.AreEqual(1, analysis.Exit[originalAssignment].Count);
      Assert.AreEqual(new AvailableExpression(originalAssignment, new MultiplyExpression(new IntegerExpression(1), new IntegerExpression(2))), analysis.Exit[originalAssignment].Single());
    }

    [TestMethod]
    public void InvocationAssignmentYieldsAvailableExpression() {
      var source = @"x = Method(1);";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var originalAssignment = _GetNodeWithSyntax(cfg, "Assignment: x = Method(1)");

      Assert.AreEqual(0, analysis.Entry[originalAssignment].Count);
      Assert.AreEqual(1, analysis.Exit[originalAssignment].Count);
      Assert.AreEqual(new AvailableExpression(originalAssignment, new InvocationExpression("Method", new[] { new IntegerExpression(1) })), analysis.Exit[originalAssignment].Single());
    }

    [TestMethod]
    public void SelfAssignmentIsNotAvailable() {
      var source = @"x = x + 1;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var assignment = _GetNodeWithSyntax(cfg, "Assignment: x = x + 1");
      Assert.AreEqual(0, analysis.Entry[assignment].Count);
      Assert.AreEqual(0, analysis.Exit[assignment].Count);
    }

    [TestMethod]
    public void SelfAssignmentWithMethodInvocationIsNotAvailable() {
      var source = @"x = Method(x);";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var assignment = _GetNodeWithSyntax(cfg, "Assignment: x = Method(x)");
      Assert.AreEqual(0, analysis.Entry[assignment].Count);
      Assert.AreEqual(0, analysis.Exit[assignment].Count);
    }

    [TestMethod]
    public void MultipleBinaryExpressionsWithoutKilling() {
      var source = @"x = 1 * 2; x = 4 + 10;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var assignment1 = _GetNodeWithSyntax(cfg, "Assignment: x = 1 * 2");

      Assert.AreEqual(0, analysis.Entry[assignment1].Count);
      Assert.AreEqual(1, analysis.Exit[assignment1].Count);
      Assert.AreEqual(new AvailableExpression(assignment1, new MultiplyExpression(new IntegerExpression(1), new IntegerExpression(2))), analysis.Exit[assignment1].Single());

      var assignment2 = _GetNodeWithSyntax(cfg, "Assignment: x = 4 + 10");
      Assert.AreEqual(1, analysis.Entry[assignment2].Count);
      Assert.AreEqual(2, analysis.Exit[assignment2].Count);

      var expected = new HashSet<AvailableExpression> {
        new AvailableExpression(assignment1, new MultiplyExpression(new IntegerExpression(1), new IntegerExpression(2))),
        new AvailableExpression(assignment2, new AddExpression(new IntegerExpression(4), new IntegerExpression(10)))
      };
      Assert.IsTrue(analysis.Exit[assignment2].SetEquals(expected));
    }

    //[TestMethod]
    //public void AllInvocationsOfDifferentMethodsAreAvailable() {
    //  var source = @"var x = Method1(1); var y = Method2(1);";
    //  var cfg = CreateControlFlowGraph(source);
    //  var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

    //  var expected = new HashSet<AvailableExpression> {
    //    new AvailableExpression(new InvocationExpression("Method1", new [] { new IntegerExpression(1) })),
    //    new AvailableExpression(new InvocationExpression("Method2", new [] { new IntegerExpression(1) }))
    //  };
    //  Assert.IsTrue(analysis.Exit[cfg.End].SetEquals(expected));
    //}

    //[TestMethod]
    //public void AllInvocationsOfSameMethodsButDifferentArgumentsAreAvailable() {
    //  var source = @"var x = Method(1); var y = Method(2);";
    //  var cfg = CreateControlFlowGraph(source);
    //  var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

    //  var expected = new HashSet<AvailableExpression> {
    //    new AvailableExpression(new InvocationExpression("Method", new [] { new IntegerExpression(1) })),
    //    new AvailableExpression(new InvocationExpression("Method", new [] { new IntegerExpression(2) }))
    //  };
    //  Assert.IsTrue(analysis.Exit[cfg.End].SetEquals(expected));
    //}

    [TestMethod]
    public void MultipleBinaryExpressionsWithKilling() {
      var source = @"y = x * 2; x = 4 + 10;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var assignment1 = _GetNodeWithSyntax(cfg, "Assignment: y = x * 2");

      Assert.AreEqual(0, analysis.Entry[assignment1].Count);
      Assert.AreEqual(1, analysis.Exit[assignment1].Count);
      Assert.AreEqual(new AvailableExpression(assignment1, new MultiplyExpression(new VariableExpression("x"), new IntegerExpression(2))), analysis.Exit[assignment1].Single());

      var assignment2 = _GetNodeWithSyntax(cfg, "Assignment: x = 4 + 10");
      Assert.AreEqual(1, analysis.Entry[assignment2].Count);
      Assert.AreEqual(1, analysis.Exit[assignment2].Count);

      var expected = new HashSet<AvailableExpression> {
        new AvailableExpression(assignment2, new AddExpression(new IntegerExpression(4), new IntegerExpression(10)))
      };
      Assert.IsTrue(analysis.Exit[assignment2].SetEquals(expected));
    }

    [TestMethod]
    public void MultipleInvocationExpressionsWithKilling() {
      var source = @"y = Method(x); x = Method(1);";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var assignment1 = _GetNodeWithSyntax(cfg, "Assignment: y = Method(x)");

      Assert.AreEqual(0, analysis.Entry[assignment1].Count);
      Assert.AreEqual(1, analysis.Exit[assignment1].Count);
      Assert.AreEqual(new AvailableExpression(assignment1, new InvocationExpression("Method", new[] { new VariableExpression("x") })), analysis.Exit[assignment1].Single());

      var assignment2 = _GetNodeWithSyntax(cfg, "Assignment: x = Method(1)");
      Assert.AreEqual(1, analysis.Entry[assignment2].Count);
      Assert.AreEqual(1, analysis.Exit[assignment2].Count);

      var expected = new HashSet<AvailableExpression> {
        new AvailableExpression(assignment2, new InvocationExpression("Method", new [] { new IntegerExpression(1) }))
      };
      Assert.IsTrue(analysis.Exit[assignment2].SetEquals(expected));
    }

    [TestMethod]
    public void BookExample() {
      var source = @"
x = a + b;
y = a * b;
while(y > a + b) {
  a = a + 1;
  x = a + b;
}";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AvailableExpressionsAnalysis.Analyze(cfg);

      var instruction1 = _GetNodeWithSyntax(cfg, "Assignment: x = a + b");
      Assert.AreEqual(0, analysis.Entry[instruction1].Count);
      Assert.AreEqual(1, analysis.Exit[instruction1].Count);
      Assert.AreEqual(new AvailableExpression(instruction1, new AddExpression(new VariableExpression("a"), new VariableExpression("b"))), analysis.Exit[instruction1].Single());

      var instruction2 = _GetNodeWithSyntax(cfg, "Assignment: y = a * b");
      Assert.AreEqual(1, analysis.Entry[instruction2].Count);
      Assert.IsTrue(analysis.Exit[instruction1].SetEquals(analysis.Entry[instruction2]));
      Assert.AreEqual(2, analysis.Exit[instruction2].Count);
      var expected2 = new HashSet<AvailableExpression> {
        new AvailableExpression(instruction1, new AddExpression(new VariableExpression("a"), new VariableExpression("b"))),
        new AvailableExpression(instruction2, new MultiplyExpression(new VariableExpression("a"), new VariableExpression("b")))
      };
      Assert.IsTrue(analysis.Exit[instruction2].SetEquals(expected2));

      var tInstruction3 = _GetNodeWithSyntax(cfg, "Assignment: $temp_0 = a + b");
      var cInstruction3 = _GetNodeWithSyntax(cfg, "ConditionalJump: y > $temp_0");
      Assert.AreEqual(1, analysis.Entry[cInstruction3].Count);
      Assert.IsFalse(analysis.Exit[instruction2].SetEquals(analysis.Entry[cInstruction3]));
      Assert.AreEqual(1, analysis.Exit[cInstruction3].Count);
      Assert.AreEqual(new AvailableExpression(tInstruction3, new AddExpression(new VariableExpression("a"), new VariableExpression("b"))), analysis.Exit[cInstruction3].Single());

      var instruction4 = _GetNodeWithSyntax(cfg, "Assignment: a = a + 1");
      Assert.AreEqual(1, analysis.Entry[instruction4].Count);
      Assert.IsTrue(analysis.Exit[cInstruction3].SetEquals(analysis.Entry[instruction4]));
      Assert.AreEqual(0, analysis.Exit[instruction4].Count);

      var instruction5 = _GetNodeWithSyntax(cfg, "Assignment: x = a + b");
      Assert.AreEqual(0, analysis.Entry[instruction5].Count);
      Assert.IsTrue(analysis.Exit[instruction4].SetEquals(analysis.Entry[instruction5]));
      Assert.AreEqual(1, analysis.Exit[instruction5].Count);
      Assert.AreEqual(new AvailableExpression(instruction1, new AddExpression(new VariableExpression("a"), new VariableExpression("b"))), analysis.Exit[instruction5].Single());
    }
  }
}
