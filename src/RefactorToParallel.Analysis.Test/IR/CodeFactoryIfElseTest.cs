using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactoryIfElseTest : CodeFactoryTestBase {
    [TestMethod]
    public void EmptyIfStatement() {
      var source = @"if(1 == 1) {}";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 1 JUMP #0
JUMP #1
LABEL #0
LABEL #1";

      Assert.AreEqual(4, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void IfStatementWithoutSuccessor() {
      var source = @"if(1 == 1) { x = 1; }";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 1 JUMP #0
JUMP #1
LABEL #0
x = 1
LABEL #1";

      Assert.AreEqual(5, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void IfStatementWithSuccessor() {
      var source = @"if(1 == 1) { x = 1; } x = 2;";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 1 JUMP #0
JUMP #1
LABEL #0
x = 1
LABEL #1
x = 2";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void EmptyIfElseStatement() {
      var source = @"if(1 == 2) { } else { }";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 2 JUMP #0
JUMP #1
LABEL #0
LABEL #1";

      Assert.AreEqual(4, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void IfElseStatementWithoutSuccessor() {
      var source = @"if(1 == 1) { x = 1; } else { x = 2; }";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 1 JUMP #0
x = 2
JUMP #1
LABEL #0
x = 1
LABEL #1";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void IfElseStatementWithSuccessor() {
      var source = @"if(1 == 1) { x = 1; } else { x = 2; } x = 3;";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 1 JUMP #0
x = 2
JUMP #1
LABEL #0
x = 1
LABEL #1
x = 3";

      Assert.AreEqual(7, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void EmptyStatementWithElse() {
      var source = @"if(1 == 5) { } else { x = 2; }";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 5 JUMP #0
x = 2
JUMP #1
LABEL #0
LABEL #1";

      Assert.AreEqual(5, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedEmptyIfStatement() {
      var source = @"if(1 == 5) { if(1 == 1) {} }";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 5 JUMP #0
JUMP #1
LABEL #0
IF 1 == 1 JUMP #2
JUMP #3
LABEL #2
LABEL #3
LABEL #1";

      Assert.AreEqual(8, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedIfElseStatement() {
      var source = @"
if(1 == 5) {
  if(1 == 1) {
    x = 0;
  } else {
    y = 2;
  }
} else {
  x = 5;
}";
      var code = CreateCode(source);
      var expected = @"
IF 1 == 5 JUMP #0
x = 5
JUMP #1
LABEL #0
IF 1 == 1 JUMP #2
y = 2
JUMP #3
LABEL #2
x = 0
LABEL #3
LABEL #1";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void IfElseIfElseStatement() {
      var source = @"
if(x > 5) {
  more = 1;
} else if(x < 5) {
  less = 1;
} else {
  equal = 1;
}";
      var code = CreateCode(source);
      var expected = @"
IF x > 5 JUMP #0
IF x < 5 JUMP #1
equal = 1
JUMP #2
LABEL #1
less = 1
LABEL #2
JUMP #3
LABEL #0
more = 1
LABEL #3";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }
  }
}
