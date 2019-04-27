using RefactorToParallel.Analysis.ControlFlow;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  public class ControlFlowGraphFactoryTestBase {
    public ControlFlowGraph CreateControlFlowGraph(string body) {
      return CreateControlFlowGraph(body, false);
    }

    public ControlFlowGraph CreateControlFlowGraph(string body, bool interprocedural) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body), interprocedural);
    }
  }
}
