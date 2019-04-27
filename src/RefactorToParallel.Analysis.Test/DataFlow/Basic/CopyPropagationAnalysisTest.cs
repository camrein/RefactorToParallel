using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class CopyPropagationAnalysisTest {
    private static FlowNode _GetNodeWithSyntax(ControlFlowGraph cfg, string syntax) {
      return cfg.Nodes.FirstOrDefault(node => string.Equals(node.Instruction.ToString(), syntax));
    }

    public ControlFlowGraph CreateControlFlowGraph(string body) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body));
    }

    [TestMethod]
    public void DirectCopy() {
      var source = @"var a = 1; var b = a;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = CopyPropagationAnalysis.Analyze(cfg);

      var assignmentA = _GetNodeWithSyntax(cfg, "Assignment: a = 1");
      var assignmentB = _GetNodeWithSyntax(cfg, "Assignment: b = a");

      Assert.AreEqual(0, analysis.Entry[assignmentA].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentA].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentB].Count);
      Assert.AreEqual(1, analysis.Exit[assignmentB].Count);
      Assert.AreEqual(new VariableCopy(assignmentB, "b", "a"), analysis.Exit[assignmentB].Single());
    }

    [TestMethod]
    public void DirectCopyAfterBranch() {
      var source = @"
var a = 1;
if(a == 1) {
  a = 2;
}
var b = a;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = CopyPropagationAnalysis.Analyze(cfg);

      var assignmentA1 = _GetNodeWithSyntax(cfg, "Assignment: a = 1");
      var assignmentA2 = _GetNodeWithSyntax(cfg, "Assignment: a = 2");
      var assignmentB = _GetNodeWithSyntax(cfg, "Assignment: b = a");

      Assert.AreEqual(0, analysis.Entry[assignmentA1].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentA1].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentA2].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentA2].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentB].Count);
      Assert.AreEqual(1, analysis.Exit[assignmentB].Count);
      Assert.AreEqual(new VariableCopy(assignmentB, "b", "a"), analysis.Exit[assignmentB].Single());
    }

    [TestMethod]
    public void CopyAfterLoopBranch() {
      var source = @"
var x = 1;
for(var i = 0; i < 10; ++i) {
  x = 2;
}
var y = x;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = CopyPropagationAnalysis.Analyze(cfg);

      var assignmentX1 = _GetNodeWithSyntax(cfg, "Assignment: x = 1");
      var assignmentX2 = _GetNodeWithSyntax(cfg, "Assignment: x = 2");
      var assignmentY = _GetNodeWithSyntax(cfg, "Assignment: y = x");

      Assert.AreEqual(0, analysis.Entry[assignmentX1].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentX1].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentX2].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentX2].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentY].Count);
      Assert.AreEqual(1, analysis.Exit[assignmentY].Count);
      Assert.AreEqual(new VariableCopy(assignmentY, "y", "x"), analysis.Exit[assignmentY].Single());
    }

    [TestMethod]
    public void NestedCopiesOnlyCopyIntermediate() {
      var source = @"
var x = 1;
var y = x;
var z = y;
var a = z;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = CopyPropagationAnalysis.Analyze(cfg);

      var assignmentX = _GetNodeWithSyntax(cfg, "Assignment: x = 1");
      var assignmentY = _GetNodeWithSyntax(cfg, "Assignment: y = x");
      var assignmentZ = _GetNodeWithSyntax(cfg, "Assignment: z = y");
      var assignmentA = _GetNodeWithSyntax(cfg, "Assignment: a = z");

      Assert.AreEqual(0, analysis.Entry[assignmentX].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentX].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentY].Count);
      Assert.AreEqual(1, analysis.Exit[assignmentY].Count);
      Assert.AreEqual(new VariableCopy(assignmentY, "y", "x"), analysis.Exit[assignmentY].Single());

      Assert.AreEqual(1, analysis.Entry[assignmentZ].Count);
      Assert.IsTrue(analysis.Entry[assignmentZ].SetEquals(analysis.Exit[assignmentY]));
      Assert.AreEqual(2, analysis.Exit[assignmentZ].Count);
      var expectedAfterZ = new HashSet<VariableCopy> {
        new VariableCopy(assignmentY, "y", "x"),
        new VariableCopy(assignmentZ, "z", "y"),
      };
      Assert.IsTrue(expectedAfterZ.SetEquals(analysis.Exit[assignmentZ]));

      Assert.AreEqual(2, analysis.Entry[assignmentA].Count);
      Assert.IsTrue(analysis.Entry[assignmentA].SetEquals(analysis.Exit[assignmentZ]));
      Assert.AreEqual(3, analysis.Exit[assignmentA].Count);
      var expectedAfterA = new HashSet<VariableCopy> {
        new VariableCopy(assignmentY, "y", "x"),
        new VariableCopy(assignmentZ, "z", "y"),
        new VariableCopy(assignmentA, "a", "z"),
      };
      Assert.IsTrue(expectedAfterA.SetEquals(analysis.Exit[assignmentA]));
    }

    [TestMethod]
    public void CopiesWhereIntermediateIsOverwritten() {
      var source = @"
var x = 1;
var y = x;
y = 2;
var z = y;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = CopyPropagationAnalysis.Analyze(cfg);

      var assignmentX = _GetNodeWithSyntax(cfg, "Assignment: x = 1");
      var assignmentY1 = _GetNodeWithSyntax(cfg, "Assignment: y = x");
      var assignmentY2 = _GetNodeWithSyntax(cfg, "Assignment: y = 2");
      var assignmentZ = _GetNodeWithSyntax(cfg, "Assignment: z = y");

      Assert.AreEqual(0, analysis.Entry[assignmentX].Count);
      Assert.AreEqual(0, analysis.Exit[assignmentX].Count);
      Assert.AreEqual(0, analysis.Entry[assignmentY1].Count);
      Assert.AreEqual(1, analysis.Exit[assignmentY1].Count);
      Assert.AreEqual(new VariableCopy(assignmentY1, "y", "x"), analysis.Exit[assignmentY1].Single());

      Assert.AreEqual(1, analysis.Entry[assignmentY2].Count);
      Assert.IsTrue(analysis.Entry[assignmentY2].SetEquals(analysis.Exit[assignmentY1]));
      Assert.AreEqual(0, analysis.Exit[assignmentY2].Count);

      Assert.AreEqual(0, analysis.Entry[assignmentZ].Count);
      Assert.AreEqual(1, analysis.Exit[assignmentZ].Count);
      Assert.AreEqual(new VariableCopy(assignmentZ, "z", "y"), analysis.Exit[assignmentZ].Single());
    }
  }
}
