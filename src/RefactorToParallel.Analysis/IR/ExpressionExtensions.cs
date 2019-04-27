using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Extension method to work with expressions.
  /// </summary>
  public static class ExpressionExtensions {
    private static readonly Flatter _flatter = new Flatter();

    /// <summary>
    /// Gets all descendant expressions of the given expression.
    /// </summary>
    /// <param name="expression">The expression to get the descendants of.</param>
    /// <returns>The descendant expressions.</returns>
    public static IEnumerable<Expression> Descendants(this Expression expression) {
      return expression.Accept(_flatter);
    }

    /// <summary>
    /// Gets all descendant expressions of the given expression including the expression itself.
    /// </summary>
    /// <param name="expression">The expression to get the descendants of.</param>
    /// <returns>The descendant expressions including the expression itself.</returns>
    public static IEnumerable<Expression> DescendantsAndSelf(this Expression expression) {
      return _flatter.Visit(expression);
    }

    private class Flatter : IExpressionVisitor<IEnumerable<Expression>> {
      private static readonly IEnumerable<Expression> _emptyEnumerable = Enumerable.Empty<Expression>();

      public IEnumerable<Expression> Visit(Expression expression) {
        return expression.Accept(this).Concat(new[] { expression });
      }

      public IEnumerable<Expression> Visit(AddExpression expression) {
        return _VisitBinary(expression);
      }

      public IEnumerable<Expression> Visit(MultiplyExpression expression) {
        return _VisitBinary(expression);
      }

      public IEnumerable<Expression> Visit(SubtractExpression expression) {
        return _VisitBinary(expression);
      }

      public IEnumerable<Expression> Visit(DivideExpression expression) {
        return _VisitBinary(expression);
      }

      public IEnumerable<Expression> Visit(ModuloExpression expression) {
        return _VisitBinary(expression);
      }

      public IEnumerable<Expression> Visit(GenericBinaryExpression expression) {
        return _VisitBinary(expression);
      }

      public IEnumerable<Expression> Visit(ParenthesesExpression expression) {
        return Visit(expression.Expression);
      }

      public IEnumerable<Expression> Visit(ComparisonExpression expression) {
        return _VisitBinary(expression);
      }

      private IEnumerable<Expression> _VisitBinary(BinaryExpression expression) {
        return Visit(expression.Left).Concat(Visit(expression.Right));
      }

      public IEnumerable<Expression> Visit(VariableExpression expression) {
        return _emptyEnumerable;
      }

      public IEnumerable<Expression> Visit(IntegerExpression expression) {
        return _emptyEnumerable;
      }

      public IEnumerable<Expression> Visit(DoubleExpression expression) {
        return _emptyEnumerable;
      }

      public IEnumerable<Expression> Visit(GenericLiteralExpression expression) {
        return _emptyEnumerable;
      }

      public IEnumerable<Expression> Visit(UnaryMinusExpression expression) {
        return Visit(expression.Expression);
      }

      public IEnumerable<Expression> Visit(ArrayExpression expression) {
        return _VisitItems(expression.Accessors);
      }

      public IEnumerable<Expression> Visit(ConditionalExpression expression) {
        return Visit(expression.Condition)
          .Concat(Visit(expression.WhenTrue))
          .Concat(Visit(expression.WhenFalse));
      }

      public IEnumerable<Expression> Visit(InvocationExpression expression) {
        return _VisitItems(expression.Arguments);
      }

      private IEnumerable<Expression> _VisitItems(IEnumerable<Expression> items) {
        return items.SelectMany(Visit);
      }
    }
  }
}
