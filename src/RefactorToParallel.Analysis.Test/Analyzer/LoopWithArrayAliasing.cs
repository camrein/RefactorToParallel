using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopWithArrayAliasing : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void NoConflictThroughInLoopAliasing() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var array2 = array1;
      array1[i + 1] = array2[i + 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }
    [TestMethod]
    public void NoConflictThroughInLoopAliasingWithBranches() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var array2 = array1;
      var array3 = array1;
      if(i > 10) {
        array3 = array2;
      }
      array1[i + 1] = array3[i + 1] + 1 - array2[i + 1];
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictThroughInLoopAliasing() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var array2 = array1;
      array1[i + 1] = array2[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ConflictThroughInLoopAliasingWithBranches() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var array2 = array1;
      var array3 = array1;
      if(i > 10) {
        array3 = array2;
      }
      array1[i + 1] = array3[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NoConflictThroughPreLoopAliasing() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    var array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i + 1] = array2[i + 1] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }

    [TestMethod]
    public void ConflictThroughPreLoopAliasing() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    var array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i + 1] = array2[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NoConflictWithParameterAlias() {
      var source = @"
class Test {
  public void Run(int[] array1) {
    var array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictThroughAliasingMethodParameter() {
      var source = @"
class Test {
  public void Run(int[] array1) {
    var array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i + 1] = array2[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NoConflictWithFieldAlias() {
      var source = @"
class Test {
  private int[] array2;

  public void Run() {
    var array1 = new int[10];
    array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(7, 5));
    }

    [TestMethod]
    public void ConflictThroughFieldAliasing() {
      var source = @"
class Test {
  private int[] array2;

  public void Run() {
    var array1 = new int[10];
    array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i * 2] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ConflictWithPropertyAlias() {
      var source = @"
class Test {
  private int[] array1 = new int[10];
  public int[] Array2 => array1;

  public void Run() {
    var array2 = Array2;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i * 2] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NoConflictWithPropertyAlias() {
      var source = @"
class Test {
  private int[] array1 = new int[10];
  public int[] Array2 => array1;

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      array1[i] = Array2[i] + 1;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void TypeIncompatibleArraysCannotAlias() {
      var source = @"
class Test {
  public byte[] Array1 { get; set; }
  public int[] Array2 { get; set; }

  public void Run() {
    var array1 = Array1;
    var array2 = Array2;
    for(var i = 0; i < 10; ++i) {
      array1[i] = (int)array2[i * 2];
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(8, 5));
    }

    [TestMethod]
    public void ArraysOfDifferentDimensionsCannotAlias() {
      var source = @"
class Test {
  public int[] Array1 { get; set; }
  public int[,] Array2 { get; set; }

  public void Run() {
    var array1 = Array1;
    var array2 = Array2;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i * 2, 0];
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(8, 5));
    }

    [TestMethod]
    public void PossibleAliasingThroughPolymorphism() {
      var source = @"
class Test {
  protected int[] _array1 = new int[10];
  protected int[] _array2 = new int[10];

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      _array1[i] = _array2[i * 2];
    }
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ImpossibleAliasingThroughPolymorphismAsFieldsArePrivate() {
      var source = @"
class Test {
  private int[] _array1 = new int[10];
  private int[] _array2 = new int[10];

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      _array1[i] = _array2[i * 2];
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void MethodParameterAndLocalArrayCreationWithoutAliasing() {
      var source = @"
class Test {
  public void Run(int[] array1) {
    var array2 = new int[10];
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i * 2];
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void MethodParameterAndLocalArrayCreationWithLocalAliasingOfParameter() {
      var source = @"
class Test {
  public void Run(int[] array1) {
    var array2 = array1;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i * 2];
    }
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void MethodParameterAndLocalArrayCreationWithParameterAliasingLocal() {
      var source = @"
class Test {
  public void Run(int[] array1) {
    var array2 = new int[10];
    array1 = array2;
    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i * 2];
    }
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void InLoopOnlyAliasingAndAccessWithoutConflict() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var array2 = array1;
      array1[i] = array1[i] + 2;
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void InLoopOnlyAliasingAndAccessWithConflict() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var array2 = array1;
      array1[i] = array1[i + 1] + 2;
    }
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ExternallyPossiblyAliasingArraysOnlyAccessedThroughInLoopAliasingAndConflicts() {
      var source = @"
class Test {
  public int[] shared1;
  public int[] shared2;

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var array1 = shared1;
      var array2 = shared2;
      array1[i] = array2[i + 1];
    }
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ExternallyPossiblyAliasingArraysOnlyAccessedThroughInLoopAliasingWithoutConflicts() {
      var source = @"
class Test {
  public int[] shared1;
  public int[] shared2;

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var array1 = shared1;
      var array2 = shared2;
      array1[i] = array2[i];
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void AliasingThroughBranchesWithoutConflict() {
      var source = @"
class Test {
  private int[] shared1 = new int[10];
  private int[] shared2 = new int[10];

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var array = shared1;
      if(i > 5) {
        array = shared2;
      }
      array[i] = shared1[i] + shared2[i];
    }
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void AliasingThroughBranchesWithConflict() {
      var source = @"
class Test {
  private int[] shared1 = new int[10];
  private int[] shared2 = new int[10];

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var array = shared1;
      if(i > 5) {
        array = shared2;
      }
      array[i] = shared1[i] + shared2[i + 1];
    }
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void AliasingThroughRefArgument() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    var array2 = new int[10];
    Alias(array1, ref array2);

    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i + 1];
    }
  }

  private void Alias(int[] source, ref int[] target) {
    target = source;
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void AliasingThroughOutArgument() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    var array2 = new int[10];
    Alias(array1, ref array2);

    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i + 1];
    }
  }

  private void Alias(int[] source, out int[] target) {
    target = source;
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void NoAliasingThroughArgument() {
      var source = @"
class Test {
  public void Run() {
    var array1 = new int[10];
    var array2 = new int[10];
    Compute(array1, array2);

    for(var i = 0; i < 10; ++i) {
      array1[i] = array2[i + 1];
    }
  }

  private void Compute(int[] source, int[] target) {
  }
}";

      VerifyDiagnostic(source, new DiagnosticResultLocation(7, 5));
    }

    [TestMethod]
    public void IntersectionWithFieldArrayAliasedThroughMethod() {
      var source = @"
public class Test {
  private int[] field = new int[10];

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var alias = GetAliased();
      field[i] = alias[i + 1];
    }
  }

  public int[] GetAliased() {
    return field;
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void IntersectionWithPropertyArrayAliasedThroughMethod() {
      var source = @"
public class Test {
  public int[] Property { get; set; } = new int[10];

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var alias = GetAliased();
      Property[i] = alias[i + 1];
    }
  }

  public int[] GetAliased() {
    return Property;
  }
}";

      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void IntersectionThroughAliasingBehindObjectToArrayCasts() {
      var source = @"
public class Test {
  public void Run() {
    var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
    object array2 = array1;
    for(var i = 1; i < array1.Length; ++i) {
      var array3 = (int[])array2;
      array1[i] = array3[i - 1];
    }
  }
}";

      VerifyDiagnostic(source);
    }
  }
}
