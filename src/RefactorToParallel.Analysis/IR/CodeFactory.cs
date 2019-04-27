using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Generates a code in the intermediate representation based on the given syntax node.
  /// </summary>
  public class CodeFactory : CSharpSyntaxVisitor<IEnumerable<Instruction>> {
    /// <summary>
    /// The identifier for API methods considered as safe for invocation.
    /// </summary>
    public const string SafeApiIdentifier = "$SafeApi";

    /// <summary>
    /// Gets an empty code.
    /// </summary>
    public static Code Empty { get; } = new Code(new Instruction[] { });

    private bool IsMethod => _methodName != null;
    private bool ProofSemantics => _semanticModel != null;

    private readonly SemanticModel _semanticModel;
    private readonly ExpressionFactory _expressionFactory;
    private readonly Label _exitLabel;
    private readonly string _methodName;

    private delegate IEnumerable<Instruction> LoopBodyTransformer();
    private LoopDescriptor _enclosingLoop;

    private CodeFactory(SemanticModel semanticModel, string methodName) {
      _semanticModel = semanticModel;
      _methodName = methodName;
      _exitLabel = methodName == null ? null : new Label();
      _expressionFactory = new ExpressionFactory(semanticModel, methodName != null);
    }

    /// <summary>
    /// Creates the code out of the given syntax node without any semantic proofs.
    /// </summary>
    /// <param name="syntax">The syntax node to create the code of.</param>
    /// <returns>The created code.</returns>
    /// <exception cref="UnsupportedSyntaxException">Thrown if syntax cannot be converted into IR code.</exception>
    public static Code Create(SyntaxNode syntax) {
      return Create(syntax, null);
    }

    /// <summary>
    /// Creates the code out of the given syntax node with semantic proofs by utilizing the semantic model.
    /// </summary>
    /// <param name="syntax">The syntax node to create the code of.</param>
    /// <param name="semanticModel">The semantic model of the syntax node. If <code>null</code>, no semantic proofs are applied.</param>
    /// <returns>The created code.</returns>
    /// <exception cref="UnsupportedSyntaxException">Thrown if syntax cannot be converted into IR code.</exception>
    public static Code Create(SyntaxNode syntax, SemanticModel semanticModel) {
      return new Code(new CodeFactory(semanticModel, null).Visit(syntax).ToArray());
    }

    /// <summary>
    /// Creates the code out of the given method declaration with leading parameter definition.
    /// </summary>
    /// <param name="syntax">The method declaration to create the code of.</param>
    /// <param name="semanticModel">The semantic model of the syntax node. If <code>null</code>, no semantic proofs are applied.</param>
    /// <returns>The created code.</returns>
    /// <exception cref="UnsupportedSyntaxException">Thrown if syntax cannot be converted into IR code.</exception>
    public static Code CreateMethod(MethodDeclarationSyntax syntax, SemanticModel semanticModel) {
      var methodName = syntax.Identifier.Text;
      var parameters = syntax.ParameterList.Parameters.SelectMany((parameter, index) => _CreateParameterDeclaration(methodName, parameter, index));

      var factory = new CodeFactory(semanticModel, syntax.Identifier.Text);

      var body = _CreateResultDeclaration(syntax)
        .Concat(factory._CreateMethodBody(syntax))
        .Concat(new[] { factory._exitLabel });

      return new Code(parameters.Concat(body).ToArray());
    }

    private IEnumerable<Instruction> _CreateMethodBody(MethodDeclarationSyntax method) {
      if(method.Body != null) {
        return Visit(method.Body);
      }

      if(method.ExpressionBody != null) {
        // Expression bodies are not required to return anything. But in these cases, they are required
        // to assign, call, increment/decrement or create new objects. These cases are all expressions
        // that are not representable as IR expressions.
        // So expressions bodies are either in the expected format or the method cannot be utilized in
        // the interprocedural analysis (UnsupportedSyntaxException is thrown).
        var expression = _expressionFactory.Create(method.ExpressionBody.Expression);
        return new[] { _CreateResultAssignment(expression) };
      }

      throw new UnsupportedSyntaxException($"failed to identify the body of the method '{_methodName}'");
    }

    private static IEnumerable<Instruction> _CreateResultDeclaration(MethodDeclarationSyntax method) {
      if(method.ReturnType is PredefinedTypeSyntax type && type.Keyword.IsKind(SyntaxKind.VoidKeyword)) {
        return Enumerable.Empty<Instruction>();
      }
      return new[] { new Declaration(InvocationExpression.GetResultIdentifier(method.Identifier.Text)) };
    }

    private static IEnumerable<Instruction> _CreateParameterDeclaration(string methodName, ParameterSyntax parameter, int parameterIndex) {
      if(parameter.Modifiers.Any(modifier => !modifier.IsKind(SyntaxKind.None))) {
        throw new UnsupportedSyntaxException($"cannot transform method {methodName} because of unsupported parameter syntax {parameter}");
      }

      var parameterName = parameter.Identifier.Text;
      return new Instruction[] {
        new Declaration(parameterName),
        new Assignment(new VariableExpression(parameterName), new VariableExpression(InvocationExpression.GetParameterIdentifier(methodName, parameterIndex)))
      };
    }

    public override IEnumerable<Instruction> DefaultVisit(SyntaxNode node) {
      throw new UnsupportedSyntaxException(node);
    }

    public override IEnumerable<Instruction> VisitBlock(BlockSyntax node) {
      return node.Statements.SelectMany(statement => statement.Accept(this));
    }

    public override IEnumerable<Instruction> VisitEmptyStatement(EmptyStatementSyntax node) {
      return Enumerable.Empty<Instruction>();
    }

    public override IEnumerable<Instruction> VisitExpressionStatement(ExpressionStatementSyntax node) {
      return node.Expression.Accept(this);
    }

    public override IEnumerable<Instruction> VisitAssignmentExpression(AssignmentExpressionSyntax node) {
      if(!node.IsKind(SyntaxKind.SimpleAssignmentExpression)) {
        return _CreateEnumerable(_CreateAssignmentFromCompoundAssignment(node));
      }

      var left = _expressionFactory.Create(node.Left);
      var right = _expressionFactory.Create(node.Right);
      return _CreateEnumerable(new Assignment(left, right));
    }

    private Assignment _CreateAssignmentFromCompoundAssignment(AssignmentExpressionSyntax node) {
      var operand = _expressionFactory.Create(node.Left);
      var expression = _expressionFactory.Create(node.Right);
      return new Assignment(operand, _CreateBinaryExpressionFromAssignment(node, operand, expression));
    }

    public override IEnumerable<Instruction> VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node) {
      return _CreateEnumerable(_CreateAssignmentFromUnary(node, node.Operand));
    }

    public override IEnumerable<Instruction> VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node) {
      return _CreateEnumerable(_CreateAssignmentFromUnary(node, node.Operand));
    }

    private Assignment _CreateAssignmentFromUnary(ExpressionSyntax node, ExpressionSyntax operandNode) {
      var operand = _expressionFactory.Create(operandNode);
      var one = new IntegerExpression(1);
      return new Assignment(operand, _CreateBinaryExpressionFromAssignment(node, operand, one));
    }

    private BinaryExpression _CreateBinaryExpressionFromAssignment(SyntaxNode node, Expression left, Expression right) {
      if(ProofSemantics && !node.IsMemberMethodOfPrimitiveType(_semanticModel)) {
        throw new UnsupportedSyntaxException($"detected overloaded compound assignment operator {node}");
      }

      switch(node.Kind()) {
      case SyntaxKind.PreIncrementExpression:
      case SyntaxKind.PostIncrementExpression:
        return new AddExpression(left, right);
      case SyntaxKind.PreDecrementExpression:
      case SyntaxKind.PostDecrementExpression:
        return new SubtractExpression(left, right);
      case SyntaxKind.AddAssignmentExpression:
        return _CreateAddExpressionFromAddAssignment((AssignmentExpressionSyntax)node, left, right);
      case SyntaxKind.SubtractAssignmentExpression:
        return new SubtractExpression(left, right);
      case SyntaxKind.MultiplyAssignmentExpression:
        return new MultiplyExpression(left, right);
      case SyntaxKind.DivideAssignmentExpression:
        return new DivideExpression(left, right);
      case SyntaxKind.ModuloAssignmentExpression:
        return new ModuloExpression(left, right);
      default:
        // TODO GenericBinaryExpression would be sufficient in this case.
        throw new UnsupportedSyntaxException(node);
      }
    }

    public AddExpression _CreateAddExpressionFromAddAssignment(AssignmentExpressionSyntax node, Expression left, Expression right) {
      // String concatenations require semantic proofs because of the implicit toString() call.
      var result = new AddExpression(left, right);
      if(!ProofSemantics) {
        return result;
      }

      var addSymbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
      if(addSymbol == null) {
        throw new UnsupportedSyntaxException($"failed to resolve the symbol for the add expression {node}");
      }

      if(!addSymbol.ContainingType.IsStringType()) {
        return result;
      }

      if(!node.Right.IsEvaluatingToPrimitiveType(_semanticModel)) {
        throw new UnsupportedSyntaxException($"the assignee of the string concatenation '{node}' will not evaluate to a primitive type");
      }

      return result;
    }

    public override IEnumerable<Instruction> VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) {
      return node.Declaration.Accept(this);
    }

    public override IEnumerable<Instruction> VisitVariableDeclaration(VariableDeclarationSyntax node) {
      return node.Variables.SelectMany(variable => variable.Accept(this));
    }

    public override IEnumerable<Instruction> VisitVariableDeclarator(VariableDeclaratorSyntax node) {
      var identifier = node.Identifier.Text;
      var result = new List<Instruction> { new Declaration(identifier) };

      var initializer = node.Initializer?.Value;
      if(initializer != null) {
        result.Add(new Assignment(new VariableExpression(identifier), _expressionFactory.Create(initializer)));
      }

      return result;
    }

    public override IEnumerable<Instruction> VisitIfStatement(IfStatementSyntax node) {
      var endElse = new Label();
      var endBranch = new Label();

      var transformed = new List<Instruction>();
      transformed.Add(new ConditionalJump(_expressionFactory.Create(node.Condition), endElse));

      if(node.Else != null) {
        transformed.AddRange(node.Else.Accept(this));
      }

      transformed.Add(new Jump(endBranch));
      transformed.Add(endElse);

      if(node.Statement != null) {
        transformed.AddRange(node.Statement.Accept(this));
      }

      transformed.Add(endBranch);

      return transformed;
    }

    private IEnumerable<Instruction> _CreateEnumerable(params Instruction[] nodes) {
      return nodes.Where(node => node != null);
    }

    public override IEnumerable<Instruction> VisitElseClause(ElseClauseSyntax node) {
      return node.Statement.Accept(this);
    }

    public override IEnumerable<Instruction> VisitDoStatement(DoStatementSyntax node) {
      LoopBodyTransformer bodyTransformer = () => node.Statement?.Accept(this) ?? Enumerable.Empty<Instruction>();

      var loopEntry = new Label();
      var loopExit = new Label();
      var continueLabel = new Label();

      var transformed = new List<Instruction>();
      transformed.Add(loopEntry);
      transformed.AddRange(_ProcessLoopBody(bodyTransformer, continueLabel, loopExit));
      transformed.Add(continueLabel);
      transformed.Add(new ConditionalJump(_expressionFactory.Create(node.Condition), loopEntry));
      transformed.Add(loopExit);

      return transformed;
    }

    public override IEnumerable<Instruction> VisitWhileStatement(WhileStatementSyntax node) {
      LoopBodyTransformer bodyTransformer = () => node.Statement?.Accept(this) ?? Enumerable.Empty<Instruction>();
      return _CreateLoop(node.Condition, bodyTransformer, null);
    }

    public override IEnumerable<Instruction> VisitForStatement(ForStatementSyntax node) {
      var transformed = new List<Instruction>();
      if(node.Declaration != null) {
        transformed.AddRange(node.Declaration.Accept(this));
      }
      transformed.AddRange(node.Initializers.SelectMany(initializer => initializer.Accept(this)));

      var continueLabel = new Label();
      LoopBodyTransformer bodyTransformer = () => {
        var body = new List<Instruction>();
        body.AddRange(node.Statement.Accept(this));
        body.Add(continueLabel);
        body.AddRange(node.Incrementors.SelectMany(incrementor => incrementor.Accept(this)));
        return body;
      };

      transformed.AddRange(_CreateLoop(node.Condition, bodyTransformer, continueLabel));
      return transformed;
    }

    public override IEnumerable<Instruction> VisitInvocationExpression(InvocationExpressionSyntax node) {
      return _CreateEnumerable(new Invocation((InvocationExpression)_expressionFactory.Create(node)));
    }

    public override IEnumerable<Instruction> VisitReturnStatement(ReturnStatementSyntax node) {
      if(!IsMethod) {
        throw new UnsupportedSyntaxException($"return statement {node} is not allowed inside loops");
      }

      var transformed = new List<Instruction>();
      if(node.Expression != null) {
        transformed.Add(_CreateResultAssignment(_expressionFactory.Create(node.Expression)));
      }

      transformed.Add(new Jump(_exitLabel));
      return transformed;
    }

    public override IEnumerable<Instruction> VisitBreakStatement(BreakStatementSyntax node) {
      if(_enclosingLoop == null) {
        throw new UnsupportedSyntaxException($"no loop to attach the break statement to");
      }
      return _CreateEnumerable(new Jump(_enclosingLoop.BreakLabel));
    }

    public override IEnumerable<Instruction> VisitContinueStatement(ContinueStatementSyntax node) {
      if(_enclosingLoop == null) {
        throw new UnsupportedSyntaxException($"no loop to attach the continue statement to");
      }
      return _CreateEnumerable(new Jump(_enclosingLoop.ContinueLabel));
    }

    private IEnumerable<Instruction> _CreateLoop(ExpressionSyntax condition, LoopBodyTransformer bodyTransformer, Label continueLabel) {
      var loopEntry = new Label();
      var loopExit = new Label();
      var loopBody = new Label();
      continueLabel = continueLabel ?? loopEntry;

      var transformed = new List<Instruction>();
      transformed.Add(loopEntry);
      transformed.Add(condition == null ? (Instruction)new Jump(loopBody) : new ConditionalJump(_expressionFactory.Create(condition), loopBody));
      transformed.Add(new Jump(loopExit));

      transformed.Add(loopBody);
      transformed.AddRange(_ProcessLoopBody(bodyTransformer, continueLabel, loopExit));
      transformed.Add(new Jump(loopEntry));

      transformed.Add(loopExit);
      return transformed;
    }

    private IEnumerable<Instruction> _ProcessLoopBody(LoopBodyTransformer bodyTransformer, Label continueLabel, Label breakLabel) {
      var oldEnclosing = _enclosingLoop;
      _enclosingLoop = new LoopDescriptor(continueLabel, breakLabel);
      // ToArray (or any form of materialization) is important to ensure that the body is actually processed at this place.
      var result = bodyTransformer().ToArray();
      _enclosingLoop = oldEnclosing;
      return result;
    }

    private Assignment _CreateResultAssignment(Expression result) {
      return new Assignment(new VariableExpression(InvocationExpression.GetResultIdentifier(_methodName)), result);
    }

    /// <summary>
    /// Descriptor holding information about a loop.
    /// </summary>
    private class LoopDescriptor {
      /// <summary>
      /// Gets the label where the continue statements should be connected to.
      /// </summary>
      public Label ContinueLabel { get; }

      /// <summary>
      /// Gets the label where the break statements should be connected to.
      /// </summary>
      public Label BreakLabel { get; }

      /// <summary>
      /// Creates a new instance.
      /// </summary>
      /// <param name="continueLabel">The label where the continue statements should be connected to.</param>
      /// <param name="breakLabel">The label where the break statements should be connected to.</param>
      public LoopDescriptor(Label continueLabel, Label breakLabel) {
        ContinueLabel = continueLabel;
        BreakLabel = breakLabel;
      }
    }
  }
}
