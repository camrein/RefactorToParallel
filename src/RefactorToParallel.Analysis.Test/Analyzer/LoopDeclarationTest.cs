using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopDeclarationTest : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void LoopBasicIncrementation() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      var variable = 0;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopAssignmentIncrementation() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i = i + 1) {
      var variable = 0;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopAddAssignmentIncrementation() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i += 1) {
      var variable = 0;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopWhichUsesOriginalAndLocalShadowCannotBeParallelized() {
      var source = @"
class Test {
  private readonly string variable;

  public void Run() {
    for(var i = 0; i < 10; i += 1) {
      var variable = this.variable;
      var shadowed = variable;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopWhichUsesOriginalAndParameterShadowCannotBeParallelized() {
      var source = @"
class Test {
  private readonly string variable;

  public void Run(string variable) {
    for(var i = 0; i < 10; i += 1) {
      var original = this.variable;
      var shadowed = variable;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopWithAutoPropertyConditionCanBeParallelized() {
      var source = @"
class Test {
  public int Max { get; set; }

  public void Run() {
    for(var i = 0; i < Max; i += 1) {
      var variable = 0;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }
  }
}
