using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.Analyzer {
  [TestClass]
  public class LoopWithSafeApiUsage : AnalyzerTest<ParallelizableForAnalyzer> {
    [TestMethod]
    public void LoopAccessingMathMethod() {
      var source = @"
using System;

class Test {
  public void Run() {
    var array = new int[] { 1, 2, 3, 4, 5, 6 };
    for(var i = 0; i < array.Length; ++i) {
      array[i] = (int)Math.Pow(i, 2);
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void LoopAccessingMathMethodWithArrayElementAsArgument() {
      var source = @"
using System;

class Test {
  public void Run() {
    var array = new int[] { 1, 2, 3, 4, 5, 6 };
    for(var i = 0; i < array.Length; ++i) {
      array[i] = (int)Math.Pow(array[i], 2);
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void LoopAccessingMathMethodWithArrayElementAsArgumentIntersectingIndicies() {
      var source = @"
using System;

class Test {
  public void Run() {
    var array = new int[] { 1, 2, 3, 4, 5, 6 };
    for(var i = 0; i < array.Length; ++i) {
      array[i] = (int)Math.Pow(i, array[i + 1]);
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopUsingMathMethodToComputeArrayIndex() {
      var source = @"
using System;

class Test {
  public void Run() {
    var array = new int[] { 1, 2, 3, 4, 5, 6 };
    for(var i = 0; i < array.Length; ++i) {
      array[(int)Math.Sin(i)] = i;
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopUsingNonSafeConsoleMethod() {
      var source = @"
using System;

class Test {
  public void Run() {
    for(var i = 0; i < 10; ++i) {
      Console.WriteLine(i);
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopInitialiazingArrayWithDefault() {
      var source = @"
class Test {
  public void Run<T>() {
    var array = new T[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i] = default(T);
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void LoopInitialiazingArrayWithTypeof() {
      var source = @"
using System;

class Test {
  public void Run<T>() {
    var array = new Type[10];
    for(var i = 0; i < array.Length; ++i) {
      array[i] = typeof(T);
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(6, 5));
    }

    [TestMethod]
    public void LoopInitialiazingStringParsedToInt() {
      var source = @"
class Test {
  public static void Main(string[] args) {
    var array = new int[args.Length];
    for(var i = 0; i < array.Length; ++i) {
      array[i] = int.Parse(args[i]);
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(4, 5));
    }

    [TestMethod]
    public void LoopTransformingArrayWithConvert() {
      var source = @"
using System;

class Test {
  public static void Main(string[] args) {
    var input = new int[] { 0, 1, 2, 3 };
    var output = new byte[input.Length];
    for(var i = 0; i < output.Length; ++i) {
      output[i] = Convert.ToByte(input[i]); 
    }
  }
}";
      VerifyDiagnostic(source, new DiagnosticResultLocation(7, 5));
    }

    [TestMethod]
    public void LoopPassingWrittenArrayToSafeApi() {
      var source = @"
using System;

class Test {
  public static void Main(string[] args) {
    var data = new byte[] { 0, 1, 2, 3 };
    var transformed = new string[data.Length];
    for(var i = 0; i < transformed.Length; ++i) {
      data[i] = (byte)i;
      transformed[i] = Convert.ToBase64String(data, 0, 4); 
    }
  }
}";
      VerifyDiagnostic(source);
    }

    [TestMethod]
    public void LoopWritingToArrayEncapsulatedInEnumerablePassedToSafeApi() {
      var source = @"
using System.Linq;

class Test {
  public static void Main(string[] args) {
    var array = new string[] { ""a"", ""b"", ""c"", ""d"" };
    var enumerable = array.AsEnumerable();
    for(var i = 0; i < array.Length; ++i) {
      array[i] = string.Join("", "", enumerable);
    }
  }
}";
      VerifyDiagnostic(source);
    }
  }
}
