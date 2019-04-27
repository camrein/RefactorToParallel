using RefactorToParallel.Analysis.Collectors;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.DataFlow.LoopDependence.Kinds;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Verifier {
  /// <summary>
  /// Verifies that any array access within the code does not conflict when executed in parallel.
  /// </summary>
  public class ArrayAccessVerifier {
    private readonly LoopDependenceAnalysis _loopDependenceAnalysis;
    private readonly AliasAnalysis _aliasAnalysis;
    private readonly ISet<ArrayAccess> _arrayAccesses;

    private ArrayAccessVerifier(LoopDependenceAnalysis loopDependenceAnalysis, AliasAnalysis aliasAnalysis, ISet<ArrayAccess> arrayAccesses) {
      _loopDependenceAnalysis = loopDependenceAnalysis;
      _aliasAnalysis = aliasAnalysis;
      _arrayAccesses = arrayAccesses;
    }

    /// <summary>
    /// Checks that there are no conflicting read and write accesses to arrays.
    /// </summary>
    /// <param name="loopIndex">The loop index of the loop that is subject for parallelization.</param>
    /// <param name="controlFlowGraph">The control flow graph of the body of the loop.</param>
    /// <param name="aliasAnalysis">A prelimary applied alias analysis already fed with possible aliases outside of the loop.</param>
    /// <returns><code>True</code> if there are no conflicting accesses to arrays.</returns>
    public static bool HasConflictingAccesses(string loopIndex, ControlFlowGraph controlFlowGraph, AliasAnalysis aliasAnalysis) {
      var loopDependenceAnalysis = LoopDependenceAnalysis.Analyze(controlFlowGraph, loopIndex);
      var arrayAccesses = ArrayAccessCollector.Collect(controlFlowGraph);
      var verifier = new ArrayAccessVerifier(loopDependenceAnalysis, aliasAnalysis, arrayAccesses);
      return verifier._ContainsLoopCarriedDependencies();
    }

    /// <summary>
    /// Checks that there are no conflicting read and write accesses to arrays.
    /// </summary>
    /// <param name="loopIndex">The loop index of the loop that is subject for parallelization.</param>
    /// <param name="controlFlowGraph">The control flow graph of the body of the loop.</param>
    /// <param name="aliasAnalysis">A prelimary applied alias analysis already fed with possible aliases outside of the loop.</param>
    /// <param name="callGraph">The graph identifying the calls of methods for interprocedural analysis.</param>
    /// <param name="procedures">The control flow graphs of the invoked methods.</param>
    /// <returns><code>True</code> if there are no conflicting accesses to arrays.</returns>
    public static bool HasConflictingAccesses(string loopIndex, ControlFlowGraph controlFlowGraph, AliasAnalysis aliasAnalysis, CallGraph callGraph, IEnumerable<ControlFlowGraph> procedures) {
      var loopDependenceAnalysis = LoopDependenceAnalysis.Analyze(controlFlowGraph, loopIndex, callGraph, procedures);
      var arrayAccesses = ArrayAccessCollector.Collect(new[] { controlFlowGraph }.Concat(procedures).SelectMany(graph => graph.Nodes));
      var verifier = new ArrayAccessVerifier(loopDependenceAnalysis, aliasAnalysis, arrayAccesses);
      return verifier._ContainsLoopCarriedDependencies();
    }


    private bool _ContainsLoopCarriedDependencies() {
      return _GroupArrayAccessesByArrayInstance()
        .Select(entry => entry.Value)
        .Where(_ContainsWriteAccesses)
        .Any(_ContainsAccessIntersections);
    }

    private IDictionary<Expression, ISet<ArrayAccess>> _GroupArrayAccessesByArrayInstance() {
      return _arrayAccesses
        .SelectMany(access => _GetAliases(access).Select(alias => new { Access = access, Alias = alias }))
        .GroupBy(entry => entry.Alias.Target)
        .ToImmutableDictionary(
          entry => entry.Key,
          entry => (ISet<ArrayAccess>)entry.Select(tuple => tuple.Access).ToImmutableHashSet()
        );
    }

    private IEnumerable<VariableAlias> _GetAliases(ArrayAccess access) {
      var aliases = _aliasAnalysis.GetAliasesOfVariableAfter(access.Expression.Name, access.Node).ToArray();
      if(aliases.Length == 0) {
        // The only case where an array alias failed to resolve is when the loop is incomplete (e.g. due to infinite loops without a condition).
        throw new UnsupportedSyntaxException($"failed to resolve aliases for array access {access.Expression}");
      }
      return aliases;
    }

    private bool _ContainsWriteAccesses(IEnumerable<ArrayAccess> accesses) {
      return accesses.Any(access => access.Write);
    }

    private bool _ContainsAccessIntersections(ISet<ArrayAccess> accesses) {
      if(!_AreAllAccessesTheSame(accesses)) {
        Debug.WriteLine($"identified accesses with distinct accessor variable definitions");
        return true;
      }

      return false;
    }

    private bool _AreAllAccessesTheSame(IEnumerable<ArrayAccess> accesses) {
      // TODO maybe merge with the other iterations?
      // Accessor does not have to be tracked as no more than one
      // definition per instruction is possible.
      Expression expectedExpression = null;
      FlowNode expectedNode = null;
      int expectedPosition = -1;

      foreach(var access in accesses) {
        var accessor = _GetAccessor(access);
        var expression = _GetAliasTarget(accessor.Variable, access.Node);

        if(expectedExpression == null) {
          expectedExpression = expression;
          expectedNode = access.Node;
          expectedPosition = accessor.Position;
          continue;
        }

        var equality = new ExpressionEquality(expectedNode, access.Node, _aliasAnalysis);
        if(expectedPosition != accessor.Position || !equality.AreEqual(expectedExpression, expression)) {
          return false;
        }
      }

      return true;
    }

    private (int Position, string Variable) _GetAccessor(ArrayAccess access) {
      var array = access.Expression;
      if(array.Accessors.Count == 0) {
        Debug.WriteLine($"unsupported amount of array accessors: {array.Accessors.Count}");
        throw new UnsupportedSyntaxException($"invalid number of dimensions for array access: {array}");
      }

      for(var i = 0; i < array.Accessors.Count; ++i) {
        var accessor = (array.Accessors[i] as VariableExpression)?.Name;
        if(accessor == null) {
          continue;
        }

        var loopDependant = _loopDependenceAnalysis.Entry[access.Node]
          .Where(descriptor => descriptor.Kind is LoopDependent)
          .Where(descriptor => descriptor.Name.Equals(accessor))
          .Any();

        if(loopDependant) {
          return (i, accessor);
        }
      }

      throw new UnsupportedSyntaxException($"array access does not make us of a loop dependant variable: {array}");
    }

    private Expression _GetAliasTarget(string variableName, FlowNode node) {
      // TODO at this point this check is unnecessary and could be removed.
      // because of the loop dependence analysis check beforehand which does not support
      // multiple values of a variable, there should actually be exactly one alias.
      var aliases = _aliasAnalysis.GetAliasesOfVariableBefore(variableName, node).ToList();
      if(aliases.Count != 1) {
        // TODO it might be possible to support multiple distinct aliases in some cases
        throw new UnsupportedSyntaxException($"detected multiple possible values for variable {variableName}: {aliases}");
      }

      return aliases[0].Target;
    }
  }
}
