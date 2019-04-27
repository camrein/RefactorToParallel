using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Generates an <see cref="Expression"/> out of the given <see cref="ExpressionSyntax"/>.
  /// </summary>
  public class ExpressionFactory {
    private readonly Transformer _transformer;

    /// <summary>
    /// Creates a new instance of the expression factory.
    /// </summary>
    /// <param name="semanticModel">The semantic model of the expressions being fed to this factory. If <code>null</code>, no semantic proofs are applied.</param>
    /// <param name="isMethod"><code>True</code> if the expression is part of a method declaration.</param>
    public ExpressionFactory(SemanticModel semanticModel, bool isMethod) {
      _transformer = new Transformer(semanticModel, isMethod);
    }

    /// <summary>
    /// Creates the expression tree out of the given expression syntax node.
    /// </summary>
    /// <returns>The created expression tree.</returns>
    /// <exception cref="UnsupportedSyntaxException">Thrown if any syntax node cannot be represented as an expression.</exception>
    public Expression Create(ExpressionSyntax node) {
      return _transformer.Visit(node);
    }

    private class Transformer : CSharpSyntaxVisitor<Expression> {
      private readonly SemanticModel _semanticModel;
      private readonly bool _proofSemantics;
      private readonly IDictionary<string, ISymbol> _variables = new Dictionary<string, ISymbol>();
      private readonly bool _isMethod;

      public Transformer(SemanticModel semanticModel, bool isMethod) {
        _semanticModel = semanticModel;
        _proofSemantics = semanticModel != null;
        _isMethod = isMethod;
      }

      public override Expression DefaultVisit(SyntaxNode node) {
        throw new UnsupportedSyntaxException(node);
      }

      public override Expression VisitCastExpression(CastExpressionSyntax node) {
        if(node.Type is ArrayTypeSyntax) {
          // TODO maybe only prevent for nodes that haven't been identified as arrays before?
          // TODO at this time it's not possible to create an alias for an array type in C#, this might change in the future.
          throw new UnsupportedSyntaxException($"detected cast to array type in node '{node}'");
        }
        return node.Expression.Accept(this);
      }

      public override Expression VisitBinaryExpression(BinaryExpressionSyntax node) {
        if(_proofSemantics && node.IsOverloadedBinaryOperator(_semanticModel)) {
          throw new UnsupportedSyntaxException($"detected overloaded binary operator '{node}'");
        }

        var left = node.Left.Accept(this);
        var right = node.Right.Accept(this);

        switch(node.Kind()) {
        case SyntaxKind.CoalesceExpression:
          return _CreateConditionalFromCoalesceExpression(node, left, right);
        case SyntaxKind.AddExpression:
          return _CreateAddExpression(node, left, right);
        case SyntaxKind.SubtractExpression:
          return new SubtractExpression(left, right);
        case SyntaxKind.MultiplyExpression:
          return new MultiplyExpression(left, right);
        case SyntaxKind.DivideExpression:
          return new DivideExpression(left, right);
        case SyntaxKind.ModuloExpression:
          return new ModuloExpression(left, right);
        case SyntaxKind.LessThanExpression:
        case SyntaxKind.LessThanOrEqualExpression:
        case SyntaxKind.GreaterThanExpression:
        case SyntaxKind.GreaterThanOrEqualExpression:
        case SyntaxKind.EqualsExpression:
        case SyntaxKind.NotEqualsExpression:
        case SyntaxKind.LogicalAndExpression: // TODO seperate expression?
        case SyntaxKind.LogicalOrExpression: // TODO seperate expression?
          return new ComparisonExpression(node.OperatorToken.Text, left, right);
        default:
          return new GenericBinaryExpression(node.OperatorToken.Text, left, right);
        }
      }

      public AddExpression _CreateAddExpression(BinaryExpressionSyntax node, Expression left, Expression right) {
        // String concatenations require semantic proofs because of the implicit toString() call.
        var result = new AddExpression(left, right);
        if(!_proofSemantics) {
          return result;
        }

        var addSymbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
        if(addSymbol == null) {
          throw new UnsupportedSyntaxException($"failed to resolve the symbol for the add expression {node}");
        }

        if(!addSymbol.ContainingType.IsStringType()) {
          return result;
        }

        if(!node.Left.IsEvaluatingToPrimitiveType(_semanticModel) || !node.Right.IsEvaluatingToPrimitiveType(_semanticModel)) {
          throw new UnsupportedSyntaxException($"one of the operands of the string concatenation '{node}' will not evaluate to a primitive type");
        }

        return result;
      }

      public override Expression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node) {
        if(_proofSemantics && !node.IsMemberMethodOfPrimitiveType(_semanticModel)) {
          throw new UnsupportedSyntaxException($"detected overloaded unary operator '{node}'");
        }

        var operand = node.Operand.Accept(this);
        switch(node.Kind()) {
        case SyntaxKind.UnaryMinusExpression:
          return new UnaryMinusExpression(operand);
        case SyntaxKind.LogicalNotExpression:
          return new ComparisonExpression("!=", operand, new GenericLiteralExpression());
        default:
          throw new UnsupportedSyntaxException(node);
        }
      }

      public override Expression VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) {
        return new ParenthesesExpression(node.Expression.Accept(this));
      }

      public override Expression VisitLiteralExpression(LiteralExpressionSyntax node) {
        return node.IsKind(SyntaxKind.NumericLiteralExpression) ? _GetNumberExpression(node) : new GenericLiteralExpression();
      }

      public override Expression VisitIdentifierName(IdentifierNameSyntax node) {
        var variableName = node.Identifier.Text;

        if(_proofSemantics) {
          // TODO it might be possible to reference methods this way, but currently this is no problem.
          var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
          switch(symbol) {
          case ILocalSymbol local:
            break;
          case IParameterSymbol parameter:
            _VerifyParameterAccess(parameter, node);
            break;
          case IMethodSymbol method:  // Disabling this case completely prevents inter-procedural analysis.
            break;
          case IPropertySymbol property:
            _VerifyPropertyAccess(property, node);
            break;
          case IFieldSymbol field:
            _VerifyFieldAccess(field, node);
            break;
          default:
            throw new UnsupportedSyntaxException($"detected prohibited identifier usage with symbol '{symbol}' in expression: {node}");
          }

          if(!(symbol is IMethodSymbol)) {
            // This filter is necessary as invocations of generic methods with different types result in distinct symbols
            if(_variables.TryGetValue(variableName, out var presentSymbol)) {
              if(!Equals(symbol, presentSymbol)) {
                throw new UnsupportedSyntaxException($"detected shadowing of variable '{node}': expected '{presentSymbol}', got '{symbol}'");
              }
            } else {
              _variables.Add(variableName, symbol);
            }
          }
        }

        return new VariableExpression(variableName);
      }

      private void _VerifyParameterAccess(IParameterSymbol parameter, SyntaxNode node) {
        if(_isMethod) {
          // ref and out parameters are already filtered for method invocations
          return;
        }

        if(parameter.RefKind != RefKind.None) {
          // accesses to ref or out parameters cannot be parallelized. An alternative to complete
          // prevention is the extraction into an external variable.
          throw new UnsupportedSyntaxException($"access to ref or out parameter '{node}' is not supported for parallel loops");
        }
      }

      private void _VerifyPropertyAccess(IPropertySymbol property, SyntaxNode node) {
        if(!_IsSupportedProperty(property)) {
          throw new UnsupportedSyntaxException($"property '{node}' is not supported");
        }

        if(!_isMethod) {
          return;
        }
        
        if(property.SetMethod != null) {
          throw new UnsupportedSyntaxException($"property '{node}' used within method is not readonly");
        }

        if(property.Type is IArrayTypeSymbol) {
          throw new UnsupportedSyntaxException($"property '{node}' used within method is an array");
        }
      }

      private void _VerifyFieldAccess(IFieldSymbol field, SyntaxNode node) {
        // TODO this filter probably has to be relaxed as it completely prevents the use of fields in method bodies.
        // Note: const is used because it is guaranteed that there are no write-accesses. Read-Only fields can be written multiple times within the constructor.
        //       If this filter is changed, further checks are necessary in the method-bodies to ensure fields are never written with a method.
        if(!_isMethod) {
          return;
        }

        if(!field.IsConst && !field.IsReadOnly) {
          throw new UnsupportedSyntaxException($"detected access to field '{node}' that is neither const nor readonly inside method");
        }

        if(field.Type is IArrayTypeSymbol) {
          throw new UnsupportedSyntaxException($"detected access to field '{node}' which is an array tpe");
        }
      }

      public override Expression VisitElementAccessExpression(ElementAccessExpressionSyntax node) {
        if(!_IsArrayAccess(node)) {
          throw new UnsupportedSyntaxException($"cannot represent node {node} as its not an array access");
        }

        var variable = node.Expression.Accept(this) as VariableExpression;
        if(variable == null) {
          // This check, inter alia, prevents jagged arrays
          throw new UnsupportedSyntaxException(node);
        }

        var accessors = node.ArgumentList.Arguments.Select(argument => argument?.Accept(this)).ToArray();
        if(accessors.Any(accessor => accessor == null)) {
          throw new UnsupportedSyntaxException(node);
        }

        return new ArrayExpression(variable.Name, accessors);
      }

      public override Expression VisitArgument(ArgumentSyntax node) {
        if(!node.RefOrOutKeyword.IsKind(SyntaxKind.None)) {
          throw new UnsupportedSyntaxException($"cannot represent argument {node} due to unsupported kind (ref or out)");
        }

        return node.Expression.Accept(this);
      }

      public override Expression VisitConditionalExpression(ConditionalExpressionSyntax node) {
        return new ConditionalExpression(node.Condition.Accept(this), node.WhenTrue.Accept(this), node.WhenFalse.Accept(this));
      }

      public override Expression VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
        if(node.Expression is ThisExpressionSyntax) {
          // ThisExpressions can simply be truncated as shadowing is prevented with the semantic checks.
          return node.Name.Accept(this);
        }

        if(!node.IsSafeFieldAccess(_semanticModel)) {
          throw new UnsupportedSyntaxException(node);
        }

        // TODO maybe something more suitable. A variable expression is not possible as a later check may not resolve the associated symbol.
        return new GenericLiteralExpression();
      }

      public override Expression VisitInvocationExpression(InvocationExpressionSyntax node) {
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
        string methodName = null;

        if(symbol.IsPureApi()) {
          // Currently, the Argument Visitor prevents the use of ref and out parameters. It might be necessary
          // to make some adaptions here when such parameters are allowed.
          methodName = CodeFactory.SafeApiIdentifier;
        } else {
          // Overloads are prevented as only methods that do not have any overloads are part of the analysis.
          // Polymorphism / Method overriding is prevented as only private methods are part of the analysis.
          if(!_IsMethodInvocation(node)) {
            throw new UnsupportedSyntaxException($"cannot represent {node} as invocation target is not a method");
          }

          // TODO better way to safely retrieve the identifiers of the invoked methods?
          var variable = node.Expression.Accept(this) as VariableExpression;
          if(variable == null) {
            throw new UnsupportedSyntaxException(node);
          }

          methodName = variable.Name;
        }

        var arguments = node.ArgumentList.Arguments.Select(argument => argument?.Accept(this)).ToArray();
        if(arguments.Any(argument => argument == null)) {
          throw new UnsupportedSyntaxException(node);
        }

        return new InvocationExpression(methodName, arguments);
      }

      public override Expression VisitTypeOfExpression(TypeOfExpressionSyntax node) {
        // TODO really safe to strip the arguments? The arguments should only be Types.
        return new GenericLiteralExpression();
      }

      public override Expression VisitDefaultExpression(DefaultExpressionSyntax node) {
        // TODO really safe to strip the arguments? The arguments should only be Types.
        return new GenericLiteralExpression();
      }

      public override Expression VisitSizeOfExpression(SizeOfExpressionSyntax node) {
        // TODO really safe to strip the arguments? The arguments should only be Types.
        return new GenericLiteralExpression();
      }

      public override Expression VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node) {
        Expression concatenated = null;
        foreach(var content in node.Contents) {
          Expression part;
          switch(content) {
          case InterpolationSyntax interpolation:
            if(_proofSemantics && !interpolation.Expression.IsEvaluatingToPrimitiveType(_semanticModel)) {
              throw new UnsupportedSyntaxException($"the interpolation '{interpolation}' of a string interpolation will not evaluate to a primitive type");
            }
            part = interpolation.Expression.Accept(this);
            break;
          case InterpolatedStringTextSyntax text:
            // TODO not really necessary at this point as strings do not provide any information.
            part = new GenericLiteralExpression();
            break;
          default:
            throw new UnsupportedSyntaxException($"interpolated string '{node}' contains unsupported content '{content}'");
          }

          concatenated = concatenated == null ? part : new AddExpression(concatenated, part);
        }

        // Faulty formatted (aka syntax errors) string interpolations as well as empty strings may lead to an empty content list.
        return concatenated ?? new GenericLiteralExpression();
      }

      private Expression _CreateConditionalFromCoalesceExpression(BinaryExpressionSyntax node, Expression left, Expression right) {
        return new ConditionalExpression(new ComparisonExpression("!=", left, new GenericLiteralExpression()), left, right);
      }

      private Expression _GetNumberExpression(LiteralExpressionSyntax node) {
        switch(node.Token.Value) {
        case int integer:
          return new IntegerExpression(integer);
        case long longVal:
          return new IntegerExpression(longVal);
        case float floatVal:
          return new DoubleExpression(floatVal);
        case double doubleVal:
          return new DoubleExpression(doubleVal);
        default:
          return new GenericLiteralExpression();
        }
      }

      private bool _IsArrayAccess(ElementAccessExpressionSyntax node) {
        if(!_proofSemantics) {
          return true;
        }

        var symbol = _semanticModel.GetSymbolInfo(node.Expression).Symbol;
        if(symbol == null) {
          return false;
        }

        // This prevents direct usage of the return value of a method (e.g. Method()[0]). But the IR does not support this kind of accesses at this time.
        var array = ((symbol as ILocalSymbol)?.Type ?? (symbol as IFieldSymbol)?.Type ?? (symbol as IPropertySymbol)?.Type ?? (symbol as IParameterSymbol)?.Type) as IArrayTypeSymbol;
        if(array == null) {
          Debug.WriteLine($"detected element access to non-array object: {node}");
          return false;
        }

        if(array.ElementType is IArrayTypeSymbol) {
          Debug.WriteLine($"detected element acccess to jagged array: {node}");
          return false;
        }

        return true;
      }

      private bool _IsMethodInvocation(InvocationExpressionSyntax node) {
        return !_proofSemantics || _semanticModel.GetSymbolInfo(node.Expression).Symbol is IMethodSymbol;
      }

      private bool _IsSupportedProperty(IPropertySymbol property) {
        return !_proofSemantics || property.IsLocalAutoProperty(_semanticModel);
      }
    }
  }
}
