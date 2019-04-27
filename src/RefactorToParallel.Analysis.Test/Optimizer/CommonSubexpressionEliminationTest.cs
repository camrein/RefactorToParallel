using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.Optimizer;
using RefactorToParallel.Analysis.Test.IR;

namespace RefactorToParallel.Analysis.Test.Optimizer {
  [TestClass]
  public class CommonSubexpressionEliminationTest {
    [TestMethod]
    public void EmptyCode() {
      var source = "";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithoutCommonSubexpressionsDueToDifferentOperators() {
      var source = "var x = x / 1; var y = x + 1; var z = x * 1;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithoutCommonSubexpressionsDueToDifferentOperands() {
      var source = "var x = x + 1; var y = x + 2; var z = y * 1;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithoutCommonSubexpressionsDueToDifferentArguments() {
      var source = "var x = Method(1); var y = Method(2); var z = y * 1;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithoutCommonSubexpressionsDueToDifferentMethods() {
      var source = "var x = Method1(1); var y = Method2(1); var z = y * 1;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSingleCommonSubexpression() {
      var source = "var x = 3; var y = x + 5; var z = x + 5;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 3
DECL y
y = x + 5
DECL z
z = y";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithSingleCommonInvocation() {
      var source = "var x = 3; var y = Method(x); var z = Method(x);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL x
x = 3
DECL y
y = Method(x)
DECL z
z = y";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithCommonArrayAccessorExpressions() {
      var source = @"
var a = array[x + 1];
array[x + 1] = 4;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL a
DECL $temp_0
$temp_0 = x + 1
a = array[$temp_0]
DECL $temp_1
$temp_1 = $temp_0
array[$temp_1] = 4";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithCommonInvocationArgumentExpressions() {
      var source = @"
var a = Method[x + 1];
Method[x + 1] = 4;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL a
DECL $temp_0
$temp_0 = x + 1
a = Method[$temp_0]
DECL $temp_1
$temp_1 = $temp_0
Method[$temp_1] = 4";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CodeWithSameExpressionsButIntermediateValueChange() {
      var source = @"
var x = 5;
var y = 3;

var a = y + x;
x = 2;
var b = y + x;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameInvocationsButIntermediateValueChange() {
      var source = @"
var x = 5;
var a = Method(x);
x = 2;
var b = Method(x);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameExpressionsButBranchedValueChange() {
      var source = @"
var x = 5;
var y = 3;

var a = y + x;
if(x > y) {
  x = 2;
}
var b = y + x;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameInvocationsButBranchedValueChange() {
      var source = @"
var x = 5;
var y = 3;

var a = Method(x);
if(x > y) {
  x = 2;
}
var b = Method(x);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameExpressionsButInLoop() {
      var source = @"
var x = 5;
var y = 3;

var a = y + x;
for(var i = 0; i < 10; ++i) {
  x = y + x;
}";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CodeWithSameInvocationsButInLoop() {
      var source = @"
var x = 5;
var y = 3;

var a = Method(x);
for(var i = 0; i < 10; ++i) {
  x = Method(x);
}";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void SelfAssignmentNegatesOptimization() {
      var source = @"
var x = 5;
var y = 3;

x = x + y;
var a = x + y;";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }

    [TestMethod]
    public void CommonSubexpressionsInSingleMethodInvocation() {
      var source = @"Method(x + 1, x + 1);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL $temp_0
$temp_0 = x + 1
DECL $temp_1
$temp_1 = $temp_0
INVOKE Method($temp_0, $temp_1)";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CommonSubexpressionsInMultipleMethodInvocations() {
      var source = @"var result = Method1(x + 1) + Method2(x + 1);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL result
DECL $temp_0
$temp_0 = x + 1
DECL $temp_1
$temp_1 = Method1($temp_0)
DECL $temp_2
$temp_2 = $temp_0
DECL $temp_3
$temp_3 = Method2($temp_2)
result = $temp_1 + $temp_3";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CommonSubexpressionsWithMultipleInvocationsOfTheSameMethod() {
      var source = @"var result = Method() + Method();";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL result
DECL $temp_0
$temp_0 = Method()
DECL $temp_1
$temp_1 = $temp_0
result = $temp_0 + $temp_1";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CommonSubexpressionsWithMultipleInvocationsOfTheSameMethodWithTheSameArguments() {
      var source = @"var result = Method(x) + Method(x);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreNotEqual(code, optimized.Code);
      Assert.IsTrue(optimized.Changed);

      var expected = @"
DECL result
DECL $temp_0
$temp_0 = Method(x)
DECL $temp_1
$temp_1 = $temp_0
result = $temp_0 + $temp_1";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(optimized.Code));
    }

    [TestMethod]
    public void CommonSubexpressionsWithMultipleInvocationsOfTheSameMethodWithDistinctArguments() {
      var source = @"var result = Method(x) + Method(y);";
      var code = TestCodeFactory.CreateThreeAddressCode(source);
      var cfg = ControlFlowGraphFactory.Create(code);
      var optimized = CommonSubexpressionElimination.Optimize(code, cfg);
      Assert.AreEqual(code, optimized.Code);
      Assert.IsFalse(optimized.Changed);
    }
  }
}
