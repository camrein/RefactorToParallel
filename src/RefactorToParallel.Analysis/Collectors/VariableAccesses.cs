using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;

namespace RefactorToParallel.Analysis.Collectors {
  /// <summary>
  /// Collects all variable accesses within the given control flow graph.
  /// </summary>
  public class VariableAccesses : IInstructionVisitor, IExpressionVisitor {
    /// <summary>
    /// Gets the variables with read accesses.
    /// </summary>
    public ISet<string> ReadVariables { get; } = new HashSet<string>();

    /// <summary>
    /// Gets the variables with write accesses.
    /// </summary>
    public ISet<string> WrittenVariables { get; } = new HashSet<string>();

    /// <summary>
    /// Gets variables declared within the control flow graph.
    /// </summary>
    public ISet<string> DeclaredVariables { get; } = new HashSet<string>(); // TODO same for arrays?

    /// <summary>
    /// Gets the arrays with read accesses.
    /// </summary>
    public ISet<ArrayExpression> ReadArrays { get; } = new HashSet<ArrayExpression>();

    /// <summary>
    /// Gets the arrays with write accesses.
    /// </summary>
    public ISet<ArrayExpression> WrittenArrays { get; } = new HashSet<ArrayExpression>();

    private VariableAccesses() {}

    /// <summary>
    /// Collects all variable accesses within the given code.
    /// </summary>
    /// <param name="code">The code to collect the accesses from.</param>
    /// <returns>The collected variable accesses.</returns>
    public static VariableAccesses Collect(Code code) {
      var accesses = new VariableAccesses();
      foreach(var node in code.Root) {
        accesses.Visit(node);
      }
      return accesses;
    }

    /// <summary>
    /// Collects all variable accesses within the given expression.
    /// </summary>
    /// <param name="expression">The expression to collect the accesses from.</param>
    /// <returns>The collected variable accesses.</returns>
    public static VariableAccesses Collect(Expression expression) {
      var accesses = new VariableAccesses();
      accesses.Visit(expression);
      return accesses;
    }

    public void Visit(Instruction instruction) {
      instruction.Accept(this);
    }

    public void Visit(Assignment instruction) {
      if(instruction.Left is VariableExpression variable) {
        WrittenVariables.Add(variable.Name);
      } else if(instruction.Left is ArrayExpression array) {
        ReadVariables.Add(array.Name);
        WrittenArrays.Add(array);
        _VisitItems(array.Accessors);
      }

      instruction.Right.Accept(this);
    }

    public void Visit(Declaration instruction) {
      DeclaredVariables.Add(instruction.Name);
    }

    public void Visit(Label instruction) { }

    public void Visit(Jump instruction) { }

    public void Visit(ConditionalJump instruction) {
      instruction.Condition.Accept(this);
    }

    public void Visit(Invocation instruction) {
      instruction.Expression.Accept(this);
    }

    public void Visit(Expression expression) {
      expression.Accept(this);
    }

    public void Visit(AddExpression expression) {
      _VisitBinaryExpression(expression);
    }

    public void Visit(MultiplyExpression expression) {
      _VisitBinaryExpression(expression);
    }

    public void Visit(SubtractExpression expression) {
      _VisitBinaryExpression(expression);
    }

    public void Visit(DivideExpression expression) {
      _VisitBinaryExpression(expression);
    }

    public void Visit(ModuloExpression expression) {
      _VisitBinaryExpression(expression);
    }

    public void Visit(GenericBinaryExpression expression) {
      _VisitBinaryExpression(expression);
    }

    public void Visit(ParenthesesExpression expression) {
      expression.Expression.Accept(this);
    }

    public void Visit(ComparisonExpression expression) {
      _VisitBinaryExpression(expression);
    }

    private void _VisitBinaryExpression(BinaryExpression expression) {
      expression.Left.Accept(this);
      expression.Right.Accept(this);
    }

    public void Visit(VariableExpression expression) {
      ReadVariables.Add(expression.Name);
    }

    public void Visit(IntegerExpression expression) { }

    public void Visit(DoubleExpression expression) { }

    public void Visit(GenericLiteralExpression expression) { }

    public void Visit(UnaryMinusExpression expression) {
      expression.Expression.Accept(this);
    }

    public void Visit(ArrayExpression expression) {
      ReadVariables.Add(expression.Name);
      ReadArrays.Add(expression);
      _VisitItems(expression.Accessors);
    }

    public void Visit(ConditionalExpression expression) {
      expression.Condition.Accept(this);
      expression.WhenTrue.Accept(this);
      expression.WhenFalse.Accept(this);
    }

    public void Visit(InvocationExpression expression) {
      _VisitItems(expression.Arguments);
    }

    private void _VisitItems(IEnumerable<Expression> items) {
      foreach(var item in items) {
        item.Accept(this);
      }
    }
  }
}
