using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopLocalWriteSharedReadTest : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void LoopLocalDeclarationWithConstant() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var variable = 0;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopLocalDeclarationWithLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var variable = i;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopLocalDeclarationWithShared() {
      var source = @"
class Test {
  public void Run() {
    var shared = 0;
    for(var i = 0; i < 10; ++i) {
      var variable = shared;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void LoopLocalAssignment() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var variable = 0;
      variable = 5;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopLocalSubtractAssignment() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var variable = 0;
      variable -= 5;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopLocalAssignmentWithExpressionReferencingShared() {
      var source = @"
class Test {
  public void Run() {
    var shared = 0;
    for(var i = 0; i < 10; ++i) {
      var variable = 0;
      variable = 5 * shared + i;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void LoopLocalIncrementation() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var variable = 0;
      ++variable;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }
  }
}
