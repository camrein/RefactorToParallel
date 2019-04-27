using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.Analysis.IR.Instructions;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactoryMethodTest {
    private Code CreateCode(string methodDeclaration, string methodName) {
      var code = $"class Test {{ {methodDeclaration} }}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals(methodName))
        .Single();

      return CodeFactory.CreateMethod(method, semanticModel);
    }

    [TestMethod]
    public void MethodWithoutParametersAndEmptyBody() {
      var source = @"void Test() {}";
      var code = CreateCode(source, "Test");
      Assert.AreEqual(1, code.Root.Count);
      Assert.AreEqual("LABEL #0", CodeStringifier.Generate(code));
    }


    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void MethodWithRefParameter() {
      var source = @"void Test(ref int a) {}";
      CreateCode(source, "Test");
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void MethodWithOutParameter() {
      var source = @"void Test(out int a) {}";
      CreateCode(source, "Test");
    }

    [TestMethod]
    public void MethodWithSingleParameterButNoBody() {
      var source = @"void Test(int x) {}";
      var code = CreateCode(source, "Test");
      Assert.AreEqual(3, code.Root.Count);

      Assert.IsInstanceOfType(code.Root[0], typeof(Declaration));
      Assert.AreEqual("x", ((Declaration)code.Root[0]).Name);

      Assert.IsInstanceOfType(code.Root[1], typeof(Assignment));
      var assignment = (Assignment)code.Root[1];

      Assert.IsInstanceOfType(assignment.Left, typeof(VariableExpression));
      Assert.AreEqual("x", ((VariableExpression)assignment.Left).Name);

      Assert.IsInstanceOfType(assignment.Right, typeof(VariableExpression));
      Assert.AreEqual("$arg_Test_0", ((VariableExpression)assignment.Right).Name);

      Assert.IsInstanceOfType(code.Root[2], typeof(Label));
    }

    [TestMethod]
    public void MethodWithSingleParameterAndBody() {
      var source = @"void Method(int x) { var y = x; }";
      var code = CreateCode(source, "Method");
      var expected = @"
DECL x
x = $arg_Method_0
DECL y
y = x
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithMultipleParametersAndBody() {
      var source = @"void InvokeMe(int x, string y, string[] z) { var a = y + z[x]; }";
      var code = CreateCode(source, "InvokeMe");
      var expected = @"
DECL x
x = $arg_InvokeMe_0
DECL y
y = $arg_InvokeMe_1
DECL z
z = $arg_InvokeMe_2
DECL a
a = y + z[x]
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithSingleParameterIncludingMethodInvocation() {
      var source = @"void Method1(int x) { Method2(x); } void Method2(int y) {}";
      var code = CreateCode(source, "Method1");
      var expected = @"
DECL x
x = $arg_Method1_0
INVOKE Method2(x)
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithSingleParameterAndBranchedReturnWithoutResult() {
      var source = @"
void Method1(int x) {
  if(x > 0) {
    return;
  }
  x = 5;
}";
      var code = CreateCode(source, "Method1");
      var expected = @"
DECL x
x = $arg_Method1_0
IF x > 0 JUMP #0
JUMP #1
LABEL #0
JUMP #2
LABEL #1
x = 5
LABEL #2";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithReturnValueAndExitReturnStatement() {
      var source = @"int Method1(int x) { return x; }";
      var code = CreateCode(source, "Method1");
      var expected = @"
DECL x
x = $arg_Method1_0
DECL $result_Method1
$result_Method1 = x
JUMP #0
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithReturnValueAndBranchedReturnStatement() {
      var source = @"
int Method1(int x) {
  if(x > 0) {
    return 0;
  }
  return x;
}";
      var code = CreateCode(source, "Method1");
      var expected = @"
DECL x
x = $arg_Method1_0
DECL $result_Method1
IF x > 0 JUMP #0
JUMP #1
LABEL #0
$result_Method1 = 0
JUMP #2
LABEL #1
$result_Method1 = x
JUMP #2
LABEL #2";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithExpressionBodyWithoutArguments() {
      var source = @"int Method1() => 5;";
      var code = CreateCode(source, "Method1");
      var expected = @"
DECL $result_Method1
$result_Method1 = 5
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void MethodWithRecursiveExpressionBody() {
      var source = @"int Test(int x) => x > 0 ? Test(x - 1) : x;";
      var code = CreateCode(source, "Test");
      var expected = @"
DECL x
x = $arg_Test_0
DECL $result_Test
$result_Test = x > 0 ? Test(x - 1) : x
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void NonReturningExpressionBodyIsNotSupported() {
      var source = @"void Test(int x) => x = 5;";
      CreateCode(source, "Test");
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void DoesNotSupportInvocationsOfLambdaExpressions() {
      var code = @"
using System;

class Test {
  private readonly Func<int> Value = () => 1;

  public void Run() {
    var value = Value();
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.CreateMethod(method, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void DoesNotSupportWriteAccessToFields() {
      var code = @"
class Test {
  private int value;

  public void Run() {
    value = 1;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.CreateMethod(method, semanticModel);
    }

    [TestMethod]
    public void SupportsReadAccessFromFields() {
      var source = @"
class Test {
  private readonly int value;

  public void Run() {
    var current = value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      var code = CodeFactory.CreateMethod(method, semanticModel);
      var expected = @"
DECL current
current = value
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void DoesNotSupportWriteAccessToProperties() {
      var code = @"
class Test {
  private int Value { get; set; }

  public void Run() {
    Value = 1;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.CreateMethod(method, semanticModel);
    }

    [TestMethod]
    public void SupportsReadAccessFromProperties() {
      var source = @"
class Test {
  private int Value { get; }

  public void Run() {
    var current = Value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      var code = CodeFactory.CreateMethod(method, semanticModel);
      var expected = @"
DECL current
current = Value
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void PreventsGenerationOfCodeForMethodsAccessingFieldArrays() {
      var source = @"
class Test {
  private readonly int[] array;

  public void Run() {
    var current = array[0];
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.CreateMethod(method, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void PreventsGenerationOfCodeForMethodsAccessingPropertyArrays() {
      var source = @"
class Test {
  private int[] Array { get; }

  public void Run() {
    var current = Array[0];
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.CreateMethod(method, semanticModel);
    }

    [TestMethod]
    public void SupportsUsageOfSafeApis() {
      var source = @"
using System;

class Test {
  public void Run() {
    var current = Math.Pow(2, 4);
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      var code = CodeFactory.CreateMethod(method, semanticModel);
      var expected = @"
DECL current
current = $SafeApi(2, 4)
LABEL #0";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }
  }
}
