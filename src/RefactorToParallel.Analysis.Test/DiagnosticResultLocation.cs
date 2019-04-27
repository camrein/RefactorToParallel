
namespace RefactorToParallel.Analysis.Test {
  /// <summary>
  /// Location where the diagnostic appears, as determined by path, line number, and column number.
  /// </summary>
  public class DiagnosticResultLocation {
    public string Path { get; }
    public int Line { get; }
    public int Column { get; }

    public DiagnosticResultLocation(int line, int column) : this("Test.cs", line, column) { }

    public DiagnosticResultLocation(string path, int line, int column) {
      Path = path;
      Line = line;
      Column = column;
    }
  }
}
