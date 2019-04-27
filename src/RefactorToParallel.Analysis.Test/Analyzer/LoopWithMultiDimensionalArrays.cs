using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopWithMultiDimensionalArrays : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void ConstantReadAccess() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      var current = shared[0, 0];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConstantWriteAccess() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[0, 0] = 2;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void WriteOnlyAccess() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[i, 0] = 2;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void NoConflictingReadWriteAccessOnFirstDimension() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[i, 0] = shared[i, 0] + 10;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void NoConflictingReadWriteAccessOnSecondimension() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[1, i + 10] = shared[1, i + 10] + 10;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictingReadWriteAccessFirstDimension() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[i, 0] = shared[i + 1, 0] + 10;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ConflictingReadWriteAccessSecondDimension() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[0, i, 0] = shared[0, i + 10, 0] + 10;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteAccessesOnDifferentDimensions() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; ++i) {
      shared[i, 0] = shared[0, i] + 10;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NestedLoopsEachAccessingSingleDimension() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        shared[i, j] = shared[i, j] + 10;
      }
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5), new DiagnosticResultLocation(5, 7));
    }

    [TestMethod]
    public void NestedLoopsAccessingEachOthersDimension() {
      var source = @"
class Test {
  public void Run() {
    var shared = new int[10, 10];
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        shared[i, j] = shared[j, i] + 10;
      }
    }
  }
}";
      VerifyDiagnostic(source);
    }
  }
}
