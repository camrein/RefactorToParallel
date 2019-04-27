using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.TestUtil.Code;
using System.Collections.Immutable;

namespace RefactorToParallel.Analysis.Test {
  public class AnalyzerTest<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new() {
    public ImmutableArray<Diagnostic> Analyze(string source) {
      var factory = DocumentFactory.Create();
      factory.CreateFromSource(source);
      return factory.Project.GetCompilationAsync().Result
        .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer()))
        .GetAnalyzerDiagnosticsAsync().Result;
    }

    public void VerifyDiagnostic(string source, params DiagnosticResultLocation[] expectedDiagnostics) {
      var diagnosticResults = Analyze(source);
      Assert.AreEqual(expectedDiagnostics.Length, diagnosticResults.Length, "Invalid diagnostics count");

      for(var i = 0; i < expectedDiagnostics.Length; ++i) {
        var result = diagnosticResults[i];
        var expected = expectedDiagnostics[i];
        var span = result.Location.GetLineSpan();
        Assert.AreEqual(expected.Path, span.Path, "Invalid file path");
        Assert.AreEqual(expected.Line, span.StartLinePosition.Line, "Invalid line number");
        Assert.AreEqual(expected.Column, span.StartLinePosition.Character + 1, "Invalid column");
      }
    }
  }
}
