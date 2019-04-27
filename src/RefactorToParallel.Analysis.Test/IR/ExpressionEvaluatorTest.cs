using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class ExpressionEvaluatorTest : ExpressionTestBase {
    [TestMethod]
    public void EvaluateAddConstants() {
      var expression = CreateExpression("1 + 5 + 4 + 10").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(20, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateSubtractConstants() {
      var expression = CreateExpression("10 - 6").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(4, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateAddAndMultiplyConstantsLeft() {
      var expression = CreateExpression("1 * 5 + 10").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(15, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateAddAndMultiplyConstantsRight() {
      var expression = CreateExpression("2 + 6 * 5").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(32, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateAddAndMultiplyConstantsWithParentheses() {
      var expression = CreateExpression("(2 + 6) * 5").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(40, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateAddWithUnknownVariable() {
      var expression = CreateExpression("3 + x").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(AddExpression));

      var add = (AddExpression)expression;
      Assert.AreEqual(3, ((IntegerExpression)add.Left).Value);
      Assert.AreEqual("x", ((VariableExpression)add.Right).Name);
    }

    [TestMethod]
    public void EvaluateLargeExpressionWithSingleUnknownVariable() {
      // Result: (((35 + x) + 15) - 10)
      var expression = CreateExpression("5 + 10 * 3 + x + 5 * 3 - 10").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(SubtractExpression));

      var subtract = (SubtractExpression)expression;
      Assert.IsInstanceOfType(subtract.Left, typeof(AddExpression));
      Assert.AreEqual(10, ((IntegerExpression)subtract.Right).Value);

      var outerAdd = (AddExpression)subtract.Left;
      Assert.IsInstanceOfType(outerAdd.Left, typeof(AddExpression));
      Assert.AreEqual(15, ((IntegerExpression)outerAdd.Right).Value);

      var innerAdd = (AddExpression)outerAdd.Left;
      Assert.AreEqual(35, ((IntegerExpression)innerAdd.Left).Value);
      Assert.AreEqual("x", ((VariableExpression)innerAdd.Right).Name);
    }

    [TestMethod]
    public void EvaluateMultiplyWithGivenVariable() {
      var x = CreateExpression("5 + 3");
      var expression = CreateExpression("10 * x").TryEvaluate(new Dictionary<string, Expression> { { "x", x } });
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(80, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateWithPartiallyGivenVariable() {
      // Result: (30 * (8 + i))
      var value = CreateExpression("5 + 3 + i");
      var expression = CreateExpression("3 * 10 * value").TryEvaluate(new Dictionary<string, Expression> { { "value", value } });
      Assert.IsInstanceOfType(expression, typeof(MultiplyExpression));

      var multiply = (MultiplyExpression)expression;
      Assert.AreEqual(30, ((IntegerExpression)multiply.Left).Value);

      Assert.IsInstanceOfType(multiply.Right, typeof(AddExpression));
      var add = (AddExpression)multiply.Right;
      Assert.AreEqual(8, ((IntegerExpression)add.Left).Value);
      Assert.AreEqual("i", ((VariableExpression)add.Right).Name);
    }

    [TestMethod]
    public void EvaluateArrayAccessors() {
      // Result: array[0,((50 * i) + 3)]
      var expression = CreateExpression("array[0, 10 * 5 * i + 3]").TryEvaluate();
      Debug.WriteLine(expression);
      Assert.IsInstanceOfType(expression, typeof(ArrayExpression));

      var array = (ArrayExpression)expression;
      Assert.AreEqual("array", array.Name);
      Assert.AreEqual(2, array.Accessors.Count);
      Assert.AreEqual(0, ((IntegerExpression)array.Accessors[0]).Value);

      Assert.IsInstanceOfType(array.Accessors[1], typeof(AddExpression));
      var add = (AddExpression)array.Accessors[1];
      Assert.AreEqual(3, ((IntegerExpression)add.Right).Value);

      Assert.IsInstanceOfType(add.Left, typeof(MultiplyExpression));
      var multiply = (MultiplyExpression)add.Left;
      Assert.AreEqual(50, ((IntegerExpression)multiply.Left).Value);
      Assert.AreEqual("i", ((VariableExpression)multiply.Right).Name);
    }

    //[TestMethod]
    //[ExpectedException(typeof(UnsupportedSyntaxException))]
    //public void EvaluateAdditionWithOverflow() {
    //  var expression = CreateExpression("9223372036854775807 + 10");
    //}

    [TestMethod]
    public void EvaluateAdditionResultingInMaxValue() {
      var expression = CreateExpression("9223372036854775806 + 1").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(9223372036854775807, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluatePartialMultiplicationWithRightZero() {
      var expression = CreateExpression("x * 0").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(0, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluatePartialMultiplicationWithLeftZero() {
      var expression = CreateExpression("0 * x").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(0, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluatePartialMultiplicationWithRightOne() {
      var expression = CreateExpression("n * 1").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("n", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void EvaluatePartialMultiplicationWithLeftOne() {
      var expression = CreateExpression("1 * y").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("y", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void EvaluatePartialAdditionOfRightZero() {
      var expression = CreateExpression("k + 0").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("k", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void EvaluatePartialAdditionOfLeftZero() {
      var expression = CreateExpression("0 + z").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("z", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void EvaluatePartialSubtractionOfRightZero() {
      var expression = CreateExpression("a - 0").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("a", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void EvaluatePartialSubtractionOfLeftZero() {
      var expression = CreateExpression("0 - b").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(VariableExpression));
      Assert.AreEqual("b", ((VariableExpression)expression).Name);
    }

    [TestMethod]
    public void EvaluateDivisionOfZero() {
      var expression = CreateExpression("0 * y").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(0, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateDivisionByZero() {
      var expression = CreateExpression("10 / 0").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(DivideExpression));
    }

    [TestMethod]
    public void EvaluateModuloOfZero() {
      var expression = CreateExpression("0 % y").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(0, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void EvaluateModuloByZero() {
      var expression = CreateExpression("y % 0").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(ModuloExpression));
    }

    [TestMethod]
    public void NegatePositiveInteger() {
      var expression = CreateExpression("-10").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(IntegerExpression));
      Assert.AreEqual(-10, ((IntegerExpression)expression).Value);
    }

    [TestMethod]
    public void NegateUnknownVariable() {
      var expression = CreateExpression("-x").TryEvaluate();
      Assert.IsInstanceOfType(expression, typeof(UnaryMinusExpression));
    }
  }
}
