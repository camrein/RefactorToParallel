using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence {
  [TestClass]
  public class LoopDependenceBasicTest : LoopDependenceTestBase {

    [TestMethod]
    public void EmptyBodyWithoutLoopIndex() {
      var source = @"";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, null);
      Assert.AreEqual(2, analysis.Entry.Count);
      Assert.AreEqual(2, analysis.Exit.Count);

      Assert.AreEqual(0, analysis.Entry[cfg.Start].Count);
      Assert.AreEqual(0, analysis.Exit[cfg.Start].Count);
      Assert.AreEqual(0, analysis.Entry[cfg.End].Count);
      Assert.AreEqual(0, analysis.Exit[cfg.End].Count);
    }

    [TestMethod]
    public void EmptyBodyWithLoopIndex() {
      var source = @"";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");
      Assert.AreEqual(2, analysis.Entry.Count);
      Assert.AreEqual(2, analysis.Exit.Count);

      Assert.AreEqual(4, analysis.Entry[cfg.Start].Count);
      Assert.AreEqual(4, analysis.Exit[cfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Exit[cfg.Start]));


      Assert.AreEqual(4, analysis.Entry[cfg.End].Count);
      Assert.AreEqual(4, analysis.Exit[cfg.End].Count);
    }

    [TestMethod]
    public void ConstantPositiveNumberDeclaration() {
      var source = @"var x = 30;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");
      Assert.AreEqual(4, analysis.Entry.Count);
      Assert.AreEqual(4, analysis.Exit.Count);

      var assignment = cfg.Nodes.Where(node => node.Instruction is Assignment).Single();
      Assert.AreEqual(5, analysis.Entry[assignment].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x@}", GetString(analysis.Entry[assignment]));

      Assert.AreEqual(8, analysis.Exit[assignment].Count);
      Assert.IsTrue(analysis.Exit[assignment].Any(descriptor => descriptor.Kind is NotZero));
      Assert.IsTrue(analysis.Exit[assignment].Any(descriptor => descriptor.Kind is Positive));
      Assert.IsTrue(analysis.Exit[assignment].Any(descriptor => descriptor.Kind is LoopIndependent));
      Assert.AreEqual("{i != 0, i@, i|, i+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignment]));

      Assert.AreEqual(8, analysis.Entry[cfg.End].Count);
      Assert.AreEqual(8, analysis.Exit[cfg.End].Count);
    }

    [TestMethod]
    public void ConstantZeroAssignment() {
      var source = @"x = 0;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");
      Assert.AreEqual(3, analysis.Entry.Count);
      Assert.AreEqual(3, analysis.Exit.Count);

      var declaration = cfg.Nodes.Where(node => node.Instruction is Assignment).Single();
      Assert.AreEqual(4, analysis.Entry[declaration].Count);
      Assert.AreEqual(7, analysis.Exit[declaration].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x = 0, x/, x@}", GetString(analysis.Exit[declaration]));

      Assert.AreEqual(7, analysis.Entry[cfg.End].Count);
      Assert.AreEqual(7, analysis.Exit[cfg.End].Count);
    }

    [TestMethod]
    public void AssignmentOfLoopIndex() {
      var source = @"x = i;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");
      Assert.AreEqual(3, analysis.Entry.Count);
      Assert.AreEqual(3, analysis.Exit.Count);

      var declaration = cfg.Nodes.Where(node => node.Instruction is Assignment).Single();
      Assert.AreEqual(4, analysis.Entry[declaration].Count);
      Assert.AreEqual(8, analysis.Exit[declaration].Count);
      Assert.AreEqual(2, analysis.Exit[declaration].Count(descriptor => descriptor.Kind is NotZero));
      Assert.AreEqual(2, analysis.Exit[declaration].Count(descriptor => descriptor.Kind is LoopDependent));
      Assert.AreEqual(2, analysis.Exit[declaration].Count(descriptor => descriptor.Kind is Positive));
      Assert.AreEqual(2, analysis.Exit[declaration].Count(descriptor => descriptor.Kind is Definition));
      Assert.AreEqual(4, analysis.Exit[declaration].Count(descriptor => descriptor.Name.Equals("x")));
      Assert.AreEqual(4, analysis.Exit[declaration].Count(descriptor => descriptor.Name.Equals("i")));
      Assert.AreEqual("{i != 0, i@, i|, i+, x != 0, x@, x|, x+}", GetString(analysis.Exit[declaration]));

      Assert.AreEqual(8, analysis.Entry[cfg.End].Count);
      Assert.AreEqual(8, analysis.Exit[cfg.End].Count);
    }

    [TestMethod]
    public void DeclaredWithConstantAndOverwrittenWithLoopIndex() {
      var source = @"var x = 3; x = i;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(5, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x != 0, x/, x@, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(8, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x != 0, x@, x|, x+}", GetString(analysis.Exit[assignments[1]]));
    }

    [TestMethod]
    public void DeclaredWithLoopIndexAndOverwrittenWithZero() {
      var source = @"var x = i; x = 0;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");

      var assignments = cfg.Nodes.Where(statement => statement.Instruction is Assignment).ToList();
      Assert.AreEqual(5, analysis.Entry[assignments[0]].Count);
      Assert.AreEqual(8, analysis.Exit[assignments[0]].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x != 0, x@, x|, x+}", GetString(analysis.Exit[assignments[0]]));

      Assert.AreEqual(8, analysis.Entry[assignments[1]].Count);
      Assert.AreEqual(7, analysis.Exit[assignments[1]].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x = 0, x/, x@}", GetString(analysis.Exit[assignments[1]]));
    }

    [TestMethod]
    public void DeclaredWithNegationOfLoopIndex() {
      var source = @"var x = -i;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");

      var assignment = cfg.Nodes.Where(statement => statement.Instruction is Assignment).Single();
      Assert.AreEqual(5, analysis.Entry[assignment].Count);
      Assert.AreEqual(8, analysis.Exit[assignment].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x-, x != 0, x@, x|}", GetString(analysis.Exit[assignment]));
    }

    [TestMethod]
    public void UnknownInvocationTargetDoesNotTransferInformation() {
      var source = @"var x = Method(i);";
      var cfg = CreateControlFlowGraph(source);
      var analysis = LoopDependenceAnalysis.Analyze(cfg, "i");
      Assert.AreEqual(4, analysis.Entry[cfg.Start].Count);

      var declaration = cfg.Nodes.Where(statement => statement.Instruction is Declaration d && d.Name.Equals("x")).Single();
      Assert.AreEqual(4, analysis.Entry[declaration].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[declaration]));
      Assert.AreEqual(5, analysis.Exit[declaration].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x@}", GetString(analysis.Exit[declaration]));

      var assignment = cfg.Nodes.Where(statement => statement.Instruction is Assignment a && a.Left.ToString().Equals("x")).Single();
      Assert.AreEqual(5, analysis.Entry[assignment].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x@}", GetString(analysis.Entry[assignment]));
      Assert.AreEqual(5, analysis.Exit[assignment].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+, x@}", GetString(analysis.Exit[assignment]));
    }
  }
}
