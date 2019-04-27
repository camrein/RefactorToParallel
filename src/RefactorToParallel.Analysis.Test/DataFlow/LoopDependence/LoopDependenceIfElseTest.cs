using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence {
  [TestClass]
  public class LoopDependenceIfElseTest : LoopDependenceTestBase {
    [TestMethod]
    public void SinglePositiveConstantAssignmentInsideIfBody() {
      // Note: there is no need to remove the information about the assignment of x
      // when leaving the if-body. Either the variable has been assigned earlier which leads
      // to different declaring nodes, or it has been declared outside of the loop which would
      // lead to a race condition anyway (previous step of the analysis).
      var source = @"if(1 == 1) { x = 13; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);

      var assignment = cfg.Nodes.Where(statement => statement.Instruction is Assignment).Single();
      Assert.AreEqual(4, analysis.Entry[assignment].Count);
      Assert.AreEqual(8, analysis.Exit[assignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignment]));

      Assert.AreEqual(8, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void AssignmentsOfDifferentConstantsPriorAndInsideIfBody() {
      var source = @"x = 5; if(1 == 1) { x = 13; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(4, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(8, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[1]]));

      Assert.AreEqual(6, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void AssignmentsOfLoopIndexAndConstantInsideIfBody() {
      var source = @"x = j; if(1 == 1) { x = 13; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(4, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(8, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[1]]));

      Assert.AreEqual(6, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void AssignmentsOfLoopIndexAndConstantInsideElseBody() {
      var source = @"x = j; if(1 == 1) { } else { x = 13; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(4, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(8, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[1]]));

      Assert.AreEqual(6, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void AssignmentsOfLoopIndexInIfAndConstantInsideElseBody() {
      var source = @"if(1 == 1) { x = j; } else { x = 13; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);

      var indexAssignment = GetNodeWithSyntax(cfg, "Assignment: x = j");
      Assert.AreEqual(4, analysis.Entry[indexAssignment].Count);
      Assert.AreEqual(8, analysis.Exit[indexAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[indexAssignment]));

      var constantAssignment = GetNodeWithSyntax(cfg, "Assignment: x = 13");
      Assert.AreEqual(4, analysis.Entry[constantAssignment].Count);
      Assert.AreEqual(8, analysis.Exit[constantAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[constantAssignment]));

      Assert.AreEqual(6, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void ConstantInsideIfBodyAndLoopIndexAfter() {
      var source = @"if(1 == 1) { x = 13; } x = j;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(4, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(8, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[assignments[1]]));
    }

    [TestMethod]
    public void MergeOfDistinctDefinitionsWithSameInformation() {
      var source = @"if(1 == 1) { x = j + 1; } else { x = j + 2 } y = x;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var plusOneAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("j + 2"))
        .Single();
      var plusTwoAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("j + 2"))
        .Single();
      var xAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("x"))
        .Single();

      Assert.AreEqual(4, analysis.Entry[plusOneAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+}", GetString(analysis.Entry[plusOneAssignment]));
      Assert.AreEqual(8, analysis.Exit[plusOneAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[plusOneAssignment]));

      Assert.AreEqual(4, analysis.Entry[plusTwoAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+}", GetString(analysis.Entry[plusTwoAssignment]));
      Assert.AreEqual(8, analysis.Exit[plusTwoAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[plusTwoAssignment]));

      Assert.AreEqual(6, analysis.Entry[xAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[xAssignment]));
      Assert.AreEqual(7, analysis.Exit[xAssignment].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Exit[xAssignment]));
    }

    [TestMethod]
    public void NestedIfStatementsOverwritingDefinitions() {
      var source = @"
var x = 0;
if(1 == 1) {
  x = 1;
  if(2 == 2) {
    x = 2;
  }
  
  var y = 3;
}
var z = x;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, null);

      var zeroAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("0"))
        .Single();
      var oneAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("1"))
        .Single();
      var twoAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("2"))
        .Single();
      var threeAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("3"))
        .Single();
      var xAssignment = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("x"))
        .Single();

      Assert.AreEqual(0, analysis.Entry[cfg.Start].Count);
      Assert.AreEqual(0, analysis.Exit[cfg.Start].Count);

      Assert.AreEqual(1, analysis.Entry[zeroAssignment].Count);
      Assert.AreEqual("{x@}", GetString(analysis.Entry[zeroAssignment]));
      Assert.AreEqual(3, analysis.Exit[zeroAssignment].Count);
      Assert.AreEqual("{x = 0, x/, x@}", GetString(analysis.Exit[zeroAssignment]));

      Assert.AreEqual(3, analysis.Entry[oneAssignment].Count);
      Assert.AreEqual("{x = 0, x/, x@}", GetString(analysis.Entry[oneAssignment]));
      Assert.AreEqual(5, analysis.Exit[oneAssignment].Count);
      Assert.AreEqual("{x != 0, x = 1, x/, x@, x+}", GetString(analysis.Exit[oneAssignment]));

      Assert.AreEqual(5, analysis.Entry[twoAssignment].Count);
      Assert.AreEqual("{x != 0, x = 1, x/, x@, x+}", GetString(analysis.Entry[twoAssignment]));
      Assert.AreEqual(4, analysis.Exit[twoAssignment].Count);
      Assert.AreEqual("{x != 0, x/, x@, x+}", GetString(analysis.Exit[twoAssignment]));

      Assert.AreEqual(3, analysis.Entry[threeAssignment].Count);
      Assert.AreEqual("{x@, x@, y@}", GetString(analysis.Entry[threeAssignment]));
      Assert.AreEqual(6, analysis.Exit[threeAssignment].Count);
      Assert.AreEqual("{x@, x@, y != 0, y/, y@, y+}", GetString(analysis.Exit[threeAssignment]));

      Assert.AreEqual(8, analysis.Entry[xAssignment].Count);
      Assert.AreEqual("{x@, x@, x@, y != 0, y/, y@, y+, z@}", GetString(analysis.Entry[xAssignment]));
      Assert.AreEqual(8, analysis.Exit[xAssignment].Count);
      Assert.AreEqual("{x@, x@, x@, y != 0, y/, y@, y+, z@}", GetString(analysis.Exit[xAssignment]));

      Assert.AreEqual(8, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{x@, x@, x@, y != 0, y/, y@, y+, z@}", GetString(analysis.Entry[cfg.End]));
      Assert.AreEqual(8, analysis.Exit[cfg.End].Count);
      Assert.AreEqual("{x@, x@, x@, y != 0, y/, y@, y+, z@}", GetString(analysis.Exit[cfg.End]));
    }
  }
}
