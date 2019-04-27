using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class ThreeAddressCodeFactoryTest {
    private Code _CreateCode(string body) {
      return TestCodeFactory.CreateThreeAddressCode(body);
    }

    [TestMethod]
    public void DeclarationWithConstant() {
      var source = @"var x = 5";
      var code = _CreateCode(source);
      var expected = @"
DECL x
x = 5";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithConstantBinaryExpression() {
      var source = @"var x = 5 * 4";
      var code = _CreateCode(source);
      var expected = @"
DECL x
x = 5 * 4";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithSingleNestedExpression() {
      var source = @"var x = 5 * 4 + 3";
      var code = _CreateCode(source);
      var expected = @"
DECL x
DECL $temp_0
$temp_0 = 5 * 4
x = $temp_0 + 3";

      Assert.AreEqual(4, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithDoubleNestedExpression() {
      var source = @"var x = 5 * 4 + 3 * 10";
      var code = _CreateCode(source);
      var expected = @"
DECL x
DECL $temp_0
$temp_0 = 5 * 4
DECL $temp_1
$temp_1 = 3 * 10
x = $temp_0 + $temp_1";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void AssignmentsAndDeclarationsWithMultipleNestings() {
      var source = @"
var x = 0;
var y = 10;
x = y;

y = (5 + x) * y + x * y / y";
      var code = _CreateCode(source);
      var expected = @"
DECL x
x = 0
DECL y
y = 10
x = y
DECL $temp_0
$temp_0 = 5 + x
DECL $temp_1
$temp_1 = $temp_0 * y
DECL $temp_2
$temp_2 = x * y
DECL $temp_3
$temp_3 = $temp_2 / y
y = $temp_1 + $temp_3";

      Assert.AreEqual(14, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WriteAccessToArrayWithConstantIndex() {
      var source = @"shared[0] = 1;";
      var code = _CreateCode(source);
      var expected = @"shared[0] = 1";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WriteAccessToArrayWithVariableIndex() {
      var source = @"shared[x] = 1;";
      var code = _CreateCode(source);
      var expected = @"shared[x] = 1";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WriteAccessToArrayWithExpressionIndex() {
      var source = @"shared[x + 1] = 1;";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = x + 1
shared[$temp_0] = 1";

      Assert.AreEqual(3, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void WriteAccessToArrayThroughArrayNesting() {
      var source = @"shared[array[x + 1]] = 1;";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = x + 1
DECL $temp_1
$temp_1 = array[$temp_0]
shared[$temp_1] = 1";

      Assert.AreEqual(5, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedReadAndWriteArrayAccessesWithComplexExpressions() {
      var source = @"array[x + 10] = array[y * array[x] + 10]";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = x + 10
DECL $temp_1
$temp_1 = array[x]
DECL $temp_2
$temp_2 = y * $temp_1
DECL $temp_3
$temp_3 = $temp_2 + 10
array[$temp_0] = array[$temp_3]";

      Assert.AreEqual(9, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ArrayAccessesWithBinaryExpressions() {
      var source = @"array[x + 10] = array[x + 10]";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = x + 10
DECL $temp_1
$temp_1 = x + 10
array[$temp_0] = array[$temp_1]";

      Assert.AreEqual(5, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ComplexCondition() {
      var source = @"if(x/10 + x%5 - 10 > 0.0) { x++; }";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = x / 10
DECL $temp_1
$temp_1 = x % 5
DECL $temp_2
$temp_2 = $temp_0 + $temp_1
DECL $temp_3
$temp_3 = $temp_2 - 10
IF $temp_3 > 0 JUMP #0
JUMP #1
LABEL #0
x = x + 1
LABEL #1";

      Assert.AreEqual(13, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ConditionalExpressionAssignment() {
      var source = @"var result = a == b ? 1 : 0;";
      var code = _CreateCode(source);
      var expected = @"
DECL result
DECL $temp_0
IF a == b JUMP #0
$temp_0 = 0
JUMP #1
LABEL #0
$temp_0 = 1
LABEL #1
result = $temp_0";

      Assert.AreEqual(9, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ConditionalExpressionInsideIfStatement() {
      var source = @"if((a == b ? 1 : 0) > 0) {}";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
IF a == b JUMP #0
$temp_0 = 0
JUMP #1
LABEL #0
$temp_0 = 1
LABEL #1
IF $temp_0 > 0 JUMP #2
JUMP #3
LABEL #2
LABEL #3";

      Assert.AreEqual(11, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void ConditionalExpressionAsArrayAccessor() {
      var source = @"var x = array[a ? 1 : 0];";
      var code = _CreateCode(source);
      var expected = @"
DECL x
DECL $temp_0
IF a JUMP #0
$temp_0 = 0
JUMP #1
LABEL #0
$temp_0 = 1
LABEL #1
x = array[$temp_0]";

      Assert.AreEqual(9, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NestedConditionalExpressions() {
      var source = @"var x = a > 0 ? 1 : (a < 0 ? 1 : 0);";
      var code = _CreateCode(source);
      var expected = @"
DECL x
DECL $temp_0
IF a > 0 JUMP #0
DECL $temp_1
IF a < 0 JUMP #1
$temp_1 = 0
JUMP #2
LABEL #1
$temp_1 = 1
LABEL #2
$temp_0 = $temp_1
JUMP #3
LABEL #0
$temp_0 = 1
LABEL #3
x = $temp_0";

      Assert.AreEqual(16, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NegativeConstantDeclaration() {
      var source = @"var x = -10;";
      var code = _CreateCode(source);
      var expected = @"
DECL x
x = -10";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void NegativeVariableAssignment() {
      var source = @"x = -y;";
      var code = _CreateCode(source);
      var expected = @"x = -y";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MultiplicationOfNegativeConstants() {
      var source = @"var x = -10 * -20;";
      var code = _CreateCode(source);
      var expected = @"
DECL x
DECL $temp_0
$temp_0 = -10
DECL $temp_1
$temp_1 = -20
x = $temp_0 * $temp_1";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MultiplicationOfPositiveAndNegativeConstant() {
      var source = @"var x = 10 * -20;";
      var code = _CreateCode(source);
      var expected = @"
DECL x
DECL $temp_0
$temp_0 = -20
x = 10 * $temp_0";

      Assert.AreEqual(4, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DirectMethodInvocationWithAddExpressionAsArgument() {
      var source = @"Method(1 + 3);";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = 1 + 3
INVOKE Method($temp_0)";

      Assert.AreEqual(3, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void AssignmentWithDirectMethodInvocation() {
      var source = @"var result = Method(2 + x);";
      var code = _CreateCode(source);
      var expected = @"
DECL result
DECL $temp_0
$temp_0 = 2 + x
result = Method($temp_0)";

      Assert.AreEqual(4, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void AssignmentWithInvocationAsArrayAccessor() {
      var source = @"array[Position1(array[0])] = 3 - array[Position2(i)];";
      var code = _CreateCode(source);
      var expected = @"
DECL $temp_0
$temp_0 = array[0]
DECL $temp_1
$temp_1 = Position1($temp_0)
DECL $temp_2
$temp_2 = Position2(i)
DECL $temp_3
$temp_3 = array[$temp_2]
array[$temp_1] = 3 - $temp_3";

      Assert.AreEqual(9, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }
  }
}
