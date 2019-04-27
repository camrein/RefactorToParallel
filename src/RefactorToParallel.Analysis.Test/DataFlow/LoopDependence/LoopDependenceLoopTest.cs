using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence {
  [TestClass]
  public class LoopDependenceLoopTest : LoopDependenceTestBase {
    [TestMethod]
    public void EmptyForLoop() {
      var source = @"for(var i = 0; i < 10; i++) { }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);

      var incrementation = cfg.Nodes.Where(statement => statement.Instruction is Assignment).Skip(1).Single();
      Assert.AreEqual(6, analysis.Entry[incrementation].Count);
      Assert.AreEqual(5, analysis.Exit[incrementation].Count);
      Assert.AreEqual("{i@, j != 0, j@, j|, j+}", GetString(analysis.Exit[incrementation]));

      Assert.AreEqual(6, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void LoopIndexAssignmentToVariableDeclaredInsideForLoop() {
      var source = @"for(var i = 0; i < 10; i++) { var x = j; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var xAssignment = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("x") == true).Single();
      Assert.AreEqual(7, analysis.Entry[xAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@}", GetString(analysis.Entry[xAssignment]));  // x@ here because of the declaration
      Assert.AreEqual(10, analysis.Exit[xAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[xAssignment]));

      var iIncrementation = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("i") == true).Skip(1).Single();
      Assert.AreEqual(10, analysis.Entry[iIncrementation].Count);
      Assert.AreEqual(9, analysis.Exit[iIncrementation].Count);
      Assert.AreEqual("{i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[iIncrementation]));

      Assert.AreEqual(10, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Entry[cfg.End]));
      Assert.AreEqual(10, analysis.Exit[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[cfg.End]));
    }

    [TestMethod]
    public void LoopIndexAssignmentInsideForLoopWithSuccessor() {
      var source = @"int x; int i; for(i = 0; i < 10; i++) { x = j; } var y = i;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var xAssignment = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("x") == true).Single();
      Debug.WriteLine(GetString(analysis.Entry[xAssignment]));
      Assert.AreEqual(8, analysis.Entry[xAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[xAssignment]));
      Assert.AreEqual(10, analysis.Exit[xAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[xAssignment]));

      var iIncrementation = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("i") == true).Skip(1).Single();
      Assert.AreEqual(10, analysis.Entry[iIncrementation].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Entry[iIncrementation]));
      Assert.AreEqual(9, analysis.Exit[iIncrementation].Count);
      Assert.AreEqual("{i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[iIncrementation]));

      var yAssignment = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("y") == true).Single();
      Assert.AreEqual(9, analysis.Entry[yAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Entry[yAssignment]));
      Assert.AreEqual(9, analysis.Exit[yAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Exit[yAssignment]));

      Assert.AreEqual(9, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Entry[cfg.End]));
      Assert.AreEqual(9, analysis.Exit[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Exit[cfg.End]));
    }

    [TestMethod]
    public void LoopIndexAssignmentToVariableDeclaredInsideForLoopWithSuccessor() {
      var source = @"int i; for(i = 0; i < 10; i++) { var x = j; } var y = i;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var xAssignment = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("x") == true).Single();
      Assert.AreEqual(7, analysis.Entry[xAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@}", GetString(analysis.Entry[xAssignment]));  // x@ here because of the declaration
      Assert.AreEqual(10, analysis.Exit[xAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[xAssignment]));

      var iIncrementation = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("i") == true).Skip(1).Single();
      Assert.AreEqual(10, analysis.Entry[iIncrementation].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Entry[iIncrementation]));
      Assert.AreEqual(9, analysis.Exit[iIncrementation].Count);
      Assert.AreEqual("{i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[iIncrementation]));

      var yAssignment = cfg.Nodes.Where(statement => (statement.Instruction as Assignment)?.Left.ToString().Equals("y") == true).Single();
      Assert.AreEqual(11, analysis.Entry[yAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+, y@}", GetString(analysis.Entry[yAssignment]));
      Assert.AreEqual(11, analysis.Exit[yAssignment].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+, y@}", GetString(analysis.Exit[yAssignment]));

      Assert.AreEqual(11, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+, y@}", GetString(analysis.Entry[cfg.End]));
      Assert.AreEqual(11, analysis.Exit[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x != 0, x@, x|, x+, y@}", GetString(analysis.Exit[cfg.End]));
    }

    [TestMethod]
    public void EmptyWhileLoop() {
      var source = @"while(1 == 2) { }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);
      Assert.AreEqual(4, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void WhileLoopWithAssignmentPriorAndInside() {
      var source = @"x = 3; while(1 == 2) { x = j; }";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(4, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(6, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[assignments[1]]));
      Assert.AreEqual(8, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x != 0, x@, x|, x+}", GetString(analysis.Exit[assignments[1]]));

      Assert.AreEqual(6, analysis.Entry[cfg.End].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[cfg.End]));
    }

    [TestMethod]
    public void ForLoopWithAssignmentInsideAndAfterLoop() {
      var source = @"x = 0; for(var i = 0; i < 10; ++i) { x = x + 1; } var y = j - x;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "j");

      var xAssigned0 = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Left.ToString().Equals("x") && assignment.Right.ToString().Equals("0"))
        .Single();
      var xAssignedPlus1 = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("x + 1"))
        .Single();
      var yAssigned = cfg.Nodes
        .Where(statement => statement.Instruction is Assignment assignment && assignment.Right.ToString().Equals("j - x"))
        .Single();


      Assert.AreEqual(4, analysis.Entry[xAssigned0].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+}", GetString(analysis.Entry[xAssigned0]));
      Assert.AreEqual(7, analysis.Exit[xAssigned0].Count);
      Assert.AreEqual("{j != 0, j@, j|, j+, x = 0, x/, x@}", GetString(analysis.Exit[xAssigned0]));

      Assert.AreEqual(8, analysis.Entry[xAssignedPlus1].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@}", GetString(analysis.Entry[xAssignedPlus1]));
      Assert.AreEqual(7, analysis.Exit[xAssignedPlus1].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@}", GetString(analysis.Exit[xAssignedPlus1]));


      Assert.AreEqual(9, analysis.Entry[yAssigned].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Entry[yAssigned]));
      Assert.AreEqual(9, analysis.Exit[yAssigned].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Exit[yAssigned]));

      Assert.AreEqual(9, analysis.Exit[cfg.End].Count);
      Assert.AreEqual("{i@, i@, j != 0, j@, j|, j+, x@, x@, y@}", GetString(analysis.Exit[cfg.End]));
    }
  }
}
