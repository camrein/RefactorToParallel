using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Optimizer {
  /// <summary>
  /// Tries to propagate copies of values accross the given IR code.
  /// </summary>
  public class CopyPropagation {
    private readonly Code _input;
    private readonly ControlFlowGraph _cfg;
    private readonly CopyPropagationAnalysis _analysis;
    private readonly IDictionary<Instruction, FlowNode> _instructionNodeMapping;

    private bool _changed;

    private CopyPropagation(Code input, ControlFlowGraph cfg, CopyPropagationAnalysis analysis,
        IDictionary<Instruction, FlowNode> instructionNodeMapping) {
      _input = input;
      _cfg = cfg;
      _analysis = analysis;
      _instructionNodeMapping = instructionNodeMapping;
    }

    /// <summary>
    /// Optimizes the given IR code by applying a copy propagation.
    /// </summary>
    /// <param name="code">The code to optimize.</param>
    /// <param name="controlFlowGraph">The control flow graph of the code.</param>
    /// <returns>A tuple of the possibly optimized code and a flag identifying if a change was applied.</returns>
    public static (Code Code, bool Changed) Optimize(Code code, ControlFlowGraph controlFlowGraph) {
      var analysis = CopyPropagationAnalysis.Analyze(controlFlowGraph);
      var mapping = controlFlowGraph.Nodes
        .ToDictionary(node => node.Instruction, node => node)
        .ToImmutableDictionary();
      var instance = new CopyPropagation(code, controlFlowGraph, analysis, mapping);

      return (instance._Optimize(), instance._changed);
    }

    private Code _Optimize() {
      var optimized = _input.Root.Select(_GetOptimized).ToArray();
      return _changed ? new Code(optimized) : _input;
    }

    private Instruction _GetOptimized(Instruction instruction) {
      if(instruction is Jump) {
        // Conventional jumps are not part of the control flow graph.
        return instruction;
      }

      var flowNode = _instructionNodeMapping[instruction];
      var optimized = new InstructionVisitor(_analysis, flowNode).Visit(instruction);
      if(optimized != instruction) {
        _changed = true;
      }
      return optimized;
    }

    private class InstructionVisitor : IInstructionVisitor<Instruction>, IExpressionVisitor<Expression> {
      private readonly FlowNode _flowNode;
      private readonly CopyPropagationAnalysis _analysis;

      public InstructionVisitor(CopyPropagationAnalysis analysis, FlowNode flowNode) {
        _analysis = analysis;
        _flowNode = flowNode;
      }

      public Instruction Visit(Instruction instruction) {
        return instruction.Accept(this);
      }

      public Instruction Visit(Assignment instruction) {
        var left = instruction.Left;
        if(left is ArrayExpression) {
          left = left.Accept(this);
        }

        var right = instruction.Right.Accept(this);
        if(left != instruction.Left || right != instruction.Right) {
          return new Assignment(left, right);
        }
        return instruction;
      }

      public Instruction Visit(Declaration instruction) {
        return instruction;
      }

      public Instruction Visit(Label instruction) {
        return instruction;
      }

      public Instruction Visit(Jump instruction) {
        return instruction;
      }

      public Instruction Visit(ConditionalJump instruction) {
        var condition = instruction.Condition.Accept(this);
        return condition == instruction.Condition ? instruction : new ConditionalJump(condition, instruction.Target);
      }

      public Instruction Visit(Invocation instruction) {
        var expression = instruction.Expression.Accept(this);
        return expression == instruction.Expression ? instruction : new Invocation((InvocationExpression)expression);
      }

      public Expression Visit(Expression expression) {
        return expression.Accept(this);
      }

      public Expression Visit(AddExpression expression) {
        return _VisitBinary(expression, (left, right) => new AddExpression(left, right));
      }

      public Expression Visit(MultiplyExpression expression) {
        return _VisitBinary(expression, (left, right) => new MultiplyExpression(left, right));
      }

      public Expression Visit(SubtractExpression expression) {
        return _VisitBinary(expression, (left, right) => new SubtractExpression(left, right));
      }

      public Expression Visit(DivideExpression expression) {
        return _VisitBinary(expression, (left, right) => new DivideExpression(left, right));
      }

      public Expression Visit(ModuloExpression expression) {
        return _VisitBinary(expression, (left, right) => new ModuloExpression(left, right));
      }

      public Expression Visit(GenericBinaryExpression expression) {
        return _VisitBinary(expression, (left, right) => new GenericBinaryExpression(expression.Operator, left, right));
      }

      private TExpression _VisitBinary<TExpression>(TExpression expression, Func<Expression, Expression, TExpression> constructor)
          where TExpression : BinaryExpression {
        var left = expression.Left.Accept(this);
        var right = expression.Right.Accept(this);
        if(left != expression.Left || right != expression.Right) {
          return constructor(left, right);
        }
        return expression;
      }

      public Expression Visit(ParenthesesExpression expression) {
        var visited = expression.Expression.Accept(this);
        return visited == expression ? expression : new ParenthesesExpression(visited);
      }

      public Expression Visit(ComparisonExpression expression) {
        return _VisitBinary(expression, (left, right) => new ComparisonExpression(expression.Operator, left, right));
      }

      public Expression Visit(VariableExpression expression) {
        // Single is used as it would be a bug in the copy analysis
        // if there are multiple possible copies available.
        var copiedVariableName = _analysis.Entry[_flowNode]
          .Where(copy => copy.TargetVariable.Equals(expression.Name))
          .SingleOrDefault()?
          .SourceVariable;
        return copiedVariableName == null ? expression : new VariableExpression(copiedVariableName);
      }

      public Expression Visit(IntegerExpression expression) {
        return expression;
      }

      public Expression Visit(DoubleExpression expression) {
        return expression;
      }

      public Expression Visit(GenericLiteralExpression expression) {
        return expression;
      }

      public Expression Visit(UnaryMinusExpression expression) {
        var visited = expression.Expression.Accept(this);
        return visited == expression ? expression : new UnaryMinusExpression(visited);
      }

      public Expression Visit(ArrayExpression expression) {
        return _VisitExpressionList(expression, expression.Accessors, items => new ArrayExpression(expression.Name, items));
      }

      public Expression Visit(ConditionalExpression expression) {
        // TODO actually implement a logic for an instruction that shouldn't be present at this step?
        var condition = expression.Condition.Accept(this);
        var whenTrue = expression.WhenTrue.Accept(this);
        var whenFalse = expression.WhenFalse.Accept(this);

        if(condition != expression.Condition || whenTrue != expression.WhenTrue || whenFalse != expression.WhenFalse) {
          return new ConditionalExpression(condition, whenTrue, whenFalse);
        }

        return expression;
      }

      public Expression Visit(InvocationExpression expression) {
        return _VisitExpressionList(expression, expression.Arguments, items => new InvocationExpression(expression.Name, items));
      }

      private TExpression _VisitExpressionList<TExpression>(TExpression expression, IReadOnlyList<Expression> items, Func<Expression[], TExpression> constructor) {
        var processedItems = items.Select(item => item.Accept(this)).ToArray();
        for(var i = 0; i < processedItems.Length; ++i) {
          if(items[i] != processedItems[i]) {
            return constructor(processedItems);
          }
        }
        return expression;
      }
    }
  }
}
