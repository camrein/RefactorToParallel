using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;

namespace RefactorToParallel.Evaluation.Common {
  public class ScanRunner {
    private readonly Type[] _analysisTypes;
    private readonly string _reportFilePattern;
    private readonly bool _skipTestProjects;

    public ScanRunner(Type[] analysisTypes, string reportFilePattern, bool skipTestProjects) {
      _analysisTypes = analysisTypes;
      _reportFilePattern = reportFilePattern;
      _skipTestProjects = skipTestProjects;
    }

    public void ScanFile(string file) {
      if (!File.Exists(file)) {
        Console.WriteLine($"file not found: {file}");
        return;
      }

      if (file.EndsWith(".sln", StringComparison.Ordinal)) {
        ScanSolution(file);
      } else if (file.EndsWith(".csproj", StringComparison.Ordinal)) {
        ScanProject(file);
      } else {
        Console.WriteLine($"invalid file type: {file}");
      }
    }

    public void ScanSolution(string solutionFile) {
      _Scan(solutionFile, (workspace, scanner) => {
        var project = workspace.OpenSolutionAsync(solutionFile).Result;
        scanner.Scan(project, _skipTestProjects);
      });
    }

    public void ScanProject(string projectFile) {
      _Scan(projectFile, (workspace, scanner) => {
        var project = workspace.OpenProjectAsync(projectFile).Result;
        scanner.Scan(project);
      });
    }

    private void _Scan(string fileName, Action<MSBuildWorkspace, Scanner> scanRunner) {
      Console.WriteLine($"opening {Path.GetFileName(fileName)}");
      var workspace = MSBuildWorkspace.Create();
      using (var scanner = _CreateScanner(fileName)) {
        scanRunner(workspace, scanner);
      }
    }

    private Scanner _CreateScanner(string fileName) {
      var reportFileName = _reportFilePattern.Replace("{fileName}", Path.GetFileNameWithoutExtension(fileName));
      return Scanner.Create(_analysisTypes, reportFileName, Console.WriteLine);
    }
  }
}
