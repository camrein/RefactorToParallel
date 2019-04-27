using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class ControlFlowGraphFactoryMethodTest {
    public ControlFlowGraph CreateControlFlowGraph(string methodDeclaration) {
      var code = $"class Test {{ {methodDeclaration} }}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();


      return ControlFlowGraphFactory.Create(CodeFactory.CreateMethod(method, semanticModel), true);
    }

    [TestMethod]
    public void EmptyMethodWithoutReturnAndArguments() {
      var source = @"private void Method() {}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void MethodWithoutArgumentsButReturn() {
      var source = @"private int Method() { return 1; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $result_Method = 1"" ];
  ""3"" [ label = ""Declaration: $result_Method"" ];
  ""4"" [ label = ""Label"" ];
  ""1"" -> ""3"" [ label = """" ];
  ""2"" -> ""4"" [ label = """" ];
  ""3"" -> ""2"" [ label = """" ];
  ""4"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void MethodWithBranchedReturnAndArgument() {
      var source = @"
private int Method(int a) {
  if(a > 0) {
    return 0;
  }
  return a;
}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $result_Method = 0"" ];
  ""3"" [ label = ""Assignment: $result_Method = a"" ];
  ""4"" [ label = ""Assignment: a = $arg_Method_0"" ];
  ""5"" [ label = ""ConditionalJump: a > 0"" ];
  ""6"" [ label = ""Declaration: $result_Method"" ];
  ""7"" [ label = ""Declaration: a"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""1"" -> ""7"" [ label = """" ];
  ""2"" -> ""10"" [ label = """" ];
  ""3"" -> ""10"" [ label = """" ];
  ""4"" -> ""6"" [ label = """" ];
  ""5"" -> ""8"" [ label = """" ];
  ""5"" -> ""9"" [ label = """" ];
  ""6"" -> ""5"" [ label = """" ];
  ""7"" -> ""4"" [ label = """" ];
  ""10"" -> ""0"" [ label = """" ];
  ""8"" -> ""2"" [ label = """" ];
  ""9"" -> ""3"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void MethodWithBranchedReturnAndRecursion() {
      var source = @"
private int Method(int a) {
  if(a > 0) {
    return Method(a - 1);
  }
  return a;
}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $arg_Method_0 = a - 1"" ];
  ""3"" [ label = ""Assignment: $result_Method = a"" ];
  ""4"" [ label = ""Assignment: $result_Method = Method(a - 1)"" ];
  ""5"" [ label = ""Assignment: a = $arg_Method_0"" ];
  ""6"" [ label = ""CALL Method(a - 1)"" ];
  ""7"" [ label = ""ConditionalJump: a > 0"" ];
  ""8"" [ label = ""Declaration: $arg_Method_0"" ];
  ""9"" [ label = ""Declaration: $result_Method"" ];
  ""10"" [ label = ""Declaration: a"" ];
  ""11"" [ label = ""Label"" ];
  ""12"" [ label = ""Label"" ];
  ""13"" [ label = ""Label"" ];
  ""1"" -> ""10"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""13"" [ label = """" ];
  ""4"" -> ""13"" [ label = """" ];
  ""5"" -> ""9"" [ label = """" ];
  ""6"" -> ""4"" [ label = """" ];
  ""7"" -> ""11"" [ label = """" ];
  ""7"" -> ""12"" [ label = """" ];
  ""8"" -> ""2"" [ label = """" ];
  ""9"" -> ""7"" [ label = """" ];
  ""10"" -> ""5"" [ label = """" ];
  ""13"" -> ""0"" [ label = """" ];
  ""12"" -> ""3"" [ label = """" ];
  ""11"" -> ""8"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }
  }
}
