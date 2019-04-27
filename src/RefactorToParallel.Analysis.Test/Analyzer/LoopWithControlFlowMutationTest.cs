using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopWithControlFlowMutationTest : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void NestedLoopsWithoutControlFlowMutation() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
      }
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5), new DiagnosticResultLocation(4, 7));
    }

    [TestMethod]
    public void LoopWithBreakStatement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      if(i > 0) {
        break;
      }
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NestedLoopsWithBreakStatement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        if(i > 0) {
          break;
        }
      }
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopWithContinueStatement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      if(i > 0) {
        continue;
      }
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NestedLoopsWithContinueStatement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        if(i > 0) {
          continue;
        }
      }
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void LoopWithReturnStatement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      return;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NestedLoopsWithReturnStatement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        if(i > 0) {
          return;
        }
      }
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopWithWriteAccessToLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      if(i > 5) {
        i = 10;
      }
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NestedLoopsWithWriteAccessToInnerLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        if(i > 0) {
          j = 10;
        }
      }
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void NestedLoopsWithWriteAccessToOuterLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; i++) {
      for(var j = 0; j < 10; j++) {
        if(i > 0) {
          i = 10;
        }
      }
    }
  }
}";
      VerifyDiagnostic(source);
    }
  }
}
