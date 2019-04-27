using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class ControlFlowGraphFactoryForTest : ControlFlowGraphFactoryTestBase {
    [TestMethod]
    public void CompletelyEmptyForStatement() {
      var source = @"for(;;) {}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Label"" ];
  ""3"" [ label = ""Label"" ];
  ""4"" [ label = ""Label"" ];
  ""5"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void ForStatementWithoutCondition() {
      var source = @"for(i = 5; ; i = i + 2) { var x = 1; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 5"" ];
  ""3"" [ label = ""Assignment: i = i + 2"" ];
  ""4"" [ label = ""Assignment: x = 1"" ];
  ""5"" [ label = ""Declaration: x"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""6"" [ label = """" ];
  ""4"" -> ""8"" [ label = """" ];
  ""5"" -> ""4"" [ label = """" ];
  ""9"" -> ""0"" [ label = """" ];
  ""8"" -> ""3"" [ label = """" ];
  ""7"" -> ""5"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void EmptyForStatement() {
      var source = @"for(var i = 0; i < 10; i = i + 1) {}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 0"" ];
  ""3"" [ label = ""Assignment: i = i + 1"" ];
  ""4"" [ label = ""ConditionalJump: i < 10"" ];
  ""5"" [ label = ""Declaration: i"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""6"" [ label = """" ];
  ""4"" -> ""7"" [ label = """" ];
  ""4"" -> ""8"" [ label = """" ];
  ""5"" -> ""2"" [ label = """" ];
  ""8"" -> ""0"" [ label = """" ];
  ""9"" -> ""3"" [ label = """" ];
  ""6"" -> ""4"" [ label = """" ];
  ""7"" -> ""9"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void ForStatementWithoutSuccessor() {
      var source = @"for(i = 5; i < 100; i = i + 2) { var x = 1; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 5"" ];
  ""3"" [ label = ""Assignment: i = i + 2"" ];
  ""4"" [ label = ""Assignment: x = 1"" ];
  ""5"" [ label = ""ConditionalJump: i < 100"" ];
  ""6"" [ label = ""Declaration: x"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""7"" [ label = """" ];
  ""3"" -> ""7"" [ label = """" ];
  ""4"" -> ""10"" [ label = """" ];
  ""5"" -> ""8"" [ label = """" ];
  ""5"" -> ""9"" [ label = """" ];
  ""6"" -> ""4"" [ label = """" ];
  ""9"" -> ""0"" [ label = """" ];
  ""10"" -> ""3"" [ label = """" ];
  ""7"" -> ""5"" [ label = """" ];
  ""8"" -> ""6"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void ForStatementWithSuccessor() {
      var source = @"for(int i = 0; i < 100; i = i + 5) { var x = 1; } y = 5;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 0"" ];
  ""3"" [ label = ""Assignment: i = i + 5"" ];
  ""4"" [ label = ""Assignment: x = 1"" ];
  ""5"" [ label = ""Assignment: y = 5"" ];
  ""6"" [ label = ""ConditionalJump: i < 100"" ];
  ""7"" [ label = ""Declaration: i"" ];
  ""8"" [ label = ""Declaration: x"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""11"" [ label = ""Label"" ];
  ""12"" [ label = ""Label"" ];
  ""1"" -> ""7"" [ label = """" ];
  ""2"" -> ""9"" [ label = """" ];
  ""3"" -> ""9"" [ label = """" ];
  ""4"" -> ""12"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
  ""6"" -> ""10"" [ label = """" ];
  ""6"" -> ""11"" [ label = """" ];
  ""7"" -> ""2"" [ label = """" ];
  ""8"" -> ""4"" [ label = """" ];
  ""12"" -> ""3"" [ label = """" ];
  ""11"" -> ""5"" [ label = """" ];
  ""9"" -> ""6"" [ label = """" ];
  ""10"" -> ""8"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void NestedForStatement() {
      var source = @"
for(int i = 0; i < 100; i = i + 1) {
  for(var j = 0; j < 100; j = j + 1) {
    x = x + 0;
  }
}";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 0"" ];
  ""3"" [ label = ""Assignment: i = i + 1"" ];
  ""4"" [ label = ""Assignment: j = 0"" ];
  ""5"" [ label = ""Assignment: j = j + 1"" ];
  ""6"" [ label = ""Assignment: x = x + 0"" ];
  ""7"" [ label = ""ConditionalJump: i < 100"" ];
  ""8"" [ label = ""ConditionalJump: j < 100"" ];
  ""9"" [ label = ""Declaration: i"" ];
  ""10"" [ label = ""Declaration: j"" ];
  ""11"" [ label = ""Label"" ];
  ""12"" [ label = ""Label"" ];
  ""13"" [ label = ""Label"" ];
  ""14"" [ label = ""Label"" ];
  ""15"" [ label = ""Label"" ];
  ""16"" [ label = ""Label"" ];
  ""17"" [ label = ""Label"" ];
  ""18"" [ label = ""Label"" ];
  ""1"" -> ""9"" [ label = """" ];
  ""2"" -> ""11"" [ label = """" ];
  ""3"" -> ""11"" [ label = """" ];
  ""4"" -> ""14"" [ label = """" ];
  ""5"" -> ""14"" [ label = """" ];
  ""6"" -> ""17"" [ label = """" ];
  ""7"" -> ""12"" [ label = """" ];
  ""7"" -> ""13"" [ label = """" ];
  ""8"" -> ""15"" [ label = """" ];
  ""8"" -> ""16"" [ label = """" ];
  ""9"" -> ""2"" [ label = """" ];
  ""10"" -> ""4"" [ label = """" ];
  ""13"" -> ""0"" [ label = """" ];
  ""18"" -> ""3"" [ label = """" ];
  ""17"" -> ""5"" [ label = """" ];
  ""15"" -> ""6"" [ label = """" ];
  ""11"" -> ""7"" [ label = """" ];
  ""14"" -> ""8"" [ label = """" ];
  ""12"" -> ""10"" [ label = """" ];
  ""16"" -> ""18"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void ForStatementWithBranchedContinue() {
      var source = @"for(i = 5; i < 100; i++) { if(i%2 == 0) { continue; } x++; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 5"" ];
  ""3"" [ label = ""Assignment: i = i + 1"" ];
  ""4"" [ label = ""Assignment: x = x + 1"" ];
  ""5"" [ label = ""ConditionalJump: i % 2 == 0"" ];
  ""6"" [ label = ""ConditionalJump: i < 100"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""11"" [ label = ""Label"" ];
  ""12"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""7"" [ label = """" ];
  ""3"" -> ""7"" [ label = """" ];
  ""4"" -> ""12"" [ label = """" ];
  ""5"" -> ""10"" [ label = """" ];
  ""5"" -> ""11"" [ label = """" ];
  ""6"" -> ""8"" [ label = """" ];
  ""6"" -> ""9"" [ label = """" ];
  ""9"" -> ""0"" [ label = """" ];
  ""12"" -> ""3"" [ label = """" ];
  ""11"" -> ""4"" [ label = """" ];
  ""8"" -> ""5"" [ label = """" ];
  ""7"" -> ""6"" [ label = """" ];
  ""10"" -> ""12"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void ForStatementWithBranchedBreak() {
      var source = @"for(i = 5; i < 100; i++) { if(i > 30) { break; } x++; }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 5"" ];
  ""3"" [ label = ""Assignment: i = i + 1"" ];
  ""4"" [ label = ""Assignment: x = x + 1"" ];
  ""5"" [ label = ""ConditionalJump: i < 100"" ];
  ""6"" [ label = ""ConditionalJump: i > 30"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""11"" [ label = ""Label"" ];
  ""12"" [ label = ""Label"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""7"" [ label = """" ];
  ""3"" -> ""7"" [ label = """" ];
  ""4"" -> ""12"" [ label = """" ];
  ""5"" -> ""8"" [ label = """" ];
  ""5"" -> ""9"" [ label = """" ];
  ""6"" -> ""10"" [ label = """" ];
  ""6"" -> ""11"" [ label = """" ];
  ""9"" -> ""0"" [ label = """" ];
  ""12"" -> ""3"" [ label = """" ];
  ""11"" -> ""4"" [ label = """" ];
  ""7"" -> ""5"" [ label = """" ];
  ""8"" -> ""6"" [ label = """" ];
  ""10"" -> ""9"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void ForStatementWithoutConditionButConditionalBreak() {
      var source = @"for(var i = 0; ; ++i) { if(i > 3) { break; } }";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 0"" ];
  ""3"" [ label = ""Assignment: i = i + 1"" ];
  ""4"" [ label = ""ConditionalJump: i > 3"" ];
  ""5"" [ label = ""Declaration: i"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""11"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""6"" [ label = """" ];
  ""4"" -> ""8"" [ label = """" ];
  ""4"" -> ""9"" [ label = """" ];
  ""5"" -> ""2"" [ label = """" ];
  ""10"" -> ""0"" [ label = """" ];
  ""11"" -> ""3"" [ label = """" ];
  ""7"" -> ""4"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
  ""8"" -> ""10"" [ label = """" ];
  ""9"" -> ""11"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }
  }
}
