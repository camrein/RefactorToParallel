using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.IR {
  /// <summary>
  /// Allows to check if two expressions are equal from a semantic point of view.
  /// </summary>
  public class ExpressionEquality {
    // TODO equality checker mixes cfg, dfa and IR, move to a more suitable namespace.
    // TODO this is probably no longer need due to the optimizers.
    private readonly FlowNode _aNode;
    private readonly FlowNode _bNode;
    private readonly AliasAnalysis _aliases;

    /// <summary>
    /// Creates an expression equality analysis which does not respect flow information.
    /// Variables with the same name are assumed to be equal.
    /// </summary>
    public ExpressionEquality() : this(null, null, null) { }

    /// <summary>
    /// Creates an expression equality which makes use of flow information and aliases.
    /// </summary>
    /// <param name="aNode">The control flow graph node where the first expression is located.</param>
    /// <param name="bNode">The control flow graph node where the second expression is located.</param>
    /// <param name="aliases">The aliases collected within the control flow graph.</param>
    public ExpressionEquality(FlowNode aNode, FlowNode bNode, AliasAnalysis aliases) {
      _aNode = aNode;
      _bNode = bNode;
      _aliases = aliases;
    }

    /// <summary>
    /// Checks if two expressions are semantically equal.
    /// </summary>
    /// <param name="a">The first expression to check.</param>
    /// <param name="b">The second expression to check.</param>
    /// <returns><code>True</code> if both expressions are semantically equal.</returns>
    public bool AreEqual(Expression a, Expression b) {
      if(a == null || b == null) {
        return false;
      }

      if(a == b) {
        // Shortcut if two expressions are equal. This is only correct because it is guaranteed
        // that expressions instances are not used multiple times within the same IR code.
        return true;
      }

      if(a.GetType() != b.GetType()) {
        return false;
      }

      if(a is AddExpression) {
        return _AreEqual((AddExpression)a, (AddExpression)b);
      }

      if(a is MultiplyExpression) {
        return _AreEqual((MultiplyExpression)a, (MultiplyExpression)b);
      }

      if(a is SubtractExpression) {
        return _AreEqual((SubtractExpression)a, (SubtractExpression)b);
      }

      if(a is DivideExpression) {
        return _AreEqual((DivideExpression)a, (DivideExpression)b);
      }

      if(a is ModuloExpression) {
        return _AreEqual((ModuloExpression)a, (ModuloExpression)b);
      }

      if(a is VariableExpression) {
        return _AreEqual((VariableExpression)a, (VariableExpression)b);
      }

      if(a is IntegerExpression) {
        return _AreEqual((IntegerExpression)a, (IntegerExpression)b);
      }

      if(a is UnaryMinusExpression) {
        return _AreEqual((UnaryMinusExpression)a, (UnaryMinusExpression)b);
      }

      Debug.WriteLine($"encountered unsupported expression for equality check: {a.GetType()}");
      return false;
    }

    private bool _AreEqual(AddExpression a, AddExpression b) {
      return (AreEqual(a.Left, b.Left) && AreEqual(a.Right, b.Right))
        || (AreEqual(a.Left, b.Right) && AreEqual(a.Right, b.Left));
    }

    private bool _AreEqual(MultiplyExpression a, MultiplyExpression b) {
      return (AreEqual(a.Left, b.Left) && AreEqual(a.Right, b.Right))
        || (AreEqual(a.Left, b.Right) && AreEqual(a.Right, b.Left));
    }

    private bool _AreEqual(SubtractExpression a, SubtractExpression b) {
      // TODO might respect unary minus expressions
      return AreEqual(a.Left, b.Left) && AreEqual(a.Right, b.Right);
    }

    private bool _AreEqual(DivideExpression a, DivideExpression b) {
      return AreEqual(a.Left, b.Left) && AreEqual(a.Right, b.Right);
    }

    private bool _AreEqual(ModuloExpression a, ModuloExpression b) {
      return AreEqual(a.Left, b.Left) && AreEqual(a.Right, b.Right);
    }

    private bool _AreEqual(VariableExpression a, VariableExpression b) {
      if(_aliases == null) {
        return a.Name.Equals(b.Name);
      }

      // TODO currently only allowing one single alias. There are cases
      // multiple are correct, but also where not. If loop dependence analysis
      // supports branches, this has to be improved.
      var aAliases = _aliases.GetAliasesOfVariableBefore(a.Name, _aNode).ToList();
      if(aAliases.Count != 1) {
        return false;
      }

      var bAliases = _aliases.GetAliasesOfVariableBefore(b.Name, _bNode).ToList();
      if(bAliases.Count != 1) {
        return false;
      }

      // TODO support nested expressions through aliasing?
      return aAliases.Single().Equals(bAliases.Single());
    }

    private bool _AreEqual(IntegerExpression a, IntegerExpression b) {
      return a.Value == b.Value;
    }

    private bool _AreEqual(UnaryMinusExpression a, UnaryMinusExpression b) {
      return AreEqual(a.Expression, b.Expression);
    }
  }
}
