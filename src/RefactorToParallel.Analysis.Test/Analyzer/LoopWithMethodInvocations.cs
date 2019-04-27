using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopWithMethodInvocations : AnalyzerTest<ParallelizableForAnalyzer> {

    [TestMethod]
    public void InvocationOfUtilityMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = ComputeValue(i);
    }
  }

  private int ComputeValue(int x) {
    return x * 2;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void InvocationOfMethodWithOverloads() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = ComputeValue(i);
    }
  }

  private int ComputeValue(int x) {
    return x * 2;
  }

  private double ComputeValue(double x) {
    return x * 2;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void InvocationOfPublicMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = ComputeValue(i);
    }
  }

  public int ComputeValue(int x) {
    return x * 2;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void OnlyArrayAccessesInInvokedMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      Update(array, i);
    }
  }

  private int Update(int[] target, int index) {
    target[index] = index * 4;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void InvocationOfVirtualMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = ComputeValue(i);
    }
  }

  protected virtual int ComputeValue(int x) {
    return x * 2;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void InvocationOfOverriddenMethod() {
      var source = @"
class Test : B {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = ComputeValue(i);
    }
  }

  protected override int ComputeValue(int x) {
    return x * 2;
  }
}

class B {
  protected virtual int ComputeValue(int x) {
    return x;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ArrayAccessesThroughMultipleInvocationsButDifferentIndices() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      var a = i;
      Update(array, i);
      Update(array, i + 1);
    }
  }

  private int Update(int[] target, int index) {
    target[index] = index * 4;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ArrayAccessesWithIntersectionInInvokedMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 9; i++) {
      Update(array, i);
    }
  }

  private int Update(int[] target, int index) {
    target[index] = target[index + 1];
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAndWriteArrayAccessInInvokedMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      Update(array, i);
    }
  }

  private int Update(int[] target, int index) {
    target[index] = target[index];
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictingAccessesThroughRecursiveLoopDependantVariableMutations() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      var x = Position(i);
      array[x] = 5;
    }
  }

  private int Position(int index) {
    if(index < 0) {
      return Position(index + 1);
    }
    return index;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ConflictingAccessesThroughArrayAliasingWithMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      var aliased = Alias(array);
      aliased[i] = aliased[i + 1];
    }
  }

  private int[] Alias(int[] alias) {
    return alias;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SafeAccessesThroughArrayAliasingWithMethod() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      var aliased = Alias(array);
      aliased[i] = aliased[i] + 1;
    }
  }

  private int[] Alias(int[] alias) {
    return alias;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictWithOriginalAndAliased() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 9; i++) {
      var aliased = Alias(array);
      array[i + 1] = aliased[i];
    }
  }

  private int[] Alias(int[] alias) {
    return alias;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ConflictWithDelegatedArrayAccess() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 9; i++) {
      array[i] = 5;
      Delegate(array, i);
    }
  }

  private void Delegate(int[] target, int id) {
    target[id + 1] = id;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SafeAccessWithDelegatedArrayAccess() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 9; i++) {
      array[i] = 5;
      Delegate(array, i);
    }
  }

  private void Delegate(int[] target, int id) {
    target[id] = id;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictThroughAccessToSharedFieldWithLoopDependantVariable() {
      var source = @"
class Test {
  private readonly int[] array = new int[10];

  public void Run() {
    for(var i = 0; i < 9; i++) {
      array[i] = 5;
      Companion(i);
    }
  }

  private void Companion(int i) {
    array[i + 1] = 2;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ConflictThroughAccessToSharedFieldWithConstant() {
      var source = @"
class Test {
  private readonly int[] array = new int[10];

  public void Run() {
    for(var i = 0; i < 9; i++) {
      array[i] = 5;
      Companion(i);
    }
  }

  private void Companion(int i) {
    array[0] = 2;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SafeAccessWithMethodComputedIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[20];
    for(var i = 0; i < 10; i++) {
      var x = Index(i);
      array[x] = array[x] + 5;
    }
  }

  private int Index(int i) {
    return i * 2;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void ConflictWithMethodComputedIndex() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[20];
    for(var i = 0; i < 10; i++) {
      var x = Index(i);
      array[x] = array[x] + 5;
    }
  }

  private void Index(int i) {
    if(i < 5) {
      return i;
    }
    return i * 2;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void SharedFieldPassedAsArgument() {
      var source = @"
class Test {
  private readonly int[] array = new int[20];

  public void Run() {
    for(var i = 0; i < 10; i++) {
      Update(array, i);
    }
  }

  private void Update(int[] target, int index) {
    target[index] = index;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }

    [TestMethod]
    public void SharedFieldPassedAsArgumentWithMethodLocalDataManipulation() {
      var source = @"
class Test {
  private readonly int[] array = new int[20];

  public void Run() {
    for(var i = 0; i < 10; i++) {
      Update(array, i);
    }
  }

  private void Update(int[] target, int index) {
    target[index] = index;
    var x = 5;
    x = target[index];
    index = 4;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }

    [TestMethod]
    public void InvocationOfMethodAccessingConstFieldsIsSupported() {
      var source = @"
class Test {
  private const int InitialValue = 5;
  private readonly int[] array = new int[20];

  public void Run() {
    for(var i = 0; i < 10; i++) {
      Reset(array, i);
    }
  }

  private void Reset(int[] target, int index) {
    target[index] = InitialValue;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void InvocationOfMethodAccessingArrayFieldIsNotSupported() {
      var source = @"
class Test {
  private readonly int[] array = new int[20];

  private int initialValue = 5;

  public void Run() {
    initialValue = 2;
    for(var i = 0; i < 10; i++) {
      Reset(i);
    }
  }

  private void Reset(int index) {
    array[index] = initialValue;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void InvocationOfMethodAccessingArrayPropertyIsNotSupported() {
      var source = @"
class Test {
  private int[] Array { get; set; } = new int[20];

  private int initialValue = 5;

  public void Run() {
    initialValue = 2;
    for(var i = 0; i < 10; i++) {
      Reset(i);
    }
  }

  private void Reset(int index) {
    Array[index] = initialValue;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CanParallelizeInvocationsOfGenericMethods() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = array[i] + Identity(1);
      var aliased = Identity(array);
      aliased[i] = aliased[i];
    }
  }

  private T Identity<T>(T arg) {
    return arg;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void CanParallelizeInvocationsOfMethodsWithMultipleSameArgumentInvocations() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 9; i++) {
      array[Position(i)] = array[Position(i)] + 1;
    }
  }

  private int Position(int x) {
    return x + 1;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void CannotParallelizeInvocationsOfMethodsWithMultipleDifferentArgumentInvocations() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 1; i < 8; i++) {
      array[Position(i)] = array[Position(i + 1)] + 1;
    }
  }

  private int Position(int x) {
    return x + 1;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CannotParallelizeInvocationsOfDifferentMethodsWithDifferentResults() {
      var source = @"
class Test {
  public void Run() {
    var array = new int[10];
    for(var i = 1; i < 8; i++) {
      array[Position1(i)] = array[Position2(i + 1)] + 1;
    }
  }

  private int Position1(int x) {
    var y = 1;
    return x + y;
  }

  private int Position2(int x) {
    var y = 2;
    return x + y;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CannotParallelizeInvocationOfMethodAccessingReadonlyFieldArray() {
      var source = @"
class Test {
  private readonly int[] array = new int[10];

  public void Run() {
    for(var i = 1; i < 8; i++) {
      Update(i);
    }
  }

  private void Update(int i) {
    array[i] = i;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CannotParallelizeInvocationOfMethodAccessingReadonlyPropertyArray() {
      var source = @"
class Test {
  private int[] Array { get; } = new int[10];

  public void Run() {
    for(var i = 1; i < 8; i++) {
      Update(i);
    }
  }

  private void Update(int i) {
    Array[i] = i;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CannotParallelizeInvocationOfMethodWritingField() {
      var source = @"
class Test {
  private int value;

  public void Run() {
    for(var i = 1; i < 8; i++) {
      Update(i);
    }
  }

  private void Update(int i) {
    value = i;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CanParallelizeInvocationOfMethodReadingField() {
      var source = @"
class Test {
  private readonly int value;

  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = Compute(i);
    }
  }

  private int Compute(int i) {
    return value * i;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void CannotParallelizeInvocationOfMethodWritingProperty() {
      var source = @"
class Test {
  private int Value { get; set; }

  public void Run() {
    for(var i = 1; i < 8; i++) {
      Update(i);
    }
  }

  private void Update(int i) {
    Value = i;
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void CanParallelizeInvocationOfMethodReadingProperty() {
      var source = @"
class Test {
  private int Value { get; }

  public void Run() {
    var array = new int[10];
    for(var i = 0; i < 10; i++) {
      array[i] = Compute(i);
    }
  }

  private int Compute(int i) {
    return Value * i;
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }
  }
}
