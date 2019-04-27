using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class ExpressionExtensionsTest : ExpressionTestBase {
    [TestMethod]
    public void DescendantsOfSingleExpressionIsEmpty() {
      var expression = CreateExpression("1");
      Assert.AreEqual(0, expression.Descendants().Count());
    }

    [TestMethod]
    public void DescendantsAndSelfOfSingleExpressionContainsOnlyItself() {
      var expression = CreateExpression("1");
      Assert.AreEqual(1, expression.DescendantsAndSelf().Count());
      Assert.AreEqual(expression, expression.DescendantsAndSelf().Single());
    }

    [TestMethod]
    public void SimpleBinaryExpressionHasTwoDescendants() {
      var expression = CreateExpression("x + 5");
      var descendants = expression.Descendants().ToList();

      Assert.AreEqual(2, descendants.Count);
      Assert.IsTrue(descendants.Any(e => e is VariableExpression variable && variable.Name.Equals("x")));
      Assert.IsTrue(descendants.Any(e => e is IntegerExpression integer && integer.Value == 5));
    }

    [TestMethod]
    public void ExpressionsAreOrderedFromInnerToOuter() {
      var expression = CreateExpression("(x + 5) - Method()");
      var expressions = expression.DescendantsAndSelf().ToList();

      Assert.AreEqual(6, expressions.Count);
      Assert.IsTrue(expressions[0] is VariableExpression variable && variable.Name.Equals("x"));
      Assert.IsTrue(expressions[1] is IntegerExpression integer && integer.Value == 5);
      Assert.IsTrue(expressions[2] is AddExpression);
      Assert.IsTrue(expressions[3] is ParenthesesExpression);
      Assert.IsTrue(expressions[4] is InvocationExpression invocation && invocation.Name.Equals("Method"));
      Assert.IsTrue(expressions[5] is SubtractExpression);
    }

    [TestMethod]
    public void NestedInvocationsAreOrderedFromInnerToOuter() {
      var expression = CreateExpression("Outer(Nested1(Inner()), Nested2())");
      var expressions = expression.DescendantsAndSelf().ToList();

      Assert.AreEqual(4, expressions.Count);
      Assert.IsTrue(expressions[0] is InvocationExpression innnerInvocation && innnerInvocation.Name.Equals("Inner"));
      Assert.IsTrue(expressions[1] is InvocationExpression nested1Invocation && nested1Invocation.Name.Equals("Nested1"));
      Assert.IsTrue(expressions[2] is InvocationExpression nested2Invocation && nested2Invocation.Name.Equals("Nested2"));
      Assert.IsTrue(expressions[3] is InvocationExpression outerInvocation && outerInvocation.Name.Equals("Outer") && expression == expressions[3]);
    }

    [TestMethod]
    public void DescendantsAndSelfOfMultiplyNestedExpression() {
      var expression = CreateExpression("array[x, 5] + (4 << 2) - Method(1, Nested(10))");
      var expressions = expression.DescendantsAndSelf().ToList();
      Assert.IsTrue(expressions.Contains(expression));

      Assert.AreEqual(13, expressions.Count);
      Assert.IsTrue(expressions.Any(e => e is ArrayExpression array && array.Name.Equals("array")));
      Assert.IsTrue(expressions.Any(e => e is VariableExpression variable && variable.Name.Equals("x")));
      Assert.IsTrue(expressions.Any(e => e is IntegerExpression integer && integer.Value == 5));

      Assert.IsTrue(expressions.Any(e => e is AddExpression));

      Assert.IsTrue(expressions.Any(e => e is ParenthesesExpression));
      Assert.IsTrue(expressions.Any(e => e is GenericBinaryExpression generic && generic.Operator.Equals("<<")));
      Assert.IsTrue(expressions.Any(e => e is IntegerExpression integer && integer.Value == 4));
      Assert.IsTrue(expressions.Any(e => e is IntegerExpression integer && integer.Value == 2));

      Assert.IsTrue(expressions.Any(e => e is SubtractExpression));

      Assert.IsTrue(expressions.Any(e => e is InvocationExpression invocation && invocation.Name.Equals("Method")));
      Assert.IsTrue(expressions.Any(e => e is InvocationExpression invocation && invocation.Name.Equals("Nested")));
      Assert.IsTrue(expressions.Any(e => e is IntegerExpression integer && integer.Value == 1));
      Assert.IsTrue(expressions.Any(e => e is IntegerExpression integer && integer.Value == 10));
    }
  }
}
