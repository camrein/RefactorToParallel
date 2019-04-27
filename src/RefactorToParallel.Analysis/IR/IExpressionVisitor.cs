using RefactorToParallel.Analysis.IR.Expressions;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Visits the different kinds of expressions.
  /// </summary>
  public interface IExpressionVisitor {
    /// <summary>
    /// Visits an expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(Expression expression);

    /// <summary>
    /// Visits an add expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(AddExpression expression);

    /// <summary>
    /// Visits a multiply expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(MultiplyExpression expression);

    /// <summary>
    /// Visits a subtract expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(SubtractExpression expression);

    /// <summary>
    /// Visits a divide expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(DivideExpression expression);

    /// <summary>
    /// Visits a modulo expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(ModuloExpression expression);

    /// <summary>
    /// Visits a generic binary expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(GenericBinaryExpression expression);

    /// <summary>
    /// Visits an expression with parentheses.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(ParenthesesExpression expression);

    /// <summary>
    /// Visits a comparison expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(ComparisonExpression expression);

    /// <summary>
    /// Visits a variable expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(VariableExpression expression);

    /// <summary>
    /// Visits an integer expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(IntegerExpression expression);

    /// <summary>
    /// Visits a double expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(DoubleExpression expression);

    /// <summary>
    /// Visits a generic literal expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(GenericLiteralExpression expression);

    /// <summary>
    /// Visits an unary minus expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(UnaryMinusExpression expression);

    /// <summary>
    /// Visits an array expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(ArrayExpression expression);

    /// <summary>
    /// Visits a conditional expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(ConditionalExpression expression);

    /// <summary>
    /// Visits an invocation expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    void Visit(InvocationExpression expression);
  }

  /// <summary>
  /// Visits the different kinds of expressions.
  /// </summary>
  public interface IExpressionVisitor<out TResult> {
    /// <summary>
    /// Visits an expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(Expression expression);

    /// <summary>
    /// Visits an add expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(AddExpression expression);

    /// <summary>
    /// Visits a multiply expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(MultiplyExpression expression);

    /// <summary>
    /// Visits a subtract expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(SubtractExpression expression);

    /// <summary>
    /// Visits a divide expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(DivideExpression expression);

    /// <summary>
    /// Visits a modulo expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(ModuloExpression expression);

    /// <summary>
    /// Visits a generic binary expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(GenericBinaryExpression expression);

    /// <summary>
    /// Visits an expression with parentheses.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(ParenthesesExpression expression);

    /// <summary>
    /// Visits a comparison expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(ComparisonExpression expression);

    /// <summary>
    /// Visits a variable expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(VariableExpression expression);

    /// <summary>
    /// Visits an integer expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(IntegerExpression expression);

    /// <summary>
    /// Visits a double expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(DoubleExpression expression);

    /// <summary>
    /// Visits a generic literal expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(GenericLiteralExpression expression);

    /// <summary>
    /// Visits an unary minus expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(UnaryMinusExpression expression);

    /// <summary>
    /// Visits an array expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(ArrayExpression expression);

    /// <summary>
    /// Visits a conditional expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(ConditionalExpression expression);

    /// <summary>
    /// Visits an invocation expression.
    /// </summary>
    /// <param name="expression">Expression to visit.</param>
    /// <returns>The result after visiting the expression.</returns>
    TResult Visit(InvocationExpression expression);
  }
}
