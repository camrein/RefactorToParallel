using Microsoft.CodeAnalysis;
using System;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Exception that is thrown if a syntax node cannot be represented within the control flow graph.
  /// </summary>
  [Serializable]
  public class UnsupportedSyntaxException : Exception {
    /// <summary>
    /// Creates a new instance that represents the given syntax node.
    /// </summary>
    /// <param name="syntax">The syntax node to represent.</param>
    public UnsupportedSyntaxException(SyntaxNode syntax) : this($"Cannot represent the node '{syntax}' of type '{syntax.GetType()}' in the cfg.") { }

    /// <summary>
    /// Creates a new instance that represents the given message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public UnsupportedSyntaxException(string message) : base(message) {}
  }
}
