using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Optimizer {
  /// <summary>
  /// Optimizer that is responsible to remove common subexpressions
  /// within the given IR code.
  /// </summary>
  public class CommonSubexpressionElimination {
    private readonly Code _input;
    private readonly ControlFlowGraph _cfg;
    private readonly AvailableExpressionsAnalysis _analysis;
    private readonly IDictionary<Instruction, FlowNode> _instructionNodeMapping;

    private bool _changed;

    private CommonSubexpressionElimination(Code input, ControlFlowGraph cfg, AvailableExpressionsAnalysis analysis,
        IDictionary<Instruction, FlowNode> instructionNodeMapping) {
      _input = input;
      _cfg = cfg;
      _analysis = analysis;
      _instructionNodeMapping = instructionNodeMapping;
    }

    public static (Code Code, bool Changed) Optimize(Code code, ControlFlowGraph controlFlowGraph) {
      var analysis = AvailableExpressionsAnalysis.Analyze(controlFlowGraph);
      var mapping = controlFlowGraph.Nodes
        .ToDictionary(node => node.Instruction, node => node)
        .ToImmutableDictionary();
      var instance = new CommonSubexpressionElimination(code, controlFlowGraph, analysis, mapping);

      return (instance._Optimize(), instance._changed);
    }

    private Code _Optimize() {
      var optimized = _input.Root.Select(_GetOptimized).ToArray();
      return _changed ? new Code(optimized) : _input;
    }

    private Instruction _GetOptimized(Instruction instruction) {
      // Currently it is sufficient to only optimize assignments.
      // For example, the three-address code ensures that arrays are only accessed
      // with variables. Conditions are of no actual use to be simplified at this time.
      var assignment = instruction as Assignment;
      if(assignment == null) {
        return instruction;
      }

      var flowNode = _instructionNodeMapping[instruction];
      var expression = _GetAvailableExpression(flowNode, assignment.Right);
      if(expression == null) {
        return instruction;
      }

      var variableName = _GetVariableName(expression.Node);
      if(variableName == null) {
        return instruction;
      }

      _changed = true;
      return new Assignment(assignment.Left, new VariableExpression(variableName));
    }

    private AvailableExpression _GetAvailableExpression(FlowNode node, Expression expression) {
      return _analysis.Entry[node]
        .Where(available => available.EqualTo(expression))
        .FirstOrDefault();
    }

    private static string _GetVariableName(FlowNode node) {
      return ((node.Instruction as Assignment)?.Left as VariableExpression)?.Name;
    }
  }
}
