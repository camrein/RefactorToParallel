using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Core.Util;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Collectors {
  /// <summary>
  /// Collects all possibilities of array aliasing outside of the loop being analyzed.
  /// </summary>
  public class ExternalArrayAliasCollector {
    private readonly SemanticModel _semanticModel;
    private readonly SyntaxNode _loopStatement;
    private readonly VariableAccesses _variableAccesses;

    private ISet<ISymbol> _nonAliasingArrays;

    private ExternalArrayAliasCollector(SemanticModel semanticModel, SyntaxNode loopStatement, VariableAccesses variableAccesses) {
      _semanticModel = semanticModel;
      _loopStatement = loopStatement;
      _variableAccesses = variableAccesses;
    }

    /// <summary>
    /// Collects all possibilities of aliasing the arrays observed in variable accesses.
    /// </summary>
    /// <param name="semanticModel">The semantic model of the analyzed for statement.</param>
    /// <param name="loopStatement">The for statement being analyzed.</param>
    /// <param name="variableAccesses">The collected variable accesses within the for statement.</param>
    /// <returns>A set containing all possibilities of array aliases.</returns>
    public static ISet<VariableAlias> Collect(SemanticModel semanticModel, SyntaxNode loopStatement, VariableAccesses variableAccesses) {
      return new ExternalArrayAliasCollector(semanticModel, loopStatement, variableAccesses)._GetPossibleArrayAliases();
    }

    private ISet<VariableAlias> _GetPossibleArrayAliases() {
      var externalVariables = _variableAccesses.ReadVariables
        .Except(_variableAccesses.DeclaredVariables)
        .ToImmutableHashSet();

      var arraySymbols = externalVariables.Select(_GetSymbolOfVariable).Where(_IsArrayTypedVariable).ToArray();
      var aliases = new List<IdentifiedAlias>();

      _nonAliasingArrays = arraySymbols
        .Where(_IsNoExternallyAccessibleFieldOrProperty)
        .Where(symbol => _GetPossibleReferences(symbol).All(_IsNotAliasing))
        .ToImmutableHashSet();

      foreach(var symbol in arraySymbols) {
        var aliased = false;
        var entryCount = aliases.Count;
        for(var i = 0; i < entryCount; ++i) {
          var candidate = aliases[i];
          if(_IsPossibleAlias(candidate.Source, symbol)) {
            // TODO prevent duplicates already at this point?
            Debug.WriteLine($"assuming the possibility that {symbol} aliases {candidate.Source} which targets {candidate.Target}");
            aliases.Add(new IdentifiedAlias(symbol, candidate.Target));
            aliased = true;
          }
        }

        if(!aliased) {
          // TODO custom expression for "base-case" alias? This might cause trouble when forgetting this situation in further changes.
          aliases.Add(new IdentifiedAlias(symbol, new VariableExpression(symbol.Name)));
        }
      }

      return aliases.Select(alias => new VariableAlias(alias.Source.Name, alias.Target)).ToImmutableHashSet();
    }

    private ISymbol _GetSymbolOfVariable(string variableName) {
      return _loopStatement.DescendantNodes()
        .OfType<IdentifierNameSyntax>()
        .Where(identifier => Equals(variableName, identifier?.Identifier.Text))
        .Select(identifier => _semanticModel.GetSymbolInfo(identifier).Symbol)
        .First();
    }

    private bool _IsArrayTypedVariable(ISymbol symbol) {
      return _GetArrayType(symbol) != null;
    }

    private bool _IsNoExternallyAccessibleFieldOrProperty(ISymbol symbol) {
      if(!(symbol is IFieldSymbol) && !(symbol is IPropertySymbol)) {
        return true;
      }
      return symbol.DeclaredAccessibility == Accessibility.Private;
    }

    private bool _IsPossibleAlias(ISymbol a, ISymbol b) {
      if(Equals(a, b)) {
        return true;
      }

      var aArrayType = _GetArrayType(a);
      var bArrayType = _GetArrayType(b);

      if(_HaveDifferentDimensions(aArrayType, bArrayType) || _AreTypesIncompatible(aArrayType, bArrayType)) {
        return false;
      }

      if(!_nonAliasingArrays.Contains(a) || !_nonAliasingArrays.Contains(b)) {
        return true;
      }

      if(a is ILocalSymbol || b is ILocalSymbol) {
        // If any of the variables is local and both are marked as "nonAliasing", they cannot
        // alias each other in anyway (e.g. externally).
        return false;
      }

      if(!(a is IParameterSymbol || b is IParameterSymbol)) {
        // Parameters may alias eachother or anything but local variables.
        return false;
      }

      return true;
    }

    private bool _HaveDifferentDimensions(IArrayTypeSymbol aArrayType, IArrayTypeSymbol bArrayType) {
      if(aArrayType == null || bArrayType == null) {
        return false;
      }
      return aArrayType.Rank != bArrayType.Rank;
    }

    private bool _AreTypesIncompatible(IArrayTypeSymbol aArrayType, IArrayTypeSymbol bArrayType) {
      if(aArrayType == null || bArrayType == null) {
        return false;
      }

      var aElementType = aArrayType.ElementType;
      var bElementType = bArrayType.ElementType;

      if(Equals(aElementType, bElementType)) {
        return false;
      }

      if(_IsValueType(aElementType) || _IsValueType(bElementType)) {
        return true;
      }

      return false;
    }

    private bool _IsValueType(ITypeSymbol type) {
      // TODO alternative for string based comparison?
      // TODO Value types cannot be assigned to array variables of a different type => somewhere defined?
      var valueType = "System.ValueType";
      return Equals(type?.ToString(), valueType) || Equals(type?.BaseType?.ToString(), valueType);
    }

    private IArrayTypeSymbol _GetArrayType(ISymbol symbol) {
      return ((symbol as ILocalSymbol)?.Type ?? (symbol as IFieldSymbol)?.Type ?? (symbol as IPropertySymbol)?.Type ?? (symbol as IParameterSymbol)?.Type) as IArrayTypeSymbol;
    }

    private bool _IsNotAliasing(SyntaxNode node) {
      // TODO what about conditional expressions?
      switch(node.Kind()) {
      case SyntaxKind.NullLiteralExpression:
      case SyntaxKind.ArrayCreationExpression:
      case SyntaxKind.ImplicitArrayCreationExpression:
        return true;
      default:
        return false;
      }
    }

    private IEnumerable<SyntaxNode> _GetPossibleReferences(ISymbol symbol) {
      return _semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .Select(node => _ResolvePossibleReference(node, symbol))
        .Where(node => node != null);
    }

    private SyntaxNode _ResolvePossibleReference(SyntaxNode node, ISymbol referencingSymbol) {
      switch(node) {
      case AssignmentExpressionSyntax assignment:
        // only two cases the symbols match at this point: direct assignment or assignment to a specified index
        if(!(assignment.Left is ElementAccessExpressionSyntax) && Equals(_semanticModel.GetSymbolInfo(assignment.Left).Symbol, referencingSymbol)) {
          return assignment.Right;
        }
        break;
      case VariableDeclaratorSyntax declarator:
        if(declarator.Initializer != null && Equals(_semanticModel.GetDeclaredSymbol(declarator), referencingSymbol)) {
          return declarator.Initializer.Value;
        }
        break;
      case ArgumentSyntax argument:
        if(!argument.RefOrOutKeyword.IsKind(SyntaxKind.None) && Equals(_semanticModel.GetSymbolInfo(argument.Expression).Symbol, referencingSymbol)) {
          return argument;
        }
        break;
      }
      return null;
    }

    private class IdentifiedAlias {
      private readonly int _hashCode;
      public ISymbol Source { get; }
      public Expression Target { get; }

      public IdentifiedAlias(ISymbol source, Expression target) {
        Source = source;
        Target = target;
        _hashCode = Hash.With(source).And(target).Get();
      }

      public override bool Equals(object obj) {
        var other = obj as IdentifiedAlias;
        return other != null && Equals(Source, other.Source) && Equals(Target, other.Target);
      }

      public override int GetHashCode() {
        return _hashCode;
      }
    }
  }
}
