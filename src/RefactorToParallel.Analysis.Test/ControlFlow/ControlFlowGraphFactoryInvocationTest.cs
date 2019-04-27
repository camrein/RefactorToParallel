using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class ControlFlowGraphFactoryInvocationTest : ControlFlowGraphFactoryTestBase {
    [TestMethod]
    public void EmptyDirectInvocation() {
      var source = @"Method();";
      var cfg = CreateControlFlowGraph(source, true);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""CALL Method()"" ];
  ""3"" [ label = ""Invocation: Method()"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""3"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void InvocationWithArgumentWithoutSuccessor() {
      var source = @"Method(1);";
      var cfg = CreateControlFlowGraph(source, true);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $arg_Method_0 = 1"" ];
  ""3"" [ label = ""CALL Method(1)"" ];
  ""4"" [ label = ""Declaration: $arg_Method_0"" ];
  ""5"" [ label = ""Invocation: Method(1)"" ];
  ""1"" -> ""4"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""3"" -> ""5"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void InvocationWithArgumentsAndSuccessor() {
      var source = @"Method(x, 5); y = 5;";
      var cfg = CreateControlFlowGraph(source, true);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $arg_Method_0 = x"" ];
  ""3"" [ label = ""Assignment: $arg_Method_1 = 5"" ];
  ""4"" [ label = ""Assignment: y = 5"" ];
  ""5"" [ label = ""CALL Method(x, 5)"" ];
  ""6"" [ label = ""Declaration: $arg_Method_0"" ];
  ""7"" [ label = ""Declaration: $arg_Method_1"" ];
  ""8"" [ label = ""Invocation: Method(x, 5)"" ];
  ""1"" -> ""6"" [ label = """" ];
  ""2"" -> ""7"" [ label = """" ];
  ""3"" -> ""5"" [ label = """" ];
  ""4"" -> ""0"" [ label = """" ];
  ""5"" -> ""8"" [ label = """" ];
  ""6"" -> ""2"" [ label = """" ];
  ""7"" -> ""3"" [ label = """" ];
  ""8"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void NestedInvocations() {
      var source = @"Outer(Inner1(Lowest()), Inner2(), Inner3());";
      var cfg = CreateControlFlowGraph(source, true);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $arg_Inner1_0 = Lowest()"" ];
  ""3"" [ label = ""Assignment: $arg_Outer_0 = Inner1(Lowest())"" ];
  ""4"" [ label = ""Assignment: $arg_Outer_1 = Inner2()"" ];
  ""5"" [ label = ""Assignment: $arg_Outer_2 = Inner3()"" ];
  ""6"" [ label = ""CALL Inner1(Lowest())"" ];
  ""7"" [ label = ""CALL Inner2()"" ];
  ""8"" [ label = ""CALL Inner3()"" ];
  ""9"" [ label = ""CALL Lowest()"" ];
  ""10"" [ label = ""CALL Outer(Inner1(Lowest()), Inner2(), Inner3())"" ];
  ""11"" [ label = ""Declaration: $arg_Inner1_0"" ];
  ""12"" [ label = ""Declaration: $arg_Outer_0"" ];
  ""13"" [ label = ""Declaration: $arg_Outer_1"" ];
  ""14"" [ label = ""Declaration: $arg_Outer_2"" ];
  ""15"" [ label = ""Invocation: Outer(Inner1(Lowest()), Inner2(), Inner3())"" ];
  ""1"" -> ""9"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""13"" [ label = """" ];
  ""4"" -> ""14"" [ label = """" ];
  ""5"" -> ""10"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
  ""7"" -> ""8"" [ label = """" ];
  ""8"" -> ""12"" [ label = """" ];
  ""9"" -> ""11"" [ label = """" ];
  ""10"" -> ""15"" [ label = """" ];
  ""11"" -> ""2"" [ label = """" ];
  ""12"" -> ""3"" [ label = """" ];
  ""13"" -> ""4"" [ label = """" ];
  ""14"" -> ""5"" [ label = """" ];
  ""15"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }
  }
}
