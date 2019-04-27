using RefactorToParallel.Evaluation.Common;
using System;

namespace RefactorToParallel.Evaluation.ByParallelFor {
  /// <summary>
  /// CLI to scan for loops within C# projects which identifies if it can be parallelized.
  /// </summary>
  public class Program {
    private const bool SkipTestProjects = true;

    private static readonly string _reportFilePattern = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{{fileName}}.csv";
    private static readonly Type[] _analysisTypes = { typeof(ParallelForAnalyzer), typeof(ParallelizableParallelForAnalyzer) };

    /// <summary>
    /// Starts the scan.
    /// </summary>
    /// <param name="args">The first argument is either the project or solution file.</param>
    static void Main(string[] args) {
      if (args.Length < 1) {
        Console.WriteLine("invalid argument count");
        return;
      }

      Console.WriteLine("scanning for parallelizable Parallel.For loops");
      var reportFilePattern = args.Length == 2 ? args[1] : _reportFilePattern;
      var runner = new ScanRunner(_analysisTypes, reportFilePattern, SkipTestProjects);
      runner.ScanFile(args[0]);
    }
  }
}
