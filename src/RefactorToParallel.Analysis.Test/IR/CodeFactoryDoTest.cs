using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactoryDoTest : CodeFactoryTestBase {
    [TestMethod]
    public void EmptyDoStatement() {
      var source = @"do {} while(1 == 1);";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
LABEL #1
IF 1 == 1 JUMP #0
LABEL #2";

      Assert.AreEqual(4, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DoStatementWithoutSuccessor() {
      var source = @"do { var x = 1; } while(1 == 1);";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
DECL x
x = 1
LABEL #1
IF 1 == 1 JUMP #0
LABEL #2";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DoStatementWithSuccessor() {
      var source = @"do { x = 1; } while(1 == 1); y = 5;";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
x = 1
LABEL #1
IF 1 == 1 JUMP #0
LABEL #2
y = 5";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedDoStatement() {
      var source = @"
do {
  do {
    j = j + 1;
  } while(j < 100);
  i = i + 1;
} while(i < 100);";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
LABEL #1
j = j + 1
LABEL #2
IF j < 100 JUMP #1
LABEL #3
i = i + 1
LABEL #4
IF i < 100 JUMP #0
LABEL #5";

      Assert.AreEqual(10, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DoStatementWithContinue() {
      var source = @"
do {
  x = 4;
  if(i != 5) {
    continue;
  }
  x = 5;
} while(i < 100);";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
x = 4
IF i != 5 JUMP #1
JUMP #2
LABEL #1
JUMP #3
LABEL #2
x = 5
LABEL #3
IF i < 100 JUMP #0
LABEL #4";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DoStatementWithBreak() {
      var source = @"
do {
  x = 5;
  if(i > 0) {
    break;
  }
  i += 1;
} while(i < 100);";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
x = 5
IF i > 0 JUMP #1
JUMP #2
LABEL #1
JUMP #3
LABEL #2
i = i + 1
LABEL #4
IF i < 100 JUMP #0
LABEL #3";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }
  }
}
