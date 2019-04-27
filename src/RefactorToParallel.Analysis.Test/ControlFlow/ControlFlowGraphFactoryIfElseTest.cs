using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class ControlFlowGraphFactoryIfElseTest : ControlFlowGraphFactoryTestBase {
    [TestMethod]
    public void EmptyIfStatement() {
      var source = @"if(1 == 1) {}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""3"" [ label = ""Label"" ];
  ""4"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""2"" -> ""4"" [ label = """" ];
  ""4"" -> ""0"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void IfStatementWithoutSuccessor() {
      var source = @"if(1 == 1) { x = 1; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 1"" ];
  ""3"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""4"" [ label = ""Label"" ];
  ""5"" [ label = ""Label"" ];
  ""1"" -> ""3"" [ label = """" ];
  ""2"" -> ""5"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
  ""3"" -> ""5"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void IfStatementWithSuccessor() {
      var source = @"if(1 == 1) { x = 1; } x = 2;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 1"" ];
  ""3"" [ label = ""Assignment: x = 2"" ];
  ""4"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""1"" -> ""4"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""0"" [ label = """" ];
  ""4"" -> ""5"" [ label = """" ];
  ""4"" -> ""6"" [ label = """" ];
  ""5"" -> ""2"" [ label = """" ];
  ""6"" -> ""3"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void EmptyIfElseStatement() {
      var source = @"if(1 == 2) { } else { }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""ConditionalJump: 1 == 2"" ];
  ""3"" [ label = ""Label"" ];
  ""4"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""2"" -> ""4"" [ label = """" ];
  ""4"" -> ""0"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void IfElseStatementWithoutSuccessor() {
      var source = @"if(1 == 1) { x = 1; } else { x = 2; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 1"" ];
  ""3"" [ label = ""Assignment: x = 2"" ];
  ""4"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""1"" -> ""4"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""6"" [ label = """" ];
  ""4"" -> ""3"" [ label = """" ];
  ""4"" -> ""5"" [ label = """" ];
  ""6"" -> ""0"" [ label = """" ];
  ""5"" -> ""2"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void IfElseStatementWithSuccessor() {
      var source = @"if(1 == 1) { x = 1; } else { x = 2; } x = 3;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 1"" ];
  ""3"" [ label = ""Assignment: x = 2"" ];
  ""4"" [ label = ""Assignment: x = 3"" ];
  ""5"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""7"" [ label = """" ];
  ""3"" -> ""7"" [ label = """" ];
  ""4"" -> ""0"" [ label = """" ];
  ""5"" -> ""3"" [ label = """" ];
  ""5"" -> ""6"" [ label = """" ];
  ""6"" -> ""2"" [ label = """" ];
  ""7"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void EmptyIfStatementWithElse() {
      var source = @"if(1 == 5) { } else { x = 2; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 2"" ];
  ""3"" [ label = ""ConditionalJump: 1 == 5"" ];
  ""4"" [ label = ""Label"" ];
  ""5"" [ label = ""Label"" ];
  ""1"" -> ""3"" [ label = """" ];
  ""2"" -> ""5"" [ label = """" ];
  ""3"" -> ""2"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
  ""4"" -> ""5"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void NestedEmptyIfStatement() {
      var source = @"if(1 == 5) { if(1 == 1) {} }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""3"" [ label = ""ConditionalJump: 1 == 5"" ];
  ""4"" [ label = ""Label"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""1"" -> ""3"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""2"" -> ""7"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
  ""3"" -> ""5"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
  ""7"" -> ""5"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void NestedIfElseStatement() {
      var source = @"
if(1 == 5) {
  if(1 == 1) {
    x = 0;
  } else {
    y = 2;
  }
} else {
  x = 5;
}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 0"" ];
  ""3"" [ label = ""Assignment: x = 5"" ];
  ""4"" [ label = ""Assignment: y = 2"" ];
  ""5"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""6"" [ label = ""ConditionalJump: 1 == 5"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""1"" -> ""6"" [ label = """" ];
  ""2"" -> ""10"" [ label = """" ];
  ""3"" -> ""8"" [ label = """" ];
  ""4"" -> ""10"" [ label = """" ];
  ""5"" -> ""4"" [ label = """" ];
  ""5"" -> ""9"" [ label = """" ];
  ""6"" -> ""3"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
  ""8"" -> ""0"" [ label = """" ];
  ""9"" -> ""2"" [ label = """" ];
  ""7"" -> ""5"" [ label = """" ];
  ""10"" -> ""8"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void IfElseIfElseStatement() {
      var source = @"
if(x > 5) {
  more = 1;
} else if(x < 5) {
  less = 1;
} else {
  equal = 1;
}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: equal = 1"" ];
  ""3"" [ label = ""Assignment: less = 1"" ];
  ""4"" [ label = ""Assignment: more = 1"" ];
  ""5"" [ label = ""ConditionalJump: x < 5"" ];
  ""6"" [ label = ""ConditionalJump: x > 5"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""1"" -> ""6"" [ label = """" ];
  ""2"" -> ""9"" [ label = """" ];
  ""3"" -> ""9"" [ label = """" ];
  ""4"" -> ""10"" [ label = """" ];
  ""5"" -> ""2"" [ label = """" ];
  ""5"" -> ""8"" [ label = """" ];
  ""6"" -> ""5"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
  ""10"" -> ""0"" [ label = """" ];
  ""8"" -> ""3"" [ label = """" ];
  ""7"" -> ""4"" [ label = """" ];
  ""9"" -> ""10"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void NestedConditionalExpressions() {
      var source = @"var x = a > 0 ? 1 : (a < 0 ? 1 : 0);";
      var cfg = ControlFlowGraphFactory.Create(TestCodeFactory.CreateThreeAddressCode(source));
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: $temp_0 = $temp_1"" ];
  ""3"" [ label = ""Assignment: $temp_0 = 1"" ];
  ""4"" [ label = ""Assignment: $temp_1 = 0"" ];
  ""5"" [ label = ""Assignment: $temp_1 = 1"" ];
  ""6"" [ label = ""Assignment: x = $temp_0"" ];
  ""7"" [ label = ""ConditionalJump: a < 0"" ];
  ""8"" [ label = ""ConditionalJump: a > 0"" ];
  ""9"" [ label = ""Declaration: $temp_0"" ];
  ""10"" [ label = ""Declaration: $temp_1"" ];
  ""11"" [ label = ""Declaration: x"" ];
  ""12"" [ label = ""Label"" ];
  ""13"" [ label = ""Label"" ];
  ""14"" [ label = ""Label"" ];
  ""15"" [ label = ""Label"" ];
  ""1"" -> ""11"" [ label = """" ];
  ""2"" -> ""15"" [ label = """" ];
  ""3"" -> ""15"" [ label = """" ];
  ""4"" -> ""14"" [ label = """" ];
  ""5"" -> ""14"" [ label = """" ];
  ""6"" -> ""0"" [ label = """" ];
  ""7"" -> ""4"" [ label = """" ];
  ""7"" -> ""13"" [ label = """" ];
  ""8"" -> ""10"" [ label = """" ];
  ""8"" -> ""12"" [ label = """" ];
  ""9"" -> ""8"" [ label = """" ];
  ""10"" -> ""7"" [ label = """" ];
  ""11"" -> ""9"" [ label = """" ];
  ""14"" -> ""2"" [ label = """" ];
  ""12"" -> ""3"" [ label = """" ];
  ""13"" -> ""5"" [ label = """" ];
  ""15"" -> ""6"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }
  }
}
