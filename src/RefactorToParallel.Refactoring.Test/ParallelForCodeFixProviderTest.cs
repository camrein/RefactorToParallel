using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis;
using RefactorToParallel.Refactoring.Test.TestHelper;
using System.Linq;

namespace RefactorToParallel.Refactoring.Test {

  [TestClass]
  public class ParallelForCodeFixProviderTest : CodeFixVerifier {
    protected override CodeFixProvider GetCSharpCodeFixProvider() {
      return new ParallelizableForCodeFixProvider();
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
      return new ParallelizableForAnalyzer();
    }

    [TestMethod]
    public void FixableDiagnosticIdEqualToForLoopAnalyzerId() {
      Assert.AreEqual(ParallelizableForAnalyzer.DiagnosticId, GetCSharpCodeFixProvider().FixableDiagnosticIds.Single());
    }

    [TestMethod]
    public void FixAllProviderIsBatchFixer() {
      Assert.AreEqual(WellKnownFixAllProviders.BatchFixer, GetCSharpCodeFixProvider().GetFixAllProvider());
    }

    [TestMethod]
    public void ForLoopWithLessThanCondition() {
      var test = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        for(var i = 0; i < 10; ++i)
        {
            var current = i;
        }
    }
}";
      var fixtest = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        Parallel.For(0, 10, i =>
        {
            var current = i;
        });
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void ForLoopWithLessThanEqualsCondition() {
      var test = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        for(var i = 0; i <= 10; ++i)
        {
            var current = i;
        }
    }
}";
      var fixtest = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        Parallel.For(0, 10 + 1, i =>
        {
            var current = i;
        });
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void ForLoopWithMissingNamespace() {
      var test = @"
class Test
{
    public void Method()
    {
        for(var i = 0; i < 10; ++i)
        {
            var current = i;
        }
    }
}";
      var fixtest = @"using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        Parallel.For(0, 10, i =>
        {
            var current = i;
        });
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void ForLoopWithoutBlock() {
      var test = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        var array = new int[10];
        for(var i = 0; i < array.Length; ++i)
            array[i] = i;
    }
}";
      var fixtest = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        var array = new int[10];
        Parallel.For(0, array.Length, i =>
        {
            array[i] = i;
        });
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void NonBlockSingleArgumentInvocationIsConvertedIntoDelegate() {
      var test = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        for(var i = 0; i < 10; ++i)
            Compute(i);
    }

    private void Compute(int i)
    {
    }
}";
      var fixtest = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        Parallel.For(0, 10, Compute);
    }

    private void Compute(int i)
    {
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void NonBlockMultiArgumentInvocationIsConvertedIntoLambda() {
      var test = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        var array = new int[10];
        for(var i = 0; i < array.Length; ++i)
            Compute(array, i);
    }

    private void Compute(int[] target, int i)
    {
        target[i] = i;
    }
}";
      var fixtest = @"
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        var array = new int[10];
        Parallel.For(0, array.Length, i =>
        {
            Compute(array, i);
        });
    }

    private void Compute(int[] target, int i)
    {
        target[i] = i;
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void DocumentWithPreprocessorDirectiveWithoutExistingUsings() {
      var test = @"
#define RUN

class Test
{
    public void Method()
    {
        var array = new int[10];
        for(var i = 0; i < array.Length; ++i)
        {
            var current = i;
        }
    }
}";
      var fixtest = @"
#define RUN

using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        var array = new int[10];
        Parallel.For(0, array.Length, i =>
        {
            var current = i;
        });
    }
}";

      VerifyCSharpFix(test, fixtest);
    }

    [TestMethod]
    public void DocumentWithPreprocessorDirectiveAndExistingUsings() {
      var test = @"
#define RUN

using System;

class Test
{
    public void Method()
    {
        var array = new int[10];
        for(var i = 0; i < array.Length; ++i)
        {
            var current = i;
        }
    }
}";
      var fixtest = @"
#define RUN

using System;
using System.Threading.Tasks;

class Test
{
    public void Method()
    {
        var array = new int[10];
        Parallel.For(0, array.Length, i =>
        {
            var current = i;
        });
    }
}";

      VerifyCSharpFix(test, fixtest);
    }
  }
}
