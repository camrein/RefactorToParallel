using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Linq;

namespace RefactorToParallel.Analysis.ControlFlow {
  /// <summary>
  /// Generates a <see cref="ControlFlowGraph"/> out of the given <see cref="Code"/>.
  /// </summary>
  public class ControlFlowGraphFactory : IInstructionVisitor {
    /// <summary>
    /// Gets an empty control flow graph.
    /// </summary>
    public static ControlFlowGraph Empty { get; } = Create(CodeFactory.Empty, CodeFactory.SafeApiIdentifier, true);

    private readonly ControlFlowGraph _cfg;
    private readonly bool _interprocedural;

    private FlowNode _previousNode;

    private ControlFlowGraphFactory(ControlFlowGraph cfg, bool interprocedural) {
      _cfg = cfg;
      _previousNode = cfg.Start;
      _interprocedural = interprocedural;
    }

    /// <summary>
    /// Creates a new control flow graph of out of the given root syntax node.
    /// </summary>
    /// <param name="code">The code to create the control flow graph of.</param>
    /// <returns>The generated control flow graph.</returns>
    public static ControlFlowGraph Create(Code code) {
      return Create(code, false);
    }

    /// <summary>
    /// Creates a new control flow graph of out of the given root syntax node.
    /// </summary>
    /// <param name="code">The code to create the control flow graph of.</param>
    /// <param name="interprocedural"><code>True</code> if the control flow graph should be prepared for an interprocedural analysis.</param>
    /// <returns>The generated control flow graph.</returns>
    public static ControlFlowGraph Create(Code code, bool interprocedural) {
      return Create(code, null, interprocedural);
    }

    /// <summary>
    /// Creates a new control flow graph of out of the given root syntax node.
    /// </summary>
    /// <param name="code">The code to create the control flow graph of.</param>
    /// <param name="methodName">Name of the containing method or <code>null</code>.</param>
    /// <param name="interprocedural"><code>True</code> if the control flow graph should be prepared for an interprocedural analysis.</param>
    /// <returns>The generated control flow graph.</returns>
    public static ControlFlowGraph Create(Code code, string methodName, bool interprocedural) {
      var cfg = new ControlFlowGraph(methodName);
      var generator = new ControlFlowGraphFactory(cfg, interprocedural);

      foreach(var node in code.Root) {
        generator.Visit(node);
      }
      generator._Finalize();
      return generator._cfg;
    }

    public void Visit(Instruction instruction) {
      instruction.Accept(this);
    }

    public void Visit(Assignment instruction) {
      _ProcessInvocations(instruction.Left);
      _ProcessInvocations(instruction.Right);

      var node = _cfg.RegisterInstruction(instruction);
      _ConnectWithPreviousNode(node);
      _previousNode = node;
    }

    public void Visit(Declaration instruction) {
      var node = _cfg.RegisterInstruction(instruction);
      _ConnectWithPreviousNode(node);
      _previousNode = node;
    }

    public void Visit(Label instruction) {
      var node = _cfg.RegisterInstruction(instruction);
      _ConnectWithPreviousNode(node);
      _previousNode = node;
    }

    public void Visit(Jump instruction) {
      // TODO keep the conventional jumps in the CFG?
      //_cfg.AddEdge(new FlowNode(node), _labels[node.Target]);
      if(_previousNode != null) {
        _cfg.AddEdge(_previousNode, _cfg.RegisterInstruction(instruction.Target));
      }
      _previousNode = null;
    }

    public void Visit(ConditionalJump instruction) {
      _ProcessInvocations(instruction.Condition);

      var node = _cfg.RegisterInstruction(instruction);
      _ConnectWithPreviousNode(node);
      _cfg.AddEdge(node, _cfg.RegisterInstruction(instruction.Target));
      _previousNode = node;
    }

    public void Visit(Invocation instruction) {
      _ProcessInvocations(instruction.Expression);

      var node = _cfg.RegisterInstruction(instruction);
      _ConnectWithPreviousNode(node);
      _previousNode = node;
    }

    private void _ProcessInvocations(Expression expression) {
      if(!_interprocedural) {
        return;
      }

      foreach(var invocation in expression.DescendantsAndSelf().OfType<InvocationExpression>()) {
        for(var i = 0; i < invocation.Arguments.Count; ++i) {
          // TODO maybe move responsibility of the argument setup into the three-address code transformer?
          var variableName = invocation.GetParameterIdentifier(i);

          var argumentDefinition = _cfg.RegisterInstruction(new Declaration(variableName));
          _ConnectWithPreviousNode(argumentDefinition);
          _previousNode = argumentDefinition;
          
          var argumentAssignment = _cfg.RegisterInstruction(new Assignment(new VariableExpression(variableName), invocation.Arguments[i]));
          _ConnectWithPreviousNode(argumentAssignment);
          _previousNode = argumentAssignment;
        }

        var node = _cfg.RegisterInvocation(invocation);
        _ConnectWithPreviousNode(node);
        _previousNode = node;
      }
    }

    private void _ConnectWithPreviousNode(FlowNode node) {
      if(_previousNode != null) {
        _cfg.AddEdge(_previousNode, node);
      }
    }

    private void _Finalize() {
      // All nodes that do not have a successor are exit nodes
      foreach(var node in _cfg.Nodes.Where(n => n.Successors.Count == 0 && n != _cfg.End)) {
        _cfg.AddEdge(node, _cfg.End);
      }
    }
  }
}
