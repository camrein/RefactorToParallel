using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Evaluation {
  /// <summary>
  /// Analysis runner which allows manually running specific analysis on
  /// a given solution.
  /// </summary>
  public class AnalysisRunner {
    private const string SupportedLanguage = "C#";

  /// <summary>
  /// Gets the analyzers managed by this runner.
  /// </summary>
  public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

    private AnalysisRunner(IEnumerable<DiagnosticAnalyzer> analyzers) {
      Analyzers = analyzers.ToImmutableArray();
    }

    /// <summary>
    /// Creates a new instance of the analysis runner.
    /// </summary>
    /// <returns></returns>
    public static AnalysisRunner Create(Type[] analyzerTypes) {
      return new AnalysisRunner(_CreateAnalyzerInstances(analyzerTypes));
    }

    /// <summary>
    /// Analyzes the given project.
    /// </summary>
    /// <param name="project">The analyzed project.</param>
    /// <returns>Dictionary with the reports grouped by filepath where each entry is ordered by line.</returns>
    public ImmutableArray<Diagnostic> Analyze(Project project) {
      var compilation = project.GetCompilationAsync().Result;
      if(!string.Equals(compilation.Language, SupportedLanguage)) {
        return ImmutableArray<Diagnostic>.Empty;
      }
      return compilation.WithAnalyzers(Analyzers).GetAnalyzerDiagnosticsAsync().Result;
    }

    private static IEnumerable<DiagnosticAnalyzer> _CreateAnalyzerInstances(Type[] analyzerTypes) {
      return analyzerTypes
        .Select(type => Activator.CreateInstance(type))
        .OfType<DiagnosticAnalyzer>();
    }
  }
}
