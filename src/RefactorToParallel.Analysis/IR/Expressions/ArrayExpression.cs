using System.Collections.Generic;

namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents an array expression.
  /// </summary>
  public class ArrayExpression : Expression {
    /// <summary>
    /// Gets the name of the array represented by this expression.
    /// </summary>
    public string Name { get; } // TODO use VariableExpression instead?

    /// <summary>
    /// Gets the expressions that access each dimension of the array.
    /// </summary>
    public IReadOnlyList<Expression> Accessors { get; }

    /// <summary>
    /// Creates a new array expression.
    /// </summary>
    /// <param name="name">The name of the variable represented by this expression.</param>
    /// <param name="accessors">The expressions that access the dimensions of the array.</param>
    public ArrayExpression(string name, IReadOnlyList<Expression> accessors) {
      Name = name;
      Accessors = accessors;
    }

    public override string ToString() {
      return $"{Name}[{string.Join(", ", Accessors)}]";
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
