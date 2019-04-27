using RefactorToParallel.Analysis.IR;

namespace RefactorToParallel.Analysis.Test.IR {
  public class CodeFactoryTestBase {
    public Code CreateCode(string body) {
      return TestCodeFactory.CreateCode(body);
    }
  }
}
