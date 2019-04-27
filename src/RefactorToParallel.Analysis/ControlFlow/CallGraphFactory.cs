using RefactorToParallel.Analysis.IR;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Factory to create call graphs of given control flow graphs.
  /// </summary>
  public static class CallGraphFactory {
    /// <summary>
    /// Creates a call graph.
    /// </summary>
    /// <param name="root">The root control flow graph where the call graph should begin with.</param>
    /// <param name="methods">The available methods to construct the call graph.</param>
    /// <returns>The constructed call graph.</returns>
    /// <exception cref="UnsupportedSyntaxException">Thrown if an unvailable method is invoked.</exception>
    public static CallGraph Create(ControlFlowGraph root, IDictionary<string, ControlFlowGraph> methods) {
      var callGraph = new CallGraph();
      var visitedMethods = new HashSet<string>();
      _VisitInvocations(root, "<root>", callGraph, visitedMethods, methods);
      return callGraph;
    }

    private static void _VisitInvocations(ControlFlowGraph currentGraph, string currentMethod, CallGraph callGraph, ISet<string> visitedMethods, IDictionary<string, ControlFlowGraph> methods) {
      visitedMethods.Add(currentMethod);

      foreach(var invocation in currentGraph.Nodes.OfType<FlowInvocation>()) {
        var invokedMethod = invocation.Expression.Name;
        if(!methods.TryGetValue(invokedMethod, out var methodGraph)) {
          throw new UnsupportedSyntaxException($"encountered invocation of unknown method: {invocation}");
        }

        var transferTo = new FlowTransfer(true, currentMethod, invocation.Expression.Name);
        callGraph.AddEdge(invocation, transferTo);
        callGraph.AddEdge(transferTo, methodGraph.Start);

        var transferBack = new FlowTransfer(false, invocation.Expression.Name, currentMethod);
        callGraph.AddEdge(methodGraph.End, transferBack);
        callGraph.AddEdge(transferBack, invocation);

        if(!visitedMethods.Contains(invokedMethod)) {
          _VisitInvocations(methodGraph, invokedMethod, callGraph, visitedMethods, methods);
        }
      }
    }
  }
}
