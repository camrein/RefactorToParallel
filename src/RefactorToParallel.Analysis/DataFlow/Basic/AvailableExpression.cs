using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Core.Util;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.DataFlow.Basic {
  /// <summary>
  /// Represents an expression that is available at a certain point of the program's execution.
  /// </summary>
  public class AvailableExpression {
    // TODO ignore the order for operations like additions? But this needs an appropriate strategy for the hash codes.
    private readonly int _hashCode;

    /// <summary>
    /// Gets the node where the expression was defined.
    /// </summary>
    public FlowNode Node { get; }

    /// <summary>
    /// Gets the binary expression that is available.
    /// </summary>
    public Expression Expression { get; }

    /// <summary>
    /// Gets the variables being accessed by the binary expression represented by this instance.
    /// </summary>
    public ISet<string> AccessedVariables { get; }


    /// <summary>
    /// Creates a new available expression with the given binary expression.
    /// </summary>
    /// <param name="expression">The binary expression represented by this instance.</param>
    public AvailableExpression(FlowNode node, BinaryExpression expression) : this(node, (Expression)expression) { }

    /// <summary>
    /// Creates a new available expression with the given invocation expression.
    /// </summary>
    /// <param name="expression">The invocation expression represented by this instance.</param>
    public AvailableExpression(FlowNode node, InvocationExpression expression) : this(node, (Expression)expression) { }

    private AvailableExpression(FlowNode node, Expression expression) {
      Node = node;
      Expression = expression;
      var subExpressions = _GetSubExpressions(expression);
      AccessedVariables = subExpressions
        .OfType<VariableExpression>()
        .Select(variable => variable.Name)
        .ToImmutableHashSet();

      var hash = Hash.With(expression.GetType()).And(node);
      _hashCode = subExpressions.Select(_GetValue).Aggregate(hash, (memo, current) => memo.And(current)).Get();
    }

    /// <summary>
    /// Checks if the given expression is equal to the expression contained in this
    /// available expression instance.
    /// </summary>
    /// <param name="expression">The expression to check against.</param>
    /// <returns><code>True</code> if the expressions are equal.</returns>
    public bool EqualTo(Expression expression) {
      if(Expression.GetType() != expression.GetType()) {
        return false;
      }

      var expressions = _GetSubExpressions(Expression);
      var otherExpressions = _GetSubExpressions(expression);
      if(expressions.Count != otherExpressions.Count) {
        return false;
      }

      if(expression is InvocationExpression otherInvocation) {
        var invocation = (InvocationExpression)Expression;
        if(!Equals(invocation.Name, otherInvocation.Name)) {
          return false;
        }
      }

      return !expressions.Where((sub, index) => !_AreEqual(sub, otherExpressions[index])).Any();
    }

    public override string ToString() {
      return $"{Node.GetHashCode()}: {Expression}";
    }

    public override bool Equals(object obj) {
      var other = obj as AvailableExpression;
      if(other == null) {
        return false;
      }

      return Node == other.Node && EqualTo(other.Expression);
    }

    private IReadOnlyList<Expression> _GetSubExpressions(Expression expression) {
      switch(expression) {
      case BinaryExpression binary:
        return new[] { binary.Left, binary.Right };
      case InvocationExpression invocation:
        return invocation.Arguments;
      default:
        throw new UnsupportedSyntaxException($"expression {expression} not supported here");
      }
    }
    
    public override int GetHashCode() {
      return _hashCode;
    }

    private static bool _AreEqual(Expression a, Expression b) {
      return Equals(_GetValue(a), _GetValue(b));
    }

    private static object _GetValue(Expression expression) {
      switch(expression) {
      case VariableExpression variable:
        return variable.Name;
      case IntegerExpression integer:
        return integer.Value;
      default:
        // TODO maybe prevent the use of different expressions beforehand?
        // but this introduces additional complexity into the analysis.
        return expression;
      }
    }
  }
}

