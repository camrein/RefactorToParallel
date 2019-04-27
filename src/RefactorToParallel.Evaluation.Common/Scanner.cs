using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace RefactorToParallel.Evaluation {
  /// <summary>
  /// Scans the given solution and identifies for loops and if it can be parallelized.
  /// </summary>
  public class Scanner : IDisposable {
    private const string ColumnSeperator = ";";

    private static readonly string[] UnitTestProjectNameSuffixFilters = { "Test", "Tests" };
    private static readonly string[] UnitTestProjectFileContentFilters = {
      "3AC096D0-A1C2-E12C-1390-A8335801FDAB", // GUID for unit test projects
      "<Reference Include=\"xunit",  // xunit test framework
      "<Reference Include=\"nunit.framework"  // NUnit test framework
    };

    private readonly AnalysisRunner _runner;
    private readonly StreamWriter _reportStream;
    private readonly Action<string> _progressCallback;

    private bool _disposed;

    private Scanner(AnalysisRunner runner, StreamWriter reportStream, Action<string> progressCallback) {
      _runner = runner;
      _reportStream = reportStream;
      _progressCallback = progressCallback;
    }

    /// <summary>
    /// Creates a new scanner.
    /// </summary>
    /// <param name="analysisTypes">The types of the analyses to include.</param>
    /// <param name="reportFile">The file where the report should be written to.</param>
    /// <returns>A scanner instance.</returns>
    public static Scanner Create(Type[] analysisTypes, string reportFile) {
      return Create(analysisTypes, reportFile, null);
    }

    /// <summary>
    /// Creates a new scanner.
    /// </summary>
    /// <param name="analysisTypes">The types of the analyses to include.</param>
    /// <param name="reportFile">The file where the report should be written to.</param>
    /// <param name="progressCallback">Callback function to report the analysis progress. <code>null</code> if no progress should be reported.</param>
    /// <returns>A scanner instance.</returns>
    public static Scanner Create(Type[] analysisTypes, string reportFile, Action<string> progressCallback) {
      var runner = AnalysisRunner.Create(analysisTypes);
      var stream = File.CreateText(reportFile);
      stream.WriteLine($"File{ColumnSeperator}Line{ColumnSeperator}Reported");
      return new Scanner(runner, stream, progressCallback);
    }

    /// <summary>
    /// Scans the given solution.
    /// </summary>
    /// <param name="solution">The solution to scan.</param>
    /// <param name="skipTestProjects"><code>True</code> if test projects should be skipped.</param>
    public void Scan(Solution solution, bool skipTestProjects) {
      var results = solution.Projects
        .AsParallel().AsOrdered()
        .Where(project => !_IsTestProject(project, skipTestProjects))
        .Select(_Scan)
        .ToImmutableArray();

      foreach(var result in results) {
        _StoreAnalysisResults(result);
      }

      if(_progressCallback != null) {
        var forCount = results.Sum(result => result.Count());
        var parallelCount = results.Sum(result => result.Count(diagnostic => diagnostic.Parallelizable));
        _progressCallback($"scanned solution: {forCount} loops whereas {parallelCount} are parallelizable");
      }
    }

    /// <summary>
    /// Scans the given project.
    /// </summary>
    /// <param name="project">The project to scan.</param>
    public void Scan(Project project) {
      _StoreAnalysisResults(_Scan(project));
    }

    private void _StoreAnalysisResults(IEnumerable<LoopDiagnostic> diagnostics) {
      foreach(var diagnostic in diagnostics) {
        _reportStream.WriteLine(_GetDiagnosticMessage(diagnostic.Diagnostic, diagnostic.Parallelizable));
      }
    }

    private static string _GetDiagnosticMessage(Diagnostic diagnostic, bool parallel) {
      var span = diagnostic.Location.GetLineSpan();
      return $"{span.Path}{ColumnSeperator}{span.StartLinePosition.Line + 1}{ColumnSeperator}{parallel}";
    }

    private static bool _IsTestProject(Project project, bool skipTestProjects) {
      if(!skipTestProjects) {
        return false;
      }

      if(UnitTestProjectNameSuffixFilters.Any(project.Name.EndsWith)) {
        return true;
      }

      var content = File.ReadAllText(project.FilePath);
      return UnitTestProjectFileContentFilters.Any(content.Contains);
    }

    private IEnumerable<LoopDiagnostic> _Scan(Project project) {
      _progressCallback?.Invoke($"scanning project: {project.Name}");

      var diagnostics = _runner.Analyze(project)
        .GroupBy(diagnostic => diagnostic.Location)
        .OrderBy(entry => entry.Key.GetLineSpan().Path)
        .ThenBy(entry => entry.Key.GetLineSpan().StartLinePosition)
        .Select(_CreateLoopDiagnostics)
        .ToImmutableArray();
      var parallelCount = diagnostics.Count(diagnostic => diagnostic.Parallelizable);

      _progressCallback?.Invoke($"{project.Name}: identified {diagnostics.Length} for loops: {parallelCount} can be parallelized");
      return diagnostics;
    }

    private static LoopDiagnostic _CreateLoopDiagnostics(IEnumerable<Diagnostic> diagnostics) {
      var current = diagnostics.ToArray();
      if(current.Length > 2) {
        throw new InvalidOperationException($"got more diagnostics at the same location than expected: {current.Length}");
      }
      return new LoopDiagnostic(current.First(), current.Length == 2);
    }

    ~Scanner() {
      Dispose(false);
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
      if(_disposed) {
        return;
      }

      if(disposing) {
        _reportStream.Dispose();
      }

      _disposed = true;
    }

    /// <summary>
    /// Holds diagnostic information about a for loop.
    /// </summary>
    private class LoopDiagnostic {
      /// <summary>
      /// Gets the diagnostic.
      /// </summary>
      public Diagnostic Diagnostic { get; }

      /// <summary>
      /// Gets if the loop has been identified as parallelizable.
      /// </summary>
      public bool Parallelizable { get; }

      /// <summary>
      /// Creates a new instance.
      /// </summary>
      /// <param name="diagnostic">The diagnostic.</param>
      /// <param name="parallelizable"><code>True</code> if the loop has been identified as parallelizable.</param>
      public LoopDiagnostic(Diagnostic diagnostic, bool parallelizable) {
        Diagnostic = diagnostic;
        Parallelizable = parallelizable;
      }
    }
  }
}
