using RefactorToParallel.Analysis.ControlFlow;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  /// <summary>
  /// This converter allows to convert CFGs to the dot language.
  /// </summary>
  public static class Dotifier {
    /// <summary>
    /// Converts the given control flow graph to a dot graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to convert.</param>
    /// <returns>The string representation of the graph.</returns>
    public static string ToDot(this ControlFlowGraph cfg) {
      return new[] { cfg }.ToDot(null);
    }

    /// <summary>
    /// Converts the given control flow graph to a dot graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to convert.</param>
    /// <param name="output">The buffer where the output should be written to.</param>
    public static void ToDot(this ControlFlowGraph cfg, TextWriter output) {
      new[] { cfg }.ToDot(null, output);
    }

    /// <summary>
    /// Converts the given control flow graphs with the given call graph to a dot graph.
    /// </summary>
    /// <param name="cfgs">The control flow graphs to convert.</param>
    /// <param name="callGraph">The call graph that connects the control flow graphs. <code>null</code> if no call graph is available.</param>
    /// <returns>The string representation of the graph.</returns>
    public static string ToDot(this IEnumerable<ControlFlowGraph> cfgs, CallGraph callGraph) {
      var buffer = new StringWriter();
      cfgs.ToDot(callGraph, buffer);
      return buffer.ToString().Trim();
    }

    /// <summary>
    /// Converts the given control flow graphs with the given call graph to a dot graph.
    /// </summary>
    /// <param name="cfgs">The control flow graphs to convert.</param>
    /// <param name="callGraph">The call graph that connects the control flow graphs. <code>null</code> if no call graph is available.</param>
    /// <param name="output">The buffer where the output should be written to.</param>
    public static void ToDot(this IEnumerable<ControlFlowGraph> cfgs, CallGraph callGraph, TextWriter output) {
      output.Write("digraph cfg {\r\n");
      var mappings = _AddNodes(cfgs.SelectMany(cfg => cfg.Nodes).Concat(callGraph?.Nodes ?? Enumerable.Empty<FlowNode>()), output);
      _AddEdges(cfgs.SelectMany(cfg => cfg.Edges), mappings, output);
      _AddStyledEdges(callGraph?.Edges ?? Enumerable.Empty<FlowEdge>(), mappings, output, "dashed");
      output.Write("}\r\n");
    }

    private static Dictionary<FlowNode, int> _AddNodes(IEnumerable<FlowNode> nodes, TextWriter output) {
      var mappings = new Dictionary<FlowNode, int>();
      var id = 0;
      foreach(var node in nodes.Distinct().OrderBy(n => n.ToString())) {
        var escapedNode = node.ToString().Replace("\"", "\\\"").Replace("\\", "\\\\");
        output.Write($"  \"{id}\" [ label = \"{escapedNode}\" ];\r\n");
        mappings.Add(node, id++);
      }
      return mappings;
    }

    private static void _AddEdges(IEnumerable<FlowEdge> edges, IReadOnlyDictionary<FlowNode, int> mappings, TextWriter output) {
      _AddStyledEdges(edges, mappings, output, null);
    }

    private static void _AddStyledEdges(IEnumerable<FlowEdge> edges, IReadOnlyDictionary<FlowNode, int> mappings, TextWriter output, string style) {
      var styleDefinition = style == null ? null : $", style={style}";
      foreach(var edge in edges.OrderBy(e => e.ToString())) {
        output.Write($"  \"{mappings[edge.From]}\" -> \"{mappings[edge.To]}\" [ label = \"\"{styleDefinition} ];\r\n");
      }
    }
  }
}
