using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Verifier {
  /// <summary>
  /// Verifier that checks if a for statement is in the appropriate form.
  /// </summary>
  public class LoopDeclarationVerifier : CSharpSyntaxVisitor<bool> {
    private readonly SemanticModel _semanticModel;
    private readonly ForStatementSyntax _forStatement;
    private readonly ILocalSymbol _loopIndex;

    private LoopDeclarationVerifier(ForStatementSyntax forStatement, SemanticModel semanticModel, ILocalSymbol loopIndex) {
      _forStatement = forStatement;
      _semanticModel = semanticModel;
      _loopIndex = loopIndex;
    }

    /// <summary>
    /// Checks if the given for statement is in the normalized form.
    /// </summary>
    /// <param name="forStatement">The for statement to check.</param>
    /// <param name="semanticModel">The semantic model to use for the checks.</param>
    /// <returns></returns>
    public static bool IsNormalized(ForStatementSyntax forStatement, SemanticModel semanticModel) {
      if(forStatement.Declaration?.Variables.Count != 1 || forStatement.Initializers.Count != 0 || forStatement.Condition == null
          || forStatement.Incrementors.Count != 1) {
        Debug.WriteLine("one of the parts of the loop's body does not have the correct number of statements");
        return false;
      }

      var declarator = forStatement.Declaration.Variables.Single();
      var loopIndex = semanticModel.GetDeclaredSymbol(declarator) as ILocalSymbol;
      if(loopIndex == null) {
        // Should never happen
        Debug.WriteLine($"index variable {declarator} is not local");
        return false;
      }

      return new LoopDeclarationVerifier(forStatement, semanticModel, loopIndex)._IsNormalized();
    }

    private bool _IsNormalized() {
      return _IsSuitableCondition() && _IsSingleStepIncrement();
    }

    private bool _IsSuitableCondition() {
      var binary = _forStatement.Condition as BinaryExpressionSyntax;
      if(binary == null) {
        Debug.WriteLine($"condition {_forStatement.Condition} is not binary");
        return false;
      }

      switch(binary.OperatorToken.Kind()) {
      case SyntaxKind.LessThanToken:
      case SyntaxKind.LessThanEqualsToken:
      //case SyntaxKind.GreaterThanToken: // TODO Refactoring only supports the above kinds
      //case SyntaxKind.GreaterThanEqualsToken:
        break;
      default:
        Debug.WriteLine($"invalid operator {binary.OperatorToken} used in binary expression");
        return false;
      }

      var leftSymbol = _semanticModel.GetSymbolInfo(binary.Left).Symbol;
      var rightSymbol = _semanticModel.GetSymbolInfo(binary.Right).Symbol;
      var comparand = binary.Right;

      if(!Equals(_loopIndex, leftSymbol)) {
        // TODO refactoring only allows the loop index to be on the left hand side.
        // TODO this is also given by the fact that only LessThan(Equals) checks are supported.
        //comparand = binary.Left;
        //if(!Equals(_loopIndex, rightSymbol)) {
        //  Debug.WriteLine($"index variable is not part of the condition {binary}");
        //  return false;
        //}
        Debug.WriteLine($"index variable is not on the left hand side of the condition {binary}");
        return false;
      }

      return _IsConstantOperand(comparand);
    }

    private bool _IsConstantOperand(SyntaxNode node) {
      if(!Visit(node)) {
        Debug.WriteLine($"cannot proof that comparand is constant: {node}");
        return false;
      }

      return true;
    }

    private bool _IsSingleStepIncrement() {
      try {
        // TODO maybe change this to use symbols, but this would require more checks.
        var indexName = _loopIndex.Name;
        var incrementor = _forStatement.Incrementors.Single();

        var code = CodeFactory.Create(_forStatement.Incrementors.Single(), _semanticModel);
        if(code.Root.Count != 1) {
          Debug.WriteLine($"the incrementor is not a single instruction: {incrementor}");
          return false;
        }

        var assignment = CodeFactory.Create(_forStatement.Incrementors.Single(), _semanticModel).Root.Single() as Assignment;
        if(assignment == null) {
          Debug.WriteLine($"the incrementor is not an assignment as expected");
          return false;
        }

        if(!(assignment.Left is VariableExpression assigned) || !Equals(assigned?.Name, indexName)) {
          Debug.WriteLine($"no incrementation of loop index");
          return false;
        }

        var equality = new ExpressionEquality();
        return equality.AreEqual(assignment.Right, new AddExpression(new VariableExpression(indexName), new IntegerExpression(1)));
      } catch(UnsupportedSyntaxException e) {
        Debug.WriteLine($"cannot check incrementor: {e.Message}");
        return false;
      }
    }

    public override bool DefaultVisit(SyntaxNode node) {
      return false;
    }

    public override bool VisitBinaryExpression(BinaryExpressionSyntax node) {
      return Visit(node.Left) && Visit(node.Right);
    }

    public override bool VisitIdentifierName(IdentifierNameSyntax node) {
      var symbol = _semanticModel.GetSymbolInfo(node).Symbol;

      // TODO somehow get rid of the string comparison
      switch(symbol) {
      case ILocalSymbol local:
        return true;
      case IFieldSymbol field:
        return true;
      case IParameterSymbol parameter:
        return true;
        // The next two cases are used to at least support boundary checks on the arrays and accesses to auto-properties (can be treated as conventional fields).
      case IPropertySymbol property:
        return Equals(property.ContainingType?.ToString(), "System.Array") || property.IsLocalAutoProperty(_semanticModel);
      case IMethodSymbol method:
        return Equals(method.ContainingType?.ToString(), "System.Array");
      }

      return false;
    }

    public override bool VisitLiteralExpression(LiteralExpressionSyntax node) {
      return true;
    }

    public override bool VisitThisExpression(ThisExpressionSyntax node) {
      return true;
    }

    public override bool VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
      return Visit(node.Expression) && Visit(node.Name);
    }
  }
}
