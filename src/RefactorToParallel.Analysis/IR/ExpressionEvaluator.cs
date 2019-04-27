using RefactorToParallel.Analysis.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Evaluates given expressions with respecting the current state of variables.
  /// </summary>
  public class ExpressionEvaluator : IExpressionVisitor<Expression> {
    private readonly IReadOnlyDictionary<string, Expression> _values;

    private ExpressionEvaluator(IReadOnlyDictionary<string, Expression> values) {
      _values = values;
    }
    
    /// <summary>
    /// Trys to evaluate a given expression as much as possible.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The evaluated and possibly smaller expression.</returns>
    public static Expression TryEvaluate(Expression expression) {
      return TryEvaluate(expression, new Dictionary<string, Expression>());
    }

    /// <summary>
    /// Trys to evaluate a given expression as much as possible by taking a given set of values
    /// for variables into account.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="values">The mappings from variable names to their current value.</param>
    /// <returns>The evaluated and possibly smaller expression.</returns>
    public static Expression TryEvaluate(Expression expression, IReadOnlyDictionary<string, Expression> values) {
      return new ExpressionEvaluator(values).Visit(expression);
    }

    public Expression Visit(Expression expression) {
      return expression.Accept(this);
    }

    public Expression Visit(AddExpression expression) {
      return _ApplyBinaryExpression(expression, 
        (left, right) => left + right,
        (left, right) => (left.Value == 0) ? right : new AddExpression(left, right),
        (left, right) => (right.Value == 0) ? left : new AddExpression(left, right),
        (left, right) => new AddExpression(left, right));
    }

    public Expression Visit(MultiplyExpression expression) {
      return _ApplyBinaryExpression(expression, 
        (left, right) => left * right,
        (left, right) => (left.Value == 0) ? new IntegerExpression(0) : (left.Value == 1 ? right : new MultiplyExpression(left, right)),
        (left, right) => (right.Value == 0) ? new IntegerExpression(0) : (right.Value == 1 ? left : new MultiplyExpression(left, right)),
        (left, right) => new MultiplyExpression(left, right));
    }

    public Expression Visit(SubtractExpression expression) {
      return _ApplyBinaryExpression(expression, 
        (left, right) => left - right,
        (left, right) => (left.Value == 0) ? right : new SubtractExpression(left, right),
        (left, right) => (right.Value == 0) ? left : new SubtractExpression(left, right),
        (left, right) => new SubtractExpression(left, right));
    }

    public Expression Visit(DivideExpression expression) {
      var left = expression.Left.Accept(this);
      if(left is IntegerExpression integer && integer.Value == 0) {
        return left;
      }

      return new DivideExpression(left, expression.Right.Accept(this));
    }

    public Expression Visit(ModuloExpression expression) {
      var left = expression.Left.Accept(this);
      if(left is IntegerExpression integer && integer.Value == 0) {
        return left;
      }

      return new ModuloExpression(left, expression.Right.Accept(this));
    }

    private Expression _ApplyBinaryExpression(BinaryExpression binary,
        Func<long, long, long> evaluation,
        Func<IntegerExpression, Expression, Expression> partialLeft,
        Func<Expression, IntegerExpression, Expression> partialRight,
        Func<Expression, Expression, Expression> constructor) {
      var left = binary.Left.Accept(this);
      var right = binary.Right.Accept(this);
      var leftInt = left as IntegerExpression;
      var rightInt = right as IntegerExpression;

      if(leftInt == null && rightInt == null) {
        return constructor(left, right);
      } else if(rightInt == null) {
        return partialLeft(leftInt, right);
      } else if(leftInt == null) {
        return partialRight(left, rightInt);
      }

      // TODO the overflow check is probably unnecessary as everything is made with long
      // But the project is about being sure that nothing goes wrong when parallelizing.
      // The "checked" keyword might be of use, but only throws exception if, for example,
      // a long outside of the range of an int is casted to such. As there is no bigger
      // than long, the checked can't be used. An alternative would be working only
      // on the integer range.
      return new IntegerExpression(evaluation(leftInt.Value, rightInt.Value));
    }

    public Expression Visit(GenericBinaryExpression expression) {
      return new GenericBinaryExpression(expression.Operator, expression.Left.Accept(this), expression.Right.Accept(this));
    }

    public Expression Visit(ParenthesesExpression expression) {
      // TODO when should the parantheses be removed?
      // TODO while the evaluation is still correct (due to the nesting of the nodes),
      //      the resulting expression is incorrect when printing it as a string
      return expression.Expression.Accept(this);
    }

    public Expression Visit(ComparisonExpression expression) {
      // TODO improve?
      return expression;
    }

    public Expression Visit(VariableExpression expression) {
      // TODO prevent recursion, these might happen with loops or similar.
      if(_values.TryGetValue(expression.Name, out var value)) {
        return value.Accept(this);
      }

      return expression;
    }

    public Expression Visit(IntegerExpression expression) {
      return expression;
    }

    public Expression Visit(DoubleExpression expression) {
      return expression;
    }
    public Expression Visit(GenericLiteralExpression expression) {
      return expression;
    }

    public Expression Visit(UnaryMinusExpression expression) {
      var evaluated = expression.Expression.Accept(this);
      if(evaluated is IntegerExpression integer) {
        return new IntegerExpression(-integer.Value);
      }
      return new UnaryMinusExpression(evaluated);
    }

    public Expression Visit(ArrayExpression expression) {
      return new ArrayExpression(expression.Name, expression.Accessors.Select(accessor => accessor.Accept(this)).ToArray());
    }


    public Expression Visit(ConditionalExpression expression) {
      return new ConditionalExpression(expression.Condition.Accept(this), expression.WhenTrue.Accept(this), expression.WhenFalse.Accept(this));
    }

    public Expression Visit(InvocationExpression expression) {
      return new InvocationExpression(expression.Name, expression.Arguments.Select(argument => argument.Accept(this)).ToArray());
    }
  }

  /// <summary>
  /// Extension methods for to evaluate expressions.
  /// </summary>
  public static class ExpressionEvaluatorExtensions {
    /// <summary>
    /// Trys to evaluate a given expression as much as possible.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The evaluated and possibly smaller expression.</returns>
    public static Expression TryEvaluate(this Expression expression) {
      return ExpressionEvaluator.TryEvaluate(expression);
    }

    /// <summary>
    /// Trys to evaluate a given expression as much as possible by taking a given set of values
    /// for variables into account.
    /// </summary>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="values">The mappings from variable names to their current value.</param>
    /// <returns>The evaluated and possibly smaller expression.</returns>
    public static Expression TryEvaluate(this Expression expression, IReadOnlyDictionary<string, Expression> values) {
      return ExpressionEvaluator.TryEvaluate(expression, values);
    }
  }
}
