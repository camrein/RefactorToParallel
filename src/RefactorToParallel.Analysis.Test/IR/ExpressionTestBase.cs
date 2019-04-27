using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.IR {
  public class ExpressionTestBase {
    public Expression CreateExpression(string body) {
      return TestCodeFactory.CreateExpression(body);
    }
  }
}
