using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactoryBasicTest : CodeFactoryTestBase {
    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void UnsupportedStatement() {
      var source = @"goto x;";
      CreateCode(source);
    }

    [TestMethod]
    public void EmptyBlock() {
      var source = @"";
      var code = CreateCode(source);
      var expected = @"";

      Assert.AreEqual(0, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void SingleDeclaration() {
      var source = @"var variable = 1;";
      var code = CreateCode(source);
      var expected = @"
DECL variable
variable = 1";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void SingleDeclarationWithoutInitializer() {
      var source = @"int variable;";
      var code = CreateCode(source);
      var expected = @"DECL variable";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MultipleDeclarationsInOneStatement() {
      var source = @"int a = 1, b = 3, c = 5;";
      var code = CreateCode(source);
      var expected = @"
DECL a
a = 1
DECL b
b = 3
DECL c
c = 5";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void SingleAssignment() {
      var source = @"variable = 2;";
      var code = CreateCode(source);
      var expected = @"variable = 2";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithAssignment() {
      var source = @"var variable = 2; variable = 3;";
      var code = CreateCode(source);
      var expected = @"
DECL variable
variable = 2
variable = 3";

      Assert.AreEqual(3, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void PostfixIncrement() {
      var source = @"i++;";
      var code = CreateCode(source);
      var expected = @"i = i + 1";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      var assignment = code.Root.OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(AddExpression));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    // TODO needs to create temporary variable
    //[TestMethod]
    //public void PrefixDecrement() {
    //  var source = @"--i;";
    //  var code = CreateCode(source);
    //  var expected = @"
    //digraph cfg {
    //  ""0"" [ label = ""<Start>"" ];
    //  ""1"" [ label = ""<End>"" ];
    //  ""2"" [ label = ""Assignment: i = i - 1"" ];
    //  ""0"" -> ""2"" [ label = """" ];
    //  ""2"" -> ""1"" [ label = """" ];
    //}";

    //  Assert.AreEqual(3, cfg.Nodes.Count);
    //  Assert.AreEqual(2, cfg.Edges.Count);
    //  var assignment = cfg.Nodes.OfType<Assignment>().Single();
    //  Assert.IsInstanceOfType(assignment.Right, typeof(SubtractExpression));
    //  Assert.AreEqual(expected.Trim(), cfg.ToDot());
    //}

    [TestMethod]
    public void AddAssignment() {
      var source = @"a += 2;";
      var code = CreateCode(source);
      var expected = @"a = a + 2";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      var assignment = code.Root.OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(AddExpression));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void SubtractAssignment() {
      var source = @"n -= 3;";
      var code = CreateCode(source);
      var expected = @"n = n - 3";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      var assignment = code.Root.OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(SubtractExpression));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MultiplyAssignment() {
      var source = @"x *= 5;";
      var code = CreateCode(source);
      var expected = @"x = x * 5";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      var assignment = code.Root.OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(MultiplyExpression));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void AssignmentOfNegativeNumber() {
      var source = @"x = -5;";
      var code = CreateCode(source);
      var expected = @"x = -5";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      var assignment = code.Root.OfType<Assignment>().Single();

      Assert.IsInstanceOfType(assignment.Right, typeof(UnaryMinusExpression));
      var minus = (UnaryMinusExpression)assignment.Right;

      Assert.IsInstanceOfType(minus.Expression, typeof(IntegerExpression));
      Assert.AreEqual(5, ((IntegerExpression)minus.Expression).Value);


      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithConditionalExpression() {
      var source = @"var variable = a > 0 ? a : 0;";
      var code = CreateCode(source);
      var expected = @"
DECL variable
variable = a > 0 ? a : 0";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithBooleanConstant() {
      var source = @"var variable = true;";
      var code = CreateCode(source);
      var expected = @"
DECL variable
variable = \literal";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DeclarationWithStringLiteral() {
      var source = @"var variable = """";";
      var code = CreateCode(source);
      var expected = @"
DECL variable
variable = \literal";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void StringDeclarationWithNullCheck() {
      var source = @"var variable = ""test""; if(variable != null) {}";
      var code = CreateCode(source);
      var expected = @"
DECL variable
variable = \literal
IF variable != \literal JUMP #0
JUMP #1
LABEL #0
LABEL #1";

      Assert.AreEqual(6, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DirectInvocationWithoutArguments() {
      var source = @"Method();";
      var code = CreateCode(source);
      var expected = @"INVOKE Method()";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void DirectInvocationWithArguments() {
      var source = @"Method(1 + 1, a, x, 4 * b);";
      var code = CreateCode(source);
      var expected = @"INVOKE Method(1 + 1, a, x, 4 * b)";

      Assert.AreEqual(1, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void InvocationNestedInAssignment() {
      var source = @"var result = First(x) + Second(1, 2);";
      var code = CreateCode(source);
      var expected = @"
DECL result
result = First(x) + Second(1, 2)";

      Assert.AreEqual(2, InstructionCounter.Count(code));
      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ReturnStatementIsProhibited() {
      var source = @"return;";
      CreateCode(source);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ContinueStatementWithoutLoopIsProhibited() {
      var source = @"continue;";
      CreateCode(source);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void BreakStatementWithoutLoopIsProhibited() {
      var source = @"break;";
      CreateCode(source);
    }
  }
}
