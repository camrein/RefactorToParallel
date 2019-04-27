using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactoryForTest : CodeFactoryTestBase {
    [TestMethod]
    public void CompletelyEmptyForStatement() {
      var source = @"for(;;) {}";
      var code = CreateCode(source);
      var expected = @"
LABEL #0
JUMP #1
JUMP #2
LABEL #1
LABEL #3
JUMP #0
LABEL #2";

      Assert.AreEqual(7, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithoutCondition() {
      var source = @"for(i = 5; ; i = i + 2) { var x = 1; }";
      var code = CreateCode(source);
      var expected = @"
i = 5
LABEL #0
JUMP #1
JUMP #2
LABEL #1
DECL x
x = 1
LABEL #3
i = i + 2
JUMP #0
LABEL #2";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void EmptyForStatement() {
      var source = @"for(var i = 0; i < 10; i = i + 1) {}";
      var code = CreateCode(source);
      var expected = @"
DECL i
i = 0
LABEL #0
IF i < 10 JUMP #1
JUMP #2
LABEL #1
LABEL #3
i = i + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(10, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithoutSuccessor() {
      var source = @"for(i = 5; i < 100; i = i + 2) { var x = 1; }";
      var code = CreateCode(source);
      var expected = @"
i = 5
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
DECL x
x = 1
LABEL #3
i = i + 2
JUMP #0
LABEL #2";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithMultipleVariables() {
      var source = @"for(i = 5, j = 0; i < 100; i = i + 2, j++) {}";
      var code = CreateCode(source);
      var expected = @"
i = 5
j = 0
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
LABEL #3
i = i + 2
j = j + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithSuccessor() {
      var source = @"for(int i = 0; i < 100; i++) { var x = 1; } y = 5;";
      var code = CreateCode(source);
      var expected = @"
DECL i
i = 0
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
DECL x
x = 1
LABEL #3
i = i + 1
JUMP #0
LABEL #2
y = 5";

      Assert.AreEqual(13, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedForStatement() {
      var source = @"
for(int i = 0; i < 100; i++) {
  for(var j = 0; j < 100; j += 5) {
    x = x + 0;
  }
}";
      var code = CreateCode(source);
      var expected = @"
DECL i
i = 0
LABEL #0
IF i < 100 JUMP #1
JUMP #2
LABEL #1
DECL j
j = 0
LABEL #3
IF j < 100 JUMP #4
JUMP #5
LABEL #4
x = x + 0
LABEL #6
j = j + 5
JUMP #3
LABEL #5
LABEL #7
i = i + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(21, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithContinue() {
      var source = @"
for(var i = 0; i < 10; ++i) {
  if(i%4 == 0) {
    continue;
  }
  var x = 0;
}";
      var code = CreateCode(source);
      var expected = @"
DECL i
i = 0
LABEL #0
IF i < 10 JUMP #1
JUMP #2
LABEL #1
IF i % 4 == 0 JUMP #3
JUMP #4
LABEL #3
JUMP #5
LABEL #4
DECL x
x = 0
LABEL #5
i = i + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(17, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithBreak() {
      var source = @"
for(var i = 0; i < 10; ++i) {
  if(i > 3) {
    break;
  }
}";
      var code = CreateCode(source);
      var expected = @"
DECL i
i = 0
LABEL #0
IF i < 10 JUMP #1
JUMP #2
LABEL #1
IF i > 3 JUMP #3
JUMP #4
LABEL #3
JUMP #2
LABEL #4
LABEL #5
i = i + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(15, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ForStatementWithoutConditionButConditionalBreak() {
      var source = @"
for(var i = 0; ; ++i) {
  if(i > 3) {
    break;
  }
}";
      var code = CreateCode(source);
      var expected = @"
DECL i
i = 0
LABEL #0
JUMP #1
JUMP #2
LABEL #1
IF i > 3 JUMP #3
JUMP #4
LABEL #3
JUMP #2
LABEL #4
LABEL #5
i = i + 1
JUMP #0
LABEL #2";

      Assert.AreEqual(15, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }
  }
}
