using System;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.IR.Expressions {
  /// <summary>
  /// Represents an invocation expression.
  /// </summary>
  public class InvocationExpression : Expression {
    /// <summary>
    /// Gets the identifier used for return values.
    /// </summary>
    public string ResultIdentifier => GetResultIdentifier(Name);

    /// <summary>
    /// Gets the name of the invoked method.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the arguments passed to the invoked method.
    /// </summary>
    public IReadOnlyList<Expression> Arguments { get; }

    /// <summary>
    /// Creates a new array expression.
    /// </summary>
    /// <param name="name">The name of the variable represented by this expression.</param>
    /// <param name="arguments">The arguments passed to the invoked method.</param>
    public InvocationExpression(string name, IReadOnlyList<Expression> arguments) {
      Name = name;
      Arguments = arguments;
    }

    /// <summary>
    /// Gets the parameter identifier for the specified index.
    /// </summary>
    /// <param name="index">The index to get the identifier of.</param>
    /// <returns>The argument identifier.</returns>
    public string GetParameterIdentifier(int index) {
      return GetParameterIdentifier(Name, index);
    }

    /// <summary>
    /// Gets the parameter identifier for the specified method with the specified index.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="index">The index to get the identifier of.</param>
    /// <returns>The argument identifier.</returns>
    public static string GetParameterIdentifier(string methodName, int index) {
      return $"$arg_{methodName}_{index}";
    }

    /// <summary>
    /// Checks if the given variable name is a parameter identifier of the given method.
    /// </summary>
    /// <param name="methodName">The method name to check if the variable is a parameter identifier of.</param>
    /// <param name="variableName">The variable name to check.</param>
    /// <returns><code>True</code> if the variable name is a parameter identifier of the given method.</returns>
    public static bool IsParameterIdentifierOf(string methodName, string variableName) {
      return variableName.StartsWith($"$arg_{methodName}_", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the result identifier for the specified method.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <returns>The result identifier.</returns>
    public static string GetResultIdentifier(string methodName) {
      return $"$result_{methodName}";
    }

    /// <summary>
    /// Checks if the given variable name is a result identifier of the given method.
    /// </summary>
    /// <param name="methodName">The method name to check if the variable is a result identifier of.</param>
    /// <param name="variableName">The variable name to check.</param>
    /// <returns><code>True</code> if the variable name is a result identifier of the given method.</returns>
    public static bool IsResultIdentifierOf(string methodName, string variableName) {
      return Equals(GetResultIdentifier(methodName), variableName);
    }

    public override string ToString() {
      return $"{Name}({string.Join(", ", Arguments)})";
    }

    public override void Accept(IExpressionVisitor visitor) {
      visitor.Visit(this);
    }

    public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor) {
      return visitor.Visit(this);
    }
  }
}
