using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RefactorToParallel.Analysis.DataFlow.LoopDependence.Rules {
  /// <summary>
  /// This engine applies the different information transfer rules to the provided expression.
  /// </summary>
  public class RuleEngine : IExpressionVisitor<ISet<DescriptorKind>> {
    private static readonly ITransferRule _addRule = new AddTransferRule();
    private static readonly ITransferRule _multiplyRule = new MultiplyTransferRule();
    private static readonly ITransferRule _subtractRule = new SubtractTransferRule();
    private static readonly ITransferRule _divideRule = new DivideTransferRule();
    private static readonly ITransferRule _moduloRule = new ModuloTransferRule();
    private static readonly VariableTransferRule _variableRule = new VariableTransferRule();
    private static readonly ITransferRule _integerRule = new IntegerTransferRule();
    private static readonly ITransferRule _unaryMinusRule = new UnaryMinusTransferRule();

    private static readonly ISet<DescriptorKind> _emptySet = ImmutableHashSet<DescriptorKind>.Empty;

    /// <summary>
    /// Gets the input knowledge used within the current rule engine.
    /// </summary>
    public ISet<VariableDescriptor> Input { get; }

    /// <summary>
    /// Gets if the analysis is an interprocedural instance.
    /// </summary>
    private bool Interprocedural { get; }

    /// <summary>
    /// Creates a new rule engine with the given input knowledge.
    /// </summary>
    /// <param name="input">The input knowledge to use.</param>
    public RuleEngine(ISet<VariableDescriptor> input) : this(input, false) { }

    /// <summary>
    /// Creates a new rule engine with the given input knowledge.
    /// </summary>
    /// <param name="input">The input knowledge to use.</param>
    public RuleEngine(ISet<VariableDescriptor> input, bool interprocedural) {
      Input = input;
      Interprocedural = interprocedural;
    }

    public ISet<DescriptorKind> Visit(Expression expression) {
      return expression.Accept(this);
    }

    public ISet<DescriptorKind> Visit(AddExpression expression) {
      return _addRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(MultiplyExpression expression) {
      return _multiplyRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(SubtractExpression expression) {
      return _subtractRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(DivideExpression expression) {
      return _divideRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(ModuloExpression expression) {
      return _moduloRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(GenericBinaryExpression expression) {
      return _emptySet;
    }

    public ISet<DescriptorKind> Visit(ParenthesesExpression expression) {
      return expression.Expression.Accept(this);
    }

    public ISet<DescriptorKind> Visit(ComparisonExpression expression) {
      return _emptySet;
    }

    public ISet<DescriptorKind> Visit(VariableExpression expression) {
      return _variableRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(IntegerExpression expression) {
      return _integerRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(DoubleExpression expression) {
      return _emptySet;
    }

    public ISet<DescriptorKind> Visit(GenericLiteralExpression expression) {
      return _emptySet;
    }

    public ISet<DescriptorKind> Visit(UnaryMinusExpression expression) {
      return _unaryMinusRule.Transfer(expression, this);
    }

    public ISet<DescriptorKind> Visit(ArrayExpression expression) {
      return _emptySet;
    }

    public ISet<DescriptorKind> Visit(ConditionalExpression expression) {
      return _emptySet;
    }

    public ISet<DescriptorKind> Visit(InvocationExpression expression) {
      return Interprocedural ? _variableRule.Transfer(expression.ResultIdentifier, this) : _emptySet;
    }
  }
}
