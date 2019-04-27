using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using System.Linq;
using RefactorToParallel.Analysis.IR.Instructions;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class ControlFlowGraphFactoryBasicTest : ControlFlowGraphFactoryTestBase {

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void UnsupportedStatement() {
      var source = @"goto x;";
      CreateControlFlowGraph(source);
    }

    [TestMethod]
    public void EmptyBlock() {
      var source = @"";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""1"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(2, cfg.Nodes.Count);
      Assert.AreEqual(1, cfg.Edges.Count);
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void SingleDeclaration() {
      var source = @"var variable = 1;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: variable = 1"" ];
  ""3"" [ label = ""Declaration: variable"" ];
  ""1"" -> ""3"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
  ""3"" -> ""2"" [ label = """" ];
}";

      Assert.AreEqual(4, cfg.Nodes.Count);
      Assert.AreEqual(3, cfg.Edges.Count);
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void SingleAssignment() {
      var source = @"variable = 2;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: variable = 2"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(3, cfg.Nodes.Count);
      Assert.AreEqual(2, cfg.Edges.Count);
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void DeclarationWithAssignment() {
      var source = @"var variable = 2; variable = 3;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: variable = 2"" ];
  ""3"" [ label = ""Assignment: variable = 3"" ];
  ""4"" [ label = ""Declaration: variable"" ];
  ""1"" -> ""4"" [ label = """" ];
  ""2"" -> ""3"" [ label = """" ];
  ""3"" -> ""0"" [ label = """" ];
  ""4"" -> ""2"" [ label = """" ];
}";

      Assert.AreEqual(5, cfg.Nodes.Count);
      Assert.AreEqual(4, cfg.Edges.Count);
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void PostfixIncrement() {
      var source = @"i++;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = i + 1"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(3, cfg.Nodes.Count);
      Assert.AreEqual(2, cfg.Edges.Count);
      var assignment = cfg.Nodes.Select(statement => statement.Instruction).OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(AddExpression));
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void PrefixDecrement() {
      var source = @"--i;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: i = i - 1"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(3, cfg.Nodes.Count);
      Assert.AreEqual(2, cfg.Edges.Count);
      var assignment = cfg.Nodes.Select(statement => statement.Instruction).OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(SubtractExpression));
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void AddAssignment() {
      var source = @"a += 2;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: a = a + 2"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(3, cfg.Nodes.Count);
      Assert.AreEqual(2, cfg.Edges.Count);
      var assignment = cfg.Nodes.Select(statement => statement.Instruction).OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(AddExpression));
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void SubtractAssignment() {
      var source = @"n -= 3;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: n = n - 3"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(3, cfg.Nodes.Count);
      Assert.AreEqual(2, cfg.Edges.Count);
      var assignment = cfg.Nodes.Select(statement => statement.Instruction).OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(SubtractExpression));
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }

    [TestMethod]
    public void MultiplyAssignment() {
      var source = @"x *= 5;";
      var cfg = CreateControlFlowGraph(source);
      var expected = @"
digraph cfg {
  ""0"" [ label = ""<End>"" ];
  ""1"" [ label = ""<Start>"" ];
  ""2"" [ label = ""Assignment: x = x * 5"" ];
  ""1"" -> ""2"" [ label = """" ];
  ""2"" -> ""0"" [ label = """" ];
}";

      Assert.AreEqual(3, cfg.Nodes.Count);
      Assert.AreEqual(2, cfg.Edges.Count);
      var assignment = cfg.Nodes.Select(statement => statement.Instruction).OfType<Assignment>().Single();
      Assert.IsInstanceOfType(assignment.Right, typeof(MultiplyExpression));
      Assert.AreEqual(expected.Trim(), cfg.ToDot());
    }
  }
}
