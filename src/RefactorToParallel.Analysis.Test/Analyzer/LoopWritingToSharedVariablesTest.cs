using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopWritingToSharedVariablesTest : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void SharedVariableAssignment() {
      var source = @"
class Test {
  public void Run() {
    var shared = 0;
    for(var i = 0; i < 10; ++i) {
      shared = 2;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SharedVariableIncrement() {
      var source = @"
class Test {
  public void Run() {
    var shared = 0;
    for(var i = 0; i < 10; ++i) {
      shared++;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SharedVariableDecrement() {
      var source = @"
class Test {
  public void Run() {
    var shared = 0;
    for(var i = 0; i < 10; ++i) {
      shared--;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SharedVariableAddAssignment() {
      var source = @"
class Test {
  public void Run() {
    var shared = 0;
    for(var i = 0; i < 10; ++i) {
      shared += 2;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopIndexAssignment() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      i = 0;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopIndexDecrement() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      --i;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopIndexMultiplyAssignment() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      i *= 5;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopWithWriteAccessToField() {
      var source = @"
class Test {
  private int value = 2;

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      value = i;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopWithWriteAccessToProperty() {
      var source = @"
class Test {
  private int Value { get; set; } = 2;

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      Value = i;
    }
  }
}";
      VerifyDiagnostic(source);
    }
  }
}
