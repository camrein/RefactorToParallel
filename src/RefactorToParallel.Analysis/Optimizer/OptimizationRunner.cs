using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR;
using System.Diagnostics;

namespace RefactorToParallel.Analysis.Optimizer {
  /// <summary>
  /// Applies the available set of optimizations to the given code.
  /// </summary>
  public class OptimizationRunner {
    private const int MaxOptimizationIterations = 5;

    private delegate (Code Code, bool Changed) OptimizationProcedure(Code code, ControlFlowGraph controlFlowGraph);

    private static readonly Optimization[] _optimizations = {
      new Optimization("common subexpression elimination", CommonSubexpressionElimination.Optimize),
      new Optimization("copy propagation", CopyPropagation.Optimize)
    };

    /// <summary>
    /// Optimizes the given three-address code and returns additional information.
    /// </summary>
    /// <param name="code">The code to optimize.</param>
    /// <returns>A tuple containing the optimized code and a flag identifying if the code received any changes.</returns>
    public static (Code Code, bool Changed) Optimize(Code code) {
      var changed = true;
      var iterations = 0;

      for(iterations = 0; iterations < MaxOptimizationIterations && changed; ++iterations) {
        changed = false;
        foreach(var optimization in _optimizations) {
          var cfg = ControlFlowGraphFactory.Create(code);
          var optimized = optimization.Procedure(code, cfg);

          if(optimized.Changed) {
            code = optimized.Code;
            changed = true;
            Debug.WriteLine($"the three-address code has been optimized by applying the {optimization.Label}");
          }
        }
      }

      Debug.WriteLine($"applied {iterations} optimization iterations; change in last iteration: {changed}");
      return (code, changed || iterations > 1);
    }

    /// <summary>
    /// Holds information about an optimization.
    /// </summary>
    private class Optimization {
      /// <summary>
      /// Gets the label of the optimization.
      /// </summary>
      public string Label { get; }

      /// <summary>
      /// Gets the procedure that invokes the optimization.
      /// </summary>
      public OptimizationProcedure Procedure { get; }

      /// <summary>
      /// Creates a new instance.
      /// </summary>
      /// <param name="label">The label of the optimization.</param>
      /// <param name="procedure">The procedure that executes the optimization.</param>
      public Optimization(string label, OptimizationProcedure procedure) {
        Label = label;
        Procedure = procedure;
      }
    }
  }
}
