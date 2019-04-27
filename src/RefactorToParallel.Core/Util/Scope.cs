using System;

namespace RefactorToParallel.Core.Util {
  /// <summary>
  /// Allows running action in a scope where the value of the variables is saved
  /// prior execution and restored after execution.
  /// </summary>
  public class Scope {
    /// <summary>
    /// Scopes the given variable and executions the action within this scope.
    /// </summary>
    /// <typeparam name="T">The type of the variable to scope.</typeparam>
    /// <param name="toScope">The scoped variable.</param>
    /// <param name="action">The action to execute within the scope.</param>
    public static void Scoped<T>(ref T toScope, Action action) {
      var old = toScope;
      action();
      toScope = old;
    }

    /// <summary>
    /// Scopes the given variable and executions the action within this scope.
    /// </summary>
    /// <typeparam name="T1">The type of the variable to scope.</typeparam>
    /// <typeparam name="T2">The type of the variable to scope.</typeparam>
    /// <param name="toScope1">The scoped variable.</param>
    /// <param name="toScope2">The scoped variable.</param>
    /// <param name="action">The action to execute within the scope.</param>
    public static void Scoped<T1, T2>(ref T1 toScope1, ref T2 toScope2, Action action) {
      var old1 = toScope1;
      var old2 = toScope2;
      action();
      toScope2 = old2;
      toScope1 = old1;
    }

    /// <summary>
    /// Scopes the given variable and executions the action within this scope.
    /// </summary>
    /// <typeparam name="T1">The type of the variable to scope.</typeparam>
    /// <typeparam name="T2">The type of the variable to scope.</typeparam>
    /// <typeparam name="T3">The type of the variable to scope.</typeparam>
    /// <param name="toScope1">The scoped variable.</param>
    /// <param name="toScope2">The scoped variable.</param>
    /// <param name="toScope3">The scoped variable.</param>
    /// <param name="action">The action to execute within the scope.</param>
    public static void Scoped<T1, T2, T3>(ref T1 toScope1, ref T2 toScope2, ref T3 toScope3, Action action) {
      var old1 = toScope1;
      var old2 = toScope2;
      var old3 = toScope3;
      action();
      toScope3 = old3;
      toScope2 = old2;
      toScope1 = old1;
    }
  }
}
