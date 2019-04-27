using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class ExpressionFactoryTest : ExpressionTestBase {
    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void UseOfUnsupportedExpression() {
      CreateExpression("x++");
    }

    [TestMethod]
    public void SingleConstant() {
      var expression = CreateExpression("1");
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(1, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void SingleVariable() {
      var expression = CreateExpression("y");
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("y", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void FloatConstantCreatesDoubleExpression() {
      var expression = CreateExpression("1F");
      Assert.IsInstanceOfType(expression, typeof(DoubleExpression));
    }

    [TestMethod]
    public void DoubleConstantCreatesDoubleExpression() {
      var expression = CreateExpression("1.0");
      Assert.IsInstanceOfType(expression, typeof(DoubleExpression));
    }

    [TestMethod]
    public void AdditionCreatesAddExpression() {
      var expression = CreateExpression("a + b");
      Assert.IsInstanceOfType(expression, typeof(AddExpression));
    }

    [TestMethod]
    public void SubtractionCreatesSubtractExpression() {
      var expression = CreateExpression("a - b");
      Assert.IsInstanceOfType(expression, typeof(SubtractExpression));
    }

    [TestMethod]
    public void MultiplicationCreatesMultiplyExpression() {
      var expression = CreateExpression("a * b");
      Assert.IsInstanceOfType(expression, typeof(MultiplyExpression));
    }

    [TestMethod]
    public void DivisionCreatesDivideExpression() {
      var expression = CreateExpression("a / b");
      Assert.IsInstanceOfType(expression, typeof(DivideExpression));
    }

    [TestMethod]
    public void ModuloCreatesModuloExpression() {
      var expression = CreateExpression("a % b");
      Assert.IsInstanceOfType(expression, typeof(ModuloExpression));
    }

    [TestMethod]
    public void ArrayAccessWithIntegerConstant() {
      var expression = CreateExpression("array[5]");
      Assert.IsInstanceOfType(expression, typeof(ArrayExpression));

      var array = (ArrayExpression)expression;
      Assert.AreEqual("array", array.Name);
      Assert.AreEqual(1, array.Accessors.Count);
      Assert.AreEqual(5, ((IntegerExpression)array.Accessors[0]).Value);
    }

    [TestMethod]
    public void ArrayAccessWithVariable() {
      var expression = CreateExpression("array[arg]");
      Assert.IsInstanceOfType(expression, typeof(ArrayExpression));

      var array = (ArrayExpression)expression;
      Assert.AreEqual("array", array.Name);
      Assert.AreEqual(1, array.Accessors.Count);
      Assert.AreEqual("arg", ((VariableExpression)array.Accessors[0]).Name);
    }

    [TestMethod]
    public void AddWithIntegerConstantAndVariable() {
      var expression = CreateExpression("y + 5");
      Assert.IsInstanceOfType(expression, typeof(AddExpression));

      var add = (AddExpression)expression;
      Assert.AreEqual("y", ((VariableExpression)add.Left).Name);
      Assert.AreEqual(5, ((IntegerExpression)add.Right).Value);
    }

    [TestMethod]
    public void MultiDimensionalArrayAccess() {
      var expression = CreateExpression("buffer[i, 5, 4]");
      Assert.IsInstanceOfType(expression, typeof(ArrayExpression));

      var array = (ArrayExpression)expression;
      Assert.AreEqual("buffer", array.Name);
      Assert.AreEqual(3, array.Accessors.Count);
      Assert.AreEqual("i", ((VariableExpression)array.Accessors[0]).Name);
      Assert.AreEqual(5, ((IntegerExpression)array.Accessors[1]).Value);
      Assert.AreEqual(4, ((IntegerExpression)array.Accessors[2]).Value);
    }

    [TestMethod]
    public void ArrayAccessWithAddExpression() {
      var expression = CreateExpression("buffer[i + 1]");
      Assert.IsInstanceOfType(expression, typeof(ArrayExpression));

      var array = (ArrayExpression)expression;
      Assert.AreEqual("buffer", array.Name);
      Assert.AreEqual(1, array.Accessors.Count);

      Assert.IsInstanceOfType(array.Accessors[0], typeof(AddExpression));
      var add = (AddExpression)array.Accessors[0];
      Assert.AreEqual("i", ((VariableExpression)add.Left).Name);
      Assert.AreEqual(1, ((IntegerExpression)add.Right).Value);
    }

    [TestMethod]
    public void CastsAreTruncated() {
      var expression = CreateExpression("(int)1.0");
      Assert.IsInstanceOfType(expression, typeof(DoubleExpression));
    }

    [TestMethod]
    public void ConditionalExpression() {
      var expression = CreateExpression("a == b ? 1 : 0");
      Assert.IsInstanceOfType(expression, typeof(ConditionalExpression));

      var conditional = (ConditionalExpression)expression;
      Assert.IsInstanceOfType(conditional.Condition, typeof(ComparisonExpression));

      Assert.IsInstanceOfType(conditional.WhenTrue, typeof(IntegerExpression));
      Assert.AreEqual(1, ((IntegerExpression)conditional.WhenTrue).Value);

      Assert.IsInstanceOfType(conditional.WhenFalse, typeof(IntegerExpression));
      Assert.AreEqual(0, ((IntegerExpression)conditional.WhenFalse).Value);
    }

    [TestMethod]
    public void CoalesceExpression() {
      var expression = CreateExpression("a ?? b");
      Assert.IsInstanceOfType(expression, typeof(ConditionalExpression));
      var conditional = (ConditionalExpression)expression;

      Assert.IsInstanceOfType(conditional.Condition, typeof(ComparisonExpression));
      var condition = (ComparisonExpression)conditional.Condition;
      Assert.AreEqual("!=", condition.Operator);
      Assert.IsInstanceOfType(condition.Left, typeof(VariableExpression));
      Assert.IsInstanceOfType(condition.Right, typeof(GenericLiteralExpression));

      Assert.IsInstanceOfType(conditional.WhenTrue, typeof(VariableExpression));
      Assert.AreEqual("a", ((VariableExpression)conditional.WhenTrue).Name);

      Assert.IsInstanceOfType(conditional.WhenFalse, typeof(VariableExpression));
      Assert.AreEqual("b", ((VariableExpression)conditional.WhenFalse).Name);
    }

    [TestMethod]
    public void LogicalNotIsTransformedIntoComparison() {
      var expression = CreateExpression("!a");
      Assert.IsInstanceOfType(expression, typeof(ComparisonExpression));
      var comparison = (ComparisonExpression)expression;

      Assert.IsInstanceOfType(comparison.Right, typeof(GenericLiteralExpression));
      Assert.IsInstanceOfType(comparison.Left, typeof(VariableExpression));
      Assert.AreEqual("a", ((VariableExpression)comparison.Left).Name);
    }

    [TestMethod]
    public void SimpleInvocation() {
      var source = @"Method(x, 1)";
      var expression = CreateExpression(source);

      Assert.IsInstanceOfType(expression, typeof(InvocationExpression));
      var invocation = (InvocationExpression)expression;
      Assert.AreEqual(2, invocation.Arguments.Count);
      Assert.AreEqual("Method", invocation.Name);

      Assert.IsInstanceOfType(invocation.Arguments[0], typeof(VariableExpression));
      Assert.AreEqual("x", ((VariableExpression)invocation.Arguments[0]).Name);
      Assert.IsInstanceOfType(invocation.Arguments[1], typeof(IntegerExpression));
      Assert.AreEqual(1, ((IntegerExpression)invocation.Arguments[1]).Value);
    }

    [TestMethod]
    public void NestedInvocations() {
      var source = @"Outer(Nested1(1), Nested2())";
      var expression = CreateExpression(source);

      Assert.IsInstanceOfType(expression, typeof(InvocationExpression));
      var invocation = (InvocationExpression)expression;
      Assert.AreEqual(2, invocation.Arguments.Count);
      Assert.AreEqual("Outer", invocation.Name);

      Assert.IsInstanceOfType(invocation.Arguments[0], typeof(InvocationExpression));
      var nested1 = (InvocationExpression)invocation.Arguments[0];
      Assert.AreEqual(1, nested1.Arguments.Count);
      Assert.AreEqual("Nested1", nested1.Name);

      Assert.IsInstanceOfType(invocation.Arguments[1], typeof(InvocationExpression));
      var nested2 = (InvocationExpression)invocation.Arguments[1];
      Assert.AreEqual(0, nested2.Arguments.Count);
      Assert.AreEqual("Nested2", nested2.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void InvocationWithRefArgumentIsProhibited() {
      var source = @"Method(ref x);";
      CreateExpression(source);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void InvocationWithOutArgumentIsProhibited() {
      var source = @"Method(out x);";
      CreateExpression(source);
    }

    [TestMethod]
    public void UnusualBinaryExpressionIsSupported() {
      var expression = CreateExpression("a << b");
      Assert.IsInstanceOfType(expression, typeof(GenericBinaryExpression));

      var binary = (GenericBinaryExpression)expression;
      Assert.AreEqual("<<", binary.Operator);
      Assert.AreEqual("a", binary.Left.ToString());
      Assert.AreEqual("b", binary.Right.ToString());
    }

    [TestMethod]
    public void StringInterpolationWithEmptyString() {
      var expression = CreateExpression("$\"\"");
      Assert.IsInstanceOfType(expression, typeof(GenericLiteralExpression));
    }

    [TestMethod]
    public void StringInterpolationWithoutInterpolation() {
      var expression = CreateExpression("$\"test\"");
      Assert.IsInstanceOfType(expression, typeof(GenericLiteralExpression));
    }

    [TestMethod]
    public void InterpolatedStringIsTransformedIntoConcatenation() {
      var expression = CreateExpression("$\"test {i} with element {array[i]}\"");
      Assert.IsInstanceOfType(expression, typeof(AddExpression));

      var root = (AddExpression)expression;
      Assert.IsInstanceOfType(root.Left, typeof(AddExpression));
      Assert.IsInstanceOfType(root.Right, typeof(ArrayExpression));
      Assert.AreEqual("array", ((ArrayExpression)root.Right).Name);

      var first = (AddExpression)root.Left;
      Assert.IsInstanceOfType(first.Left, typeof(AddExpression));
      Assert.IsInstanceOfType(first.Right, typeof(GenericLiteralExpression));

      var second = (AddExpression)first.Left;
      Assert.IsInstanceOfType(second.Left, typeof(GenericLiteralExpression));
      Assert.IsInstanceOfType(second.Right, typeof(VariableExpression));
      Assert.AreEqual("i", ((VariableExpression)second.Right).Name);
    }

    [TestMethod]
    public void SupportsDefaultKeyword() {
      var expression = CreateExpression("default(int)");
      Assert.IsInstanceOfType(expression, typeof(GenericLiteralExpression));
    }

    [TestMethod]
    public void SupportsTypeofKeyword() {
      var expression = CreateExpression("typeof(int)");
      Assert.IsInstanceOfType(expression, typeof(GenericLiteralExpression));
    }

    [TestMethod]
    public void SupportsSizeofKeyword() {
      var expression = CreateExpression("sizeof(int)");
      Assert.IsInstanceOfType(expression, typeof(GenericLiteralExpression));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void PreventsCastsToArrayTypes() {
      CreateExpression("(int[])x");
    }

    [TestMethod]
    public void SupportsCastsToNonArrayTypes() {
      var expression = CreateExpression("(int)x");
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
    }
  }
}
