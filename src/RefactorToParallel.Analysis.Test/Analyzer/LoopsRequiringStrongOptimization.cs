using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopsRequiringStrongOptimization : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void ArrayWrittenByTwoNestedLoopsWithPreviousIndexComputation() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 1; i < array.Length + 1; ++i) {
      var n = i - 1;

      for(var j = 0; j < 10; ++j) {
        array[i - 1] += 4;
      }

      for(var k = 0; k < 10; ++k) {
        array[i - 1] += 5;
      }
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithStronglyNestedBinaryExpressions() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      array[i + 1 + 1 + 1 + 1] = array[i + 1 + 1 + 1 + 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithExpressionExceedingTheOptimizationLimit() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      array[i + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1] = array[i + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }
  }
}
