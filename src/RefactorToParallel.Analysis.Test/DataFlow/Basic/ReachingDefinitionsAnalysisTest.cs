using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class ReachingDefinitionsAnalysisTest {
    private static FlowNode _GetNodeWithSyntax(ControlFlowGraph cfg, string syntax) {
      return cfg.Nodes.FirstOrDefault(node => string.Equals(node.Instruction.ToString(), syntax));
    }

    public ControlFlowGraph CreateControlFlowGraph(string body) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body));
    }

    [TestMethod]
    public void SingleDeclaration() {
      var source = @"var original = 1;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = ReachingDefinitionsAnalysis.Analyze(cfg);

      var originalAssignment = _GetNodeWithSyntax(cfg, "Assignment: original = 1");

      Assert.AreEqual(0, analysis.Entry[originalAssignment].Count);
      Assert.AreEqual(1, analysis.Exit[originalAssignment].Count);
      Assert.AreEqual(new ReachingDefinitionsTuple("original", originalAssignment), analysis.Exit[originalAssignment].Single());
    }

    [TestMethod]
    public void DeclarationWithDirectAssignment() {
      var source = @"var original = 1; var alias = original;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = ReachingDefinitionsAnalysis.Analyze(cfg);

      var originalAssignment = _GetNodeWithSyntax(cfg, "Assignment: original = 1");
      var aliasAssignment = _GetNodeWithSyntax(cfg, "Assignment: alias = original");

      Assert.AreEqual(0, analysis.Entry[originalAssignment].Count);
      Assert.AreEqual(1, analysis.Exit[originalAssignment].Count);
      Assert.AreEqual(new ReachingDefinitionsTuple("original", originalAssignment),
        analysis.Exit[originalAssignment].Single());

      Assert.AreEqual(1, analysis.Entry[aliasAssignment].Count);
      Assert.AreEqual(2, analysis.Exit[aliasAssignment].Count);
      Assert.IsTrue(analysis.Exit[aliasAssignment].Contains(new ReachingDefinitionsTuple("original", originalAssignment)));
      Assert.IsTrue(analysis.Exit[aliasAssignment].Contains(new ReachingDefinitionsTuple("alias", aliasAssignment)));
    }

    [TestMethod]
    public void DeclarationWithAssignmentInLoop() {
      const string source = @"
var original = 1;
while(1 == 1) {
  original = original + 1;
}";
      var cfg = CreateControlFlowGraph(source);
      var analysis = ReachingDefinitionsAnalysis.Analyze(cfg);

      var assignment = _GetNodeWithSyntax(cfg, "Assignment: original = 1");
      var incrementInLoop = _GetNodeWithSyntax(cfg, "Assignment: original = original + 1");

      var incrementEntry = analysis.Entry[incrementInLoop];
      Assert.AreEqual(2, incrementEntry.Count);
      Assert.IsTrue(incrementEntry.Contains(new ReachingDefinitionsTuple("original", assignment)));
      Assert.IsTrue(incrementEntry.Contains(new ReachingDefinitionsTuple("original", incrementInLoop)));

      var incrementExit = analysis.Exit[incrementInLoop];
      Assert.AreEqual(1, incrementExit.Count);
      Assert.IsTrue(incrementExit.Contains(new ReachingDefinitionsTuple("original", incrementInLoop)));

      var endExit = analysis.Exit[cfg.End];
      Assert.AreEqual(2, endExit.Count);
      Assert.IsTrue(endExit.Contains(new ReachingDefinitionsTuple("original", assignment)));
      Assert.IsTrue(endExit.Contains(new ReachingDefinitionsTuple("original", incrementInLoop)));
    }
  }
}
