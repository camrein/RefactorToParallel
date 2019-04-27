using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// This class allows the transformation of any arbitrary IR code into its three-address code equivalent.
  /// </summary>
  public static class ThreeAddressCodeFactory {
    /// <summary>
    /// Transforms the given IR code into its three-address equivalent.
    /// </summary>
    /// <param name="code">The code to create the three-address code of.</param>
    /// <returns>The transformed three-address code.</returns>
    public static Code Create(Code code) {
      return new Code(code.Root.SelectMany(new Transformer().Visit).ToArray());
    }

    /// <summary>
    /// Code transformer which applies the actual three-address code transformation
    /// provided by the factory.
    /// </summary>
    private class Transformer : IInstructionVisitor<IEnumerable<Instruction>>, IExpressionVisitor<SimplifiedExpression> {
      private int _currentVariableId;
      private int _expressionDepth;

      public IEnumerable<Instruction> Visit(Instruction instruction) {
        return instruction.Accept(this);
      }

      public IEnumerable<Instruction> Visit(Assignment instruction) {
        var left = Visit(instruction.Left);
        var right = Visit(instruction.Right);

        return new List<Instruction>(left.Instructions.Concat(right.Instructions)) {
          new Assignment(left.Expression, right.Expression)
        };
      }

      public IEnumerable<Instruction> Visit(Declaration instruction) {
        return _CreateEnumerable(instruction);
      }

      public IEnumerable<Instruction> Visit(Label instruction) {
        return _CreateEnumerable(instruction);
      }

      public IEnumerable<Instruction> Visit(Jump instruction) {
        return _CreateEnumerable(instruction);
      }

      public IEnumerable<Instruction> Visit(ConditionalJump instruction) {
        var simplified = Visit(instruction.Condition);
        return new List<Instruction>(simplified.Instructions) {
          new ConditionalJump(simplified.Expression, instruction.Target)
        };
      }

      public IEnumerable<Instruction> Visit(Invocation instruction) {
        // Cast is used to ensure that the expression depth counter is incremented.
        var simplified = Visit((Expression)instruction.Expression);
        return new List<Instruction>(simplified.Instructions) {
          new Invocation((InvocationExpression)simplified.Expression)
        };
      }

      public SimplifiedExpression Visit(Expression expression) {
        ++_expressionDepth;
        var result = expression.Accept(this);
        --_expressionDepth;
        return result;
      }

      public SimplifiedExpression Visit(AddExpression expression) {
        return _VisitBinary(expression, (left, right) => new AddExpression(left, right));
      }

      public SimplifiedExpression Visit(MultiplyExpression expression) {
        return _VisitBinary(expression, (left, right) => new MultiplyExpression(left, right));
      }

      public SimplifiedExpression Visit(SubtractExpression expression) {
        return _VisitBinary(expression, (left, right) => new SubtractExpression(left, right));
      }

      public SimplifiedExpression Visit(DivideExpression expression) {
        return _VisitBinary(expression, (left, right) => new DivideExpression(left, right));
      }

      public SimplifiedExpression Visit(ModuloExpression expression) {
        return _VisitBinary(expression, (left, right) => new ModuloExpression(left, right));
      }

      public SimplifiedExpression Visit(GenericBinaryExpression expression) {
        return _VisitBinary(expression, (left, right) => new GenericBinaryExpression(expression.Operator, left, right));
      }

      public SimplifiedExpression Visit(ParenthesesExpression expression) {
        return expression.Expression.Accept(this);
      }

      public SimplifiedExpression Visit(ComparisonExpression expression) {
        return _VisitBinary(expression, (left, right) => new ComparisonExpression(expression.Operator, left, right));
      }

      private SimplifiedExpression _VisitBinary<TExpression>(TExpression expression, Func<Expression, Expression, TExpression> constructor)
          where TExpression : BinaryExpression {
        var left = Visit(expression.Left);
        var right = Visit(expression.Right);
        Expression constructed = constructor(left.Expression, right.Expression);

        var instructions = new List<Instruction>(left.Instructions.Concat(right.Instructions));
        if(_expressionDepth > 1) {
          var extracted = _GetExpressionExtractedInTemporaryVariable(constructed);
          instructions.AddRange(extracted.Instructions);
          constructed = new VariableExpression(extracted.VariableName);
        }

        return new SimplifiedExpression(constructed, instructions);
      }

      public SimplifiedExpression Visit(VariableExpression expression) {
        return new SimplifiedExpression(expression);
      }

      public SimplifiedExpression Visit(IntegerExpression expression) {
        return new SimplifiedExpression(expression);
      }

      public SimplifiedExpression Visit(DoubleExpression expression) {
        return new SimplifiedExpression(expression);
      }

      public SimplifiedExpression Visit(UnaryMinusExpression expression) {
        if(_expressionDepth > 1) {
          var extracted = _GetExpressionExtractedInTemporaryVariable(expression);
          return new SimplifiedExpression(new VariableExpression(extracted.VariableName), extracted.Instructions.ToList());
        }
        return new SimplifiedExpression(expression);
      }

      public SimplifiedExpression Visit(ArrayExpression expression) {
        var simplifiedAccessors = expression.Accessors.Select(Visit).ToList();
        var instructions = new List<Instruction>(simplifiedAccessors.SelectMany(accessor => accessor.Instructions));
        var accessors = simplifiedAccessors.Select(accessor => accessor.Expression).ToArray();
        var array = new ArrayExpression(expression.Name, accessors);

        if(_expressionDepth > 1) {
          var extracted = _GetExpressionExtractedInTemporaryVariable(array);
          instructions.AddRange(extracted.Instructions);
          return new SimplifiedExpression(new VariableExpression(extracted.VariableName), instructions);
        }

        return new SimplifiedExpression(array, instructions);
      }


      public SimplifiedExpression Visit(GenericLiteralExpression expression) {
        return new SimplifiedExpression(expression);
      }

      public SimplifiedExpression Visit(ConditionalExpression expression) {
        // Conditional expressions are completely extracted (transformed to IR equivalent to if/else statements), so the depth can be reset
        var oldExpressionDepth = _expressionDepth;
        _expressionDepth = 0;

        var temporaryVariable = _GetNextVariableName();
        var endElse = new Label();
        var endBranch = new Label();

        var instructions = new List<Instruction>();
        instructions.Add(new Declaration(temporaryVariable));
        
        var simplifiedCondition = Visit(expression.Condition);
        instructions.AddRange(simplifiedCondition.Instructions);
        instructions.Add(new ConditionalJump(simplifiedCondition.Expression, endElse));

        var simplifiedWhenFalse = Visit(expression.WhenFalse);
        instructions.AddRange(simplifiedWhenFalse.Instructions);
        instructions.Add(new Assignment(new VariableExpression(temporaryVariable), simplifiedWhenFalse.Expression));
        instructions.Add(new Jump(endBranch));
        instructions.Add(endElse);

        var simplifiedWhenTrue = Visit(expression.WhenTrue);
        instructions.AddRange(simplifiedWhenTrue.Instructions);
        instructions.Add(new Assignment(new VariableExpression(temporaryVariable), simplifiedWhenTrue.Expression));
        instructions.Add(endBranch);

        _expressionDepth = oldExpressionDepth;

        return new SimplifiedExpression(new VariableExpression(temporaryVariable), instructions);
      }

      public SimplifiedExpression Visit(InvocationExpression expression) {
        var simplifiedArguments = expression.Arguments.Select(Visit).ToList();
        var instructions = new List<Instruction>(simplifiedArguments.SelectMany(argument => argument.Instructions));
        var arguments = simplifiedArguments.Select(argument => argument.Expression).ToArray();
        var array = new InvocationExpression(expression.Name, arguments);

        if(_expressionDepth > 1) {
          var extracted = _GetExpressionExtractedInTemporaryVariable(array);
          instructions.AddRange(extracted.Instructions);
          return new SimplifiedExpression(new VariableExpression(extracted.VariableName), instructions);
        }

        return new SimplifiedExpression(array, instructions);
      }

      private (IEnumerable<Instruction> Instructions, string VariableName) _GetExpressionExtractedInTemporaryVariable(Expression expression) {
        var temporaryVariable = _GetNextVariableName();
        Debug.WriteLine($"extracting expression '{expression}' into variable {temporaryVariable}");

        return (new Instruction[] {
          new Declaration(temporaryVariable),
          new Assignment(new VariableExpression(temporaryVariable), expression)
        }, temporaryVariable);
      }

      private string _GetNextVariableName() {
        return $"$temp_{_currentVariableId++}";
      }

      private IEnumerable<Instruction> _CreateEnumerable(params Instruction[] nodes) {
        return nodes.Where(node => node != null);
      }
    }

    /// <summary>
    /// Holds information extracted from the sub-expression.
    /// </summary>
    private class SimplifiedExpression {
      /// <summary>
      /// Gets the instructions that have been extracted.
      /// </summary>
      public IList<Instruction> Instructions { get; }

      /// <summary>
      /// Gets the expression to access the extracted sub-expression.
      /// </summary>
      public Expression Expression { get; }

      /// <summary>
      /// Creates a new simplified expression without actual simplification by re-using
      /// the actual expression.
      /// </summary>
      /// <param name="expression">The expression to use.</param>
      public SimplifiedExpression(Expression expression) : this(expression, new List<Instruction>()) {
      }

      /// <summary>
      /// Creates a new simplified expression whereas the expression accesses the temporary variable
      /// of the extracted sub-expressions which are stored within instructions.
      /// </summary>
      /// <param name="expression">The expression to access the extracted sub-expression.</param>
      /// <param name="instructions">The instructions used for the extraction.</param>
      public SimplifiedExpression(Expression expression, IList<Instruction> instructions) {
        Expression = expression;
        Instructions = instructions;
      }
    }
  }
}
