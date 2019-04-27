using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopSemanticProofsTest : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void ElementAccessToListAreProhibited() {
      var source = @"
using System.Collections.Generic;

class Test {
  public void Run() {
    var list = new List<int>();
    for(var i = 0; i < list.Count; ++i) {
      var current = list[i];
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAccessToAutoProperty() {
      var source = @"
class Test {
  public int Count { get; set; }

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = Count;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(5, 5));
    }

    [TestMethod]
    public void ReadAccessToCustomPropertyAreProhibited() {
      var source = @"
class Test {
  private int count;
  public int Count {
    get { return count; }
    set { count = value; }
  }

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = Count;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAccessToVirtualAutoPropertiesAreProhibited() {
      var source = @"
class Test {
  public virtual int Count { get; set; }

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = Count;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAccessToExpressionCustomPropertyAreProhibited() {
      var source = @"
class Test {
  private int count;
  public int Count {
    get => count;
    set => count = value;
  }

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = Count;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAccessToPropertyWithSideEffectsAreProhibited() {
      var source = @"
class Test {
  private int _count = 0;
  public int Count => _count++;

  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = Count;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAccessToUndefinedVariableAreProhibited() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = _count;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void ReadAccessToUndefinedArrayAreProhibited() {
      var source = @"
class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      var current = array[i];
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void JaggedArraysAreProhibited() {
      var source = @"
class Test {
  public void Run() {
    var inner = new int[10];
    var jagged = new int[][] { inner, inner, inner };

    for(var i = 1; i < jagged.Length; ++i) {
      var nested = jagged[i];
      nested[i] = 5;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void AllowsAccessToApiFields() {
      var source = @"
using System;

class Test {
  public void Run() {
    var array = new double[10];

    for(var i = 1; i < array.Length; ++i) {
      array[i] = i * Math.PI;
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(7, 5));
    }

    [TestMethod]
    public void PreventsIndexComputationWithApiFields() {
      var source = @"
using System;

class Test {
  public void Run() {
    var array = new double[10];

    for(var i = 1; i < array.Length; ++i) {
      array[(int)(i * Math.PI)] = i * Math.PI;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void PreventsUseOfOverloadedBinaryOperator() {
      var source = @"
class Test {
  public void Run() {
    var array = new Integer[] { new Integer(0), new Integer(1), new Integer(2), new Integer(3) };
    var offset = new Integer(10);
    for(var i = 0; i < 10; ++i) {
      array[i] = array[i] + offset;
    }
  }
}

public class Integer {
  private int _value;

  public Integer() : this(0) { }

  public Integer(int value) {
    _value = value;
  }

  public static Integer operator +(Integer lhv, Integer rhv) {
    return new Integer(lhv._value + rhv._value);
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void PreventsUseOfOverloadedCompoundAssignmentOperator() {
      var source = @"
class Test {
  public void Run() {
    var array = new Integer[] { new Integer(0), new Integer(1), new Integer(2), new Integer(3) };
    var offset = new Integer(10);
    for(var i = 0; i < 10; ++i) {
      array[i] += offset;
    }
  }
}

public class Integer {
  private int _value;

  public Integer() : this(0) { }

  public Integer(int value) {
    _value = value;
  }

  public static Integer operator +(Integer lhv, Integer rhv) {
    return new Integer(lhv._value + rhv._value);
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void PreventsUseOfOverloadedUnaryOperator() {
      var source = @"
class Test {
  public void Run() {
    var array = new Integer[] { new Integer(0), new Integer(1), new Integer(2), new Integer(3) };
    var offset = new Integer(10);
    for(var i = 0; i < 10; ++i) {
      array[i]++;
    }
  }
}

public class Integer {
  private int _value;

  public Integer() : this(0) { }

  public Integer(int value) {
    _value = value;
  }

  public static Integer operator ++(Integer lhv) {
    return new Integer(lhv._value + 1);
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void PreventsUseOfOverloadedUnaryExpressionOperator() {
      var source = @"
class Test {
  public void Run() {
    var array = new Integer[] { new Integer(0), new Integer(1), new Integer(2), new Integer(3) };
    var offset = new Integer(10);
    for(var i = 0; i < 10; ++i) {
      array[i] = -array[i];
    }
  }
}

public class Integer {
  private int _value;

  public Integer() : this(0) { }

  public Integer(int value) {
    _value = value;
  }

  public static Integer operator -(Integer lhv) {
    return new Integer(-lhv._value);
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void PreventsUseOfStringConcatenationInBinaryExpressionWithTypesWithPossibleToStringSideEffects() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[10];
    var counter = new Counter();
    for(var i = 0; i < array.Length; ++i) {
      array[i] = ""current: "" + counter;
    }
  }
}

public class Counter {
  private int count;

  public override string ToString() {
    return (++count).ToString();
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void AllowsUseOfStringConcatenationInBinaryExpressionWithPrimitiveTypes() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[10];
    string a = null;
    int b = 1;
    for(var i = 0; i < array.Length; ++i) {
      array[i] = a + b +1 + 2l + ""hello"";
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void PreventsUseOfStringConcatenationInCompoundAssignmentWithTypesWithPossibleToStringSideEffects() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[10];
    var counter = new Counter();
    for(var i = 0; i < array.Length; ++i) {
      array[i] += counter;
    }
  }
}

public class Counter {
  private int count;

  public override string ToString() {
    return (++count).ToString();
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void AllowsUseOfStringConcatenationInCompoundAssignmentWithPrimitiveTypes() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[10];
    string a = null;
    int b = 1;
    for(var i = 0; i < array.Length; ++i) {
      array[i] += a;
      array[i] += b;
      array[i] += 1;
      array[i] += 2l;
      array[i] += ""hello"";
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void PreventsUseOfStringInterpolationWithTypesWithPossibleToStringSideEffects() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[10];
    var counter = new Counter();
    for(var i = 0; i < array.Length; ++i) {
      array[i] = $""current: {counter}"";
    }
  }
}

public class Counter {
  private int count;

  public override string ToString() {
    return (++count).ToString();
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void AllowsUseOfStringInterpolationWithPrimitiveTypes() {
      var source = @"
class Test {
  public void Run() {
    var array = new string[10];
    string a = null;
    int b = 1;
    for(var i = 0; i < array.Length; ++i) {
      array[i] = $""{a} {b} {1} {2l} {""hello""}"";
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }
  }
}
