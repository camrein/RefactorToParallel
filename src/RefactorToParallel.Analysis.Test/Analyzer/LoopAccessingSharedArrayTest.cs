using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopAccessingSharedArrayTest : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void SingleReadWithConstant() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var first = array[0];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void SingleReadWithLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var current = array[i];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void MultipleReadsWithConstants() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var first = array[0];
      var last = array[9];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void MultipleReadsWithLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 1; i < array.Length-1; ++i) {
      var total = array[i-1] + array[i] + array[i+1];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void SingleReadWithSharedVariableAndLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    var total = array.Length;
    for(var i = 0; i < array.Length; ++i) {
      var value = array[total-i];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }

    [TestMethod]
    public void SingleWriteWithConstant() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[0] = i;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SingleWriteWithLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i] = i;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexOnSamePosition() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i] = array[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexIntersectingPositions() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 1; i < array.Length; ++i) {
      array[i] = array[i - 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexWithIntersectionDueSingleMultiplicationWithZero() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i] = array[i * 0] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

//    [TestMethod]
//    public void ReadAndWriteWithLoopIndexWithoutIntersectionDueMultiplicationWithOne() {
//      var source = @"
//class Test {
//  public void Run() {
//    var array = new int[10];
//    for(var i = 0; i < array.Length; ++i) {
//      array[i] = array[i * 1] + 1;
//    }
//  }
//}";
//      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
//    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexWithIntersectionDueSingleMultiplicationWithTwo() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i * 2] = array[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexWithoutIntersectionDueSameNonZeroMultiplication() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i * 4] = array[i * 4] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexWithIntersectionDueMultiplicationWithZero() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i * 0] = array[i * 0] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteWithLoopIndexWithIntersectionDueMultiplicationWithUnknownVariable() {
      var source = @"
class Test {
  public void Run(int arg) {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i * arg] = array[i * arg] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteWithEqualLoopIndexAndEqualVariable() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var a = i;
      array[i] = array[a] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithIndirectlyEqualLoopDependantVariables() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var a = i;
      var b = i;
      array[a] = array[b] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithInbetweenWrittenLoopDependantVariable() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var a = i;
      array[a] = array[a] + 1;

      ++a;
      var next = array[a];
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteWithSameShiftSize() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i + 1] = array[i + 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteToFieldLevelArray() {
      var source = @"
class Test {
  private readonly int[] array = new int[10];

  public void Run() {
    for(var i = 0; i < this.array.Length; ++i) {
      array[i + 1] = array[i + 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }

    [TestMethod]
    public void ReadAndWriteToArrayParameter() {
      var source = @"
class Test {
  public void Run(int[] array) {
    for(var i = 0; i < array.Length; ++i) {
      array[i] = array[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(3, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithSubtraction() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      array[10 - i] = array[10 - i] + 10;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteWithSubtractionOfNegation() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      var x = -i;
      array[10 - x] = array[10 - x] + 10;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteThroughVariableOfLoopDependantBranch() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < array.Length; ++i) {
      int x = i;
      if(i > 10) {
        x = -i;
      }
      array[x] = array[x] + 10;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteWithObfuscatedAccessors() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      var x = 2 * i;
      var y = x + 1;
      array[2 * i + 1] = array[y] + 2 + array[x + 1];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteAccessWithStringInterpolation() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[100];
    for(var i = 0; i < 100; ++i) {
      array[i] = $""value at {i} is {array[i]}"";
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteAccessWithStringInterpolationAndConflict() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[100];
    for(var i = 0; i < 100; ++i) {
      array[i] = $""value at {i} is {array[i + 1]}"";
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteAccessWithPositionInversion() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      array[100-i] = array[100-i] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteAccessWithNegativeOffset() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 1; i < 101; ++i) {
      array[i - 1] = array[i - 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteAccessWithLoopIndexSubtractingFromLoopDependentVariable() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 1; i < 101; ++i) {
      var x = i * 2;
      array[i - x] = array[i - x] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteAccessWithLoopIndexSubtractionOfNegativeLoopDependentVariable() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 1; i < 101; ++i) {
      var x = -i;
      array[i - x] = array[i - x] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ReadAndWriteAccessWithImplicitelyLoopDependentVariableSubtractedFromLoopIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 1; i < 101; ++i) {
      var x = 0;
      for(var j = 0; j < i; ++j) {
        x = x + 1;
      }
      array[i - x] = array[i - x] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteAccessAfterInfiniteForLoop() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      var x = 0;
      for(;;) { }
      array[x] += 2;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteAccessIntersectingAfterBreakingInfiniteForLoop() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      var x = 0;
      for(;;) {
        if(i > 50) {
          x = 5;
          break;
        }
      }
      array[x] += 2;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteAccessAfterBreakingInfiniteForLoop() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[100];
    for(var i = 0; i < 100; ++i) {
      var x = 0;
      for(;;) {
        if(i > 50) {
          x = i;
          break;
        }
      }
      array[x] += 2;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }
  }
}
