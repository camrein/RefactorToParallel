using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.Optimizer;
using RefactorToParallel.Analysis.Test.IR;

namespace RefactorToParallel.Analysis.Test.Optimizer {
  [TestClass]
  public class CopyPropagationTest {
    [TestMethod]
    public void EmptyCode() {
      var source = "";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithoutCopies() {
      var source = "var x = 0; var y = 2; var z = x + y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithCodeWithSingleCopyUsage() {
      var source = "var x = 0; var y = x; var z = y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL z
z = x";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithCodeWithCopyUsageDoesNotPropagateNestedCopies() {
      var source = @"
var x = 0;
var y = x;
var z = y;

var a = z;
var b = a;
var c = b;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL z
z = x
DECL a
a = y
DECL b
b = z
DECL c
c = a";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithBranchOnlyCopy() {
      var source = @"
var x = 0;
var y = 2;
if(x > 0) {
  y = x;
}
var z = y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithInLoopCopy() {
      var source = @"
var x = 0;
var y = x + 1;
for(var i = 0; i < 10; ++i) {
  x = x + 1;
}";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameVariableButIntermediateOverwriting() {
      var source = @"
var x = 0;
var y = x;
x = 5;
var z = y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameVariableAndDoubleIntermediateOverwriting() {
      var source = @"
var x = 0;
var y = x;
var z = y;
x = 5;

var a = y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL z
z = x
x = 5
DECL a
a = y";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithSameVariableAndDoubleIntermediateOverwritingDoesNotAffectSucceedingCopies() {
      var source = @"
var x = 0;
var y = x;
var z = y;
x = 5;

var a = y;
var b = z;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL z
z = x
x = 5
DECL a
a = y
DECL b
b = y";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithBranchOverwritingCopy() {
      var source = @"
var x = 0;
var y = x;
if(x > 0) {
  y = 5;
}
var z = y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithUsageAndCopyInBranch() {
      var source = @"
var x = 0;
var y = 5;
if(x > 0) {
  y = x;
  var z = y;
}";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = 5
IF x > 0 JUMP #0
JUMP #1
LABEL #0
y = x
DECL z
z = x
LABEL #1";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CopyUsageInArrayAccessor() {
      var source = @"
var x = 0;
var y = x;
var z = 5 + array[y]";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL z
DECL $temp_0
$temp_0 = array[x]
z = 5 + $temp_0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CopyUsageInCondition() {
      var source = @"
var x = 0;
var y = x;
if(y == x) { }";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
IF x == x JUMP #0
JUMP #1
LABEL #0
LABEL #1";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CopyUsageInNestedBinaryExpressions() {
      var source = @"
var x = 0;
var y = x;
var a = 4;
var b = a;

var c = x + y * b - y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL a
a = 4
DECL b
b = a
DECL c
DECL $temp_0
$temp_0 = x * a
DECL $temp_1
$temp_1 = x + $temp_0
c = $temp_1 - x";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CopyUsageInMethodInvocation() {
      var source = @"
var x = 0;
var y = x;
Method(x, y);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
INVOKE Method(x, x)";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CopyUsageInNestedMethodInvocations() {
      var source = @"
var x = 0;
var y = x;
Outer(x, Inner1(Inner2(y)));";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CopyPropagation.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 0
DECL y
y = x
DECL $temp_0
$temp_0 = Inner2(x)
DECL $temp_1
$temp_1 = Inner1($temp_0)
INVOKE Outer(x, $temp_1)";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }
  }
}
