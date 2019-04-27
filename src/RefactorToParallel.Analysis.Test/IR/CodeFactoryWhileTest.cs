using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactoryWhileTest : CodeFactoryTestBase {
    [TestMethod]
    public void EmptyWhileStatement() {
      var source = @"while(1 == 1) {}";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
IF 1 == 1 JUMP #1
JUMP #2
LABEL #1
JUMP #0
LABEL #2";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WhileStatementWithoutSuccessor() {
      var source = @"while(1 == 1) { var x = 1; }";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
IF 1 == 1 JUMP #1
JUMP #2
LABEL #1
DECL x
x = 1
JUMP #0
LABEL #2";

      Assert.AreEqual(8, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WhileStatementWithSuccessor() {
      var source = @"while(1 == 1) { x = 1; } y = 5;";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
IF 1 == 1 JUMP #1
JUMP #2
LABEL #1
x = 1
JUMP #0
LABEL #2
y = 5";

      Assert.AreEqual(8, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedWhileStatement() {
      var source = @"
while(i < 100) {
  while(j < 100) {
    j = j + 1;
  }
  i = i + 1;
}";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
LABEL #3
IF j < 100 JUMP #4
JUMP #5
LABEL #4
j = j + 1
JUMP #3
LABEL #5
i = i + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(14, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WhileStatementWithContinue() {
      var source = @"
while(i < 100) {
  if(i%2 == 0) {
    continue;
  }
}";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
IF i % 2 == 0 JUMP #3
JUMP #4
LABEL #3
JUMP #0
LABEL #4
JUMP #0
LABEL #2";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WhileStatementWithBreak() {
      var source = @"
while(i < 100) {
  if(i%2 == 0) {
    break;
  }
}";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
IF i % 2 == 0 JUMP #3
JUMP #4
LABEL #3
JUMP #2
LABEL #4
JUMP #0
LABEL #2";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }
  }
}
