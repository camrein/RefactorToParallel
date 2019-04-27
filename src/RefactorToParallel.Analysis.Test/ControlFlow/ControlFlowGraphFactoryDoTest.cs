using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class ControlFlowGraphFactoryDoTest : ControlFlowGraphFactoryTestBase {
    [TestMethod]
    public void EmptyWhileStatement() {
      var source = @"do {} while(1 == 1);";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""3"" [ label = ""Label"" ];
  ""4"" [ label = ""Label"" ];
  ""5"" [ label = ""Label"" ];
  ""1"" -> ""3"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""2"" -> ""5"" [ label = """" ];
  ""5"" -> ""0"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
  ""3"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void WhileStatementWithoutSuccessor() {
      var source = @"do { var x = 1; } while(1 == 1);";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 1"" ];
  ""3"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""4"" [ label = ""Declaration: x"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""5"" [ label = """" ];
  ""3"" -> ""7"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
  ""7"" -> ""0"" [ label = """" ];
  ""6"" -> ""3"" [ label = """" ];
  ""5"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void WhileStatementWithSuccessor() {
      var source = @"do { x = 1; } while(1 == 1); y = 5;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = 1"" ];
  ""3"" [ label = ""Assignment: y = 5"" ];
  ""4"" [ label = ""ConditionalJump: 1 == 1"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""6"" [ label = """" ];
  ""3"" -> ""0"" [ label = """" ];
  ""4"" -> ""5"" [ label = """" ];
  ""4"" -> ""7"" [ label = """" ];
  ""5"" -> ""2"" [ label = """" ];
  ""7"" -> ""3"" [ label = """" ];
  ""6"" -> ""4"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void NestedWhileStatement() {
      var source = @"
do {
  do {
    j = j + 1;
  } while(j < 100);
  i = i + 1;
} while(i < 100);";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = i + 1"" ];
  ""3"" [ label = ""Assignment: j = j + 1"" ];
  ""4"" [ label = ""ConditionalJump: i < 100"" ];
  ""5"" [ label = ""ConditionalJump: j < 100"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""10"" [ label = ""Label"" ];
  ""11"" [ label = ""Label"" ];
  ""1"" -> ""6"" [ label = """" ];
  ""2"" -> ""10"" [ label = """" ];
  ""3"" -> ""8"" [ label = """" ];
  ""4"" -> ""6"" [ label = """" ];
  ""4"" -> ""11"" [ label = """" ];
  ""5"" -> ""7"" [ label = """" ];
  ""5"" -> ""9"" [ label = """" ];
  ""11"" -> ""0"" [ label = """" ];
  ""9"" -> ""2"" [ label = """" ];
  ""7"" -> ""3"" [ label = """" ];
  ""10"" -> ""4"" [ label = """" ];
  ""8"" -> ""5"" [ label = """" ];
  ""6"" -> ""7"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void DoStatementWithBranchedContinue() {
      var source = @"do { if(i%2 == 0) { continue; } i = 50; } while(i < 10);";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 50"" ];
  ""3"" [ label = ""ConditionalJump: i % 2 == 0"" ];
  ""4"" [ label = ""ConditionalJump: i < 10"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""8"" [ label = """" ];
  ""3"" -> ""6"" [ label = """" ];
  ""3"" -> ""7"" [ label = """" ];
  ""4"" -> ""5"" [ label = """" ];
  ""4"" -> ""9"" [ label = """" ];
  ""9"" -> ""0"" [ label = """" ];
  ""7"" -> ""2"" [ label = """" ];
  ""5"" -> ""3"" [ label = """" ];
  ""8"" -> ""4"" [ label = """" ];
  ""6"" -> ""8"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void DoStatementWithBranchedBreak() {
      var source = @"do { if(i > 30) { break; } i = 10; } while(i != 10);";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = 10"" ];
  ""3"" [ label = ""ConditionalJump: i != 10"" ];
  ""4"" [ label = ""ConditionalJump: i > 30"" ];
  ""5"" [ label = ""Label"" ];
  ""6"" [ label = ""Label"" ];
  ""7"" [ label = ""Label"" ];
  ""8"" [ label = ""Label"" ];
  ""9"" [ label = ""Label"" ];
  ""1"" -> ""5"" [ label = """" ];
  ""2"" -> ""9"" [ label = """" ];
  ""3"" -> ""5"" [ label = """" ];
  ""3"" -> ""8"" [ label = """" ];
  ""4"" -> ""6"" [ label = """" ];
  ""4"" -> ""7"" [ label = """" ];
  ""8"" -> ""0"" [ label = """" ];
  ""7"" -> ""2"" [ label = """" ];
  ""9"" -> ""3"" [ label = """" ];
  ""5"" -> ""4"" [ label = """" ];
  ""6"" -> ""8"" [ label = """" ];
}";

      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }
  }
}
