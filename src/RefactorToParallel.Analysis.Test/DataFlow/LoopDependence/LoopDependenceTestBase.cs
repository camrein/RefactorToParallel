using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence {
  [TestClass]
  public class LoopDependenceTestBase {
    public virtual ControlFlowGraph CreateControlFlowGraph(string body) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body));
    }
    public FlowNode GetNodeWithSyntax(ControlFlowGraph cfg, string syntax) {
      return cfg.Nodes.FirstOrDefault(node => string.Equals(node.Instruction.ToString(), syntax));
    }

    public string GetString(IEnumerable<VariableDescriptor> state) {
      return $"{{{string.Join(", ", state.OrderBy(descriptor => descriptor.ToString()))}}}";
    }
  }
}
