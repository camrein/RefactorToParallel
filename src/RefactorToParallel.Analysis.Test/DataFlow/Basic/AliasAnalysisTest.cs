using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.TestUtil.Code;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.Basic {
  [TestClass]
  public class AliasAnalysisTest {
    private static FlowNode _GetNodeWithSyntax(ControlFlowGraph cfg, string syntax) {
      return cfg.Nodes.FirstOrDefault(node => string.Equals(node.Instruction.ToString(), syntax));
    }

    public ControlFlowGraph CreateControlFlowGraph(string body) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body), true);
    }

    [TestMethod]
    public void DirectAlias() {
      var source = @"var original = 1; var alias = original;";
      var cfg = CreateControlFlowGraph(source);
      var analysis = AliasAnalysis.Analyze(cfg);

      var originalDeclaration = _GetNodeWithSyntax(cfg, "Assignment: original = 1");
      var aliasDeclaration = _GetNodeWithSyntax(cfg, "Assignment: alias = original");

      Assert.AreEqual("1", analysis.GetAliasesOfVariableAfter("original", originalDeclaration).Single().Target.ToString());
      Assert.AreEqual("1", analysis.GetAliasesOfVariableAfter("original", aliasDeclaration).Single().Target.ToString());
      Assert.AreEqual("1", analysis.GetAliasesOfVariableAfter("alias", aliasDeclaration).Single().Target.ToString());
    }

    [TestMethod]
    public void DirectAliasAfterBranch() {
      var source = @"
var original = 1;
if(original == 1) {
  original = 2;
}
var alias = original;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = AliasAnalysis.Analyze(cfg);

      var aliasDeclaration = _GetNodeWithSyntax(cfg, "Assignment: alias = original");

      var originalAtEnd = analysis.GetAliasesOfVariableAfter("original", aliasDeclaration)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1", "2" }.SetEquals(originalAtEnd));


      var aliasAtEnd = analysis.GetAliasesOfVariableAfter("alias", aliasDeclaration)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1", "2" }.SetEquals(aliasAtEnd));
    }

    [TestMethod]
    public void AliasAfterLoopBranch() {
      var source = @"
var original = 1;
for(var i = 0; i < 10; ++i) {
  original = 2;
}
var alias = original;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = AliasAnalysis.Analyze(cfg);

      var aliasDeclaration = _GetNodeWithSyntax(cfg, "Assignment: alias = original");

      var originalAtEnd = analysis.GetAliasesOfVariableAfter("original", aliasDeclaration)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1", "2" }.SetEquals(originalAtEnd));


      var aliasAtEnd = analysis.GetAliasesOfVariableAfter("alias", aliasDeclaration)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1", "2" }.SetEquals(aliasAtEnd));
    }

    [TestMethod]
    public void IndirectAliasing() {
      var source = @"
var x = 1;
var y = x;
var z = y;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = AliasAnalysis.Analyze(cfg);

      var aliasDeclaration = _GetNodeWithSyntax(cfg, "Assignment: z = y");

      var aliasAtEnd = analysis.GetAliasesOfVariableAfter("z", aliasDeclaration)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1" }.SetEquals(aliasAtEnd));
    }

    [TestMethod]
    public void IndirectAliasingWithIntermediateBeingOverwritten() {
      var source = @"
var x = 1;
var y = x;
var z = y;

y = 2;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = AliasAnalysis.Analyze(cfg);

      var aliasesOfZ = analysis.GetAliasesOfVariableAfter("z", cfg.End)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1" }.SetEquals(aliasesOfZ));

      var overwriting = _GetNodeWithSyntax(cfg, "Assignment: y = 2");
      var aliasBeforeOverWriting = analysis.GetAliasesOfVariableBefore("y", overwriting)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1" }.SetEquals(aliasBeforeOverWriting));
      var aliasAfterOverWriting = analysis.GetAliasesOfVariableAfter("y", overwriting)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "2" }.SetEquals(aliasAfterOverWriting));

      var aliasesOfY = analysis.GetAliasesOfVariableAfter("y", cfg.End)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "2" }.SetEquals(aliasesOfY));
    }

    [TestMethod]
    public void BranchedAliasAssignedToAnotherAlias() {
      var source = @"
var x = 1;
if(x < 0) {
  x = 0;
}

y = x;";

      var cfg = CreateControlFlowGraph(source);
      var analysis = AliasAnalysis.Analyze(cfg);

      var aliasesOfX = analysis.GetAliasesOfVariableAfter("x", cfg.End)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1", "0" }.SetEquals(aliasesOfX));

      var aliasesOfY = analysis.GetAliasesOfVariableAfter("y", cfg.End)
        .Select(alias => alias.Target.ToString()).ToImmutableHashSet();
      Assert.IsTrue(new HashSet<string> { "1", "0" }.SetEquals(aliasesOfY));
    }

    [TestMethod]
    public void SingleArgumentAliasWithoutSuccessor() {
      var invokerSource = @"var x = 1; Method(x);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int y) {
    var z = y;
  }
}";
      //var 
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(2, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("1", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("x")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // invoked method
      Assert.AreEqual(1, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual(rootArgumentAlias, analysis.Entry[methodCfg.Start].Single());

      Assert.AreEqual(3, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("y")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("z")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));
    }

    [TestMethod]
    public void SingleArgumentAliasWithOuterAlias() {
      var invokerSource = @"var x = 1 + 10; var a = x; Method(a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int y) {
    var z = y;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(3, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("1 + 10", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("a")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("x")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // invoked method
      Assert.AreEqual(1, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual(rootArgumentAlias, analysis.Entry[methodCfg.Start].Single());

      Assert.AreEqual(3, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("y")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("z")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));
    }

    [TestMethod]
    public void MultipleArgumentsWithDifferentValues() {
      var invokerSource = @"var a = 10; var b = 20; Method(a, b);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int x, int y) { }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(4, analysis.Exit[invokerCfg.End].Count);
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("20")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("b") && alias.Target.ToString().Equals("20")));

      // invoked method
      Assert.AreEqual(2, analysis.Entry[methodCfg.Start].Count);

      Assert.AreEqual(4, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("20")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("20")));
    }

    [TestMethod]
    public void MultipleArgumentsWithSameValues() {
      var invokerSource = @"var a = 10; var b = 20; Method(a, a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int x, int y) { }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(4, analysis.Exit[invokerCfg.End].Count);
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("b") && alias.Target.ToString().Equals("20")));

      // invoked method
      Assert.AreEqual(2, analysis.Entry[methodCfg.Start].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.Start].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.Start].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("10")));

      Assert.AreEqual(4, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("10")));
    }

    [TestMethod]
    public void MultipleArgumentsWithExchangingValueOrder() {
      var invokerSource = @"var a = 10; var b = 20; var c = 30; Method(a, b, c);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int x, int y, int z) {
    Method(y, z, x);
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(6, analysis.Exit[invokerCfg.End].Count);
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("20")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_2") && alias.Target.ToString().Equals("30")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("b") && alias.Target.ToString().Equals("20")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("c") && alias.Target.ToString().Equals("30")));

      // invoked method
      Assert.AreEqual(9, analysis.Entry[methodCfg.Start].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("20")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_2") && alias.Target.ToString().Equals("30")));

      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("20")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("30")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_2") && alias.Target.ToString().Equals("10")));

      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("30")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_1") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_2") && alias.Target.ToString().Equals("20")));

      Assert.AreEqual(18, analysis.Entry[methodCfg.End].Count);
    }

    [TestMethod]
    public void DoubleInvocationOfSameMethodWithDifferentArguments() {
      var invokerSource = @"Method(1); Method(2);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int y) {
    var z = y;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(1, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("2", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // invoked method
      Assert.AreEqual(2, analysis.Entry[methodCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[methodCfg.Start].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[methodCfg.Start].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("2")));

      Assert.AreEqual(6, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("z") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0") && alias.Target.ToString().Equals("2")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("2")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("z") && alias.Target.ToString().Equals("2")));
    }

    [TestMethod]
    public void RecursiveInvocation() {
      var invokerSource = @"var a = 10; Recursive(a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Recursive(int x) {
    if(x > 0) {
      Recursive(x - 1);
    }
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Recursive", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Recursive", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(2, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("10", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("a")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // invoked method
      Assert.AreEqual(2, analysis.Entry[methodCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[methodCfg.Start].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Entry[methodCfg.Start].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("x - 1")));

      Assert.AreEqual(4, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("10")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("x - 1")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("x - 1")));
    }

    [TestMethod]
    public void NestedRecursion() {
      var invokerSource = @"var n = 5; n = n - 5; Increment(n); Decrement(n);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Increment(int x) {
    if(x < 0) {
      Increment(x + 1);
    } else if(x > 0) {
      Decrement(x);
    }
  }

  private void Decrement(int y) { 
    if(y > 0) {
      Decrement(y - 1);
    } else if(y <0) {
      Increment(y);
    }
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var incrementCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Increment"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var decrementCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Decrement"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();

      var incrementCfg = ControlFlowGraphFactory.Create(incrementCode, "Increment", true);
      var decrementCfg = ControlFlowGraphFactory.Create(decrementCode, "Decrement", true);

      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Increment", incrementCfg }, { "Decrement", decrementCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(3, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("n - 5", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("n")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // increment method
      Assert.AreEqual(3, analysis.Entry[incrementCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[incrementCfg.Start].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("n - 5")));
      Assert.IsTrue(analysis.Entry[incrementCfg.Start].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("x + 1")));
      Assert.IsTrue(analysis.Entry[incrementCfg.Start].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("y - 1")));

      Assert.AreEqual(9, analysis.Exit[incrementCfg.End].Count);
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("n - 5")));
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("n - 5")));
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("n - 5")));

      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("y - 1")));
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("y - 1")));
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("y - 1")));

      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("x + 1")));
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("x + 1")));
      Assert.IsTrue(analysis.Exit[incrementCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("x + 1")));

      // decrement method
      Assert.AreEqual(3, analysis.Entry[incrementCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[decrementCfg.Start].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("n - 5")));
      Assert.IsTrue(analysis.Entry[decrementCfg.Start].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("x + 1")));
      Assert.IsTrue(analysis.Entry[decrementCfg.Start].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("y - 1")));

      Assert.AreEqual(9, analysis.Exit[decrementCfg.End].Count);
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("n - 5")));
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("n - 5")));
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("n - 5")));

      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("y - 1")));
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("y - 1")));
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("y - 1")));

      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("$arg_Increment_0") && alias.Target.ToString().Equals("x + 1")));
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("x + 1")));
      Assert.IsTrue(analysis.Exit[decrementCfg.End].Any(alias => alias.Source.Equals("$arg_Decrement_0") && alias.Target.ToString().Equals("x + 1")));
    }


    [TestMethod]
    public void StronglyNestedAliasPassing() {
      var invokerSource = @"var a = 4 * 5; Start(a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Start(int b) {
    Second(b);
  }

  private void Second(int c) {
    Third(c);
  }

  private void Third(int d) {
    Forth(d);
  }

  private void Forth(int e) {
    Fifth(e);
  }

  private void Fifth(int f) {
    Start(f);
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var startCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Start"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var secondCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Second"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var thirdCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Third"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var forthCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Forth"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var fifthCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Fifth"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();

      var startCfg = ControlFlowGraphFactory.Create(startCode, "Start", true);
      var secondCfg = ControlFlowGraphFactory.Create(secondCode, "Second", true);
      var thirdCfg = ControlFlowGraphFactory.Create(thirdCode, "Third", true);
      var forthCfg = ControlFlowGraphFactory.Create(forthCode, "Forth", true);
      var fifthCfg = ControlFlowGraphFactory.Create(fifthCode, "Fifth", true);

      var procedureCfgs = new Dictionary<string, ControlFlowGraph> {
        { "Start", startCfg },
        { "Second", secondCfg },
        { "Third", thirdCfg },
        { "Forth", forthCfg },
        { "Fifth", fifthCfg },
      };

      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(2, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("4 * 5", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("a")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Start_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // start method
      Assert.AreEqual(1, analysis.Entry[startCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[startCfg.Start].Any(alias => alias.Source.Equals("$arg_Start_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      Assert.AreEqual(3, analysis.Exit[startCfg.End].Count);
      Assert.IsTrue(analysis.Exit[startCfg.End].Any(alias => alias.Source.Equals("$arg_Start_0") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[startCfg.End].Any(alias => alias.Source.Equals("b") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[startCfg.End].Any(alias => alias.Source.Equals("$arg_Second_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      // second method
      Assert.AreEqual(1, analysis.Entry[secondCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[secondCfg.Start].Any(alias => alias.Source.Equals("$arg_Second_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      Assert.AreEqual(3, analysis.Exit[secondCfg.End].Count);
      Assert.IsTrue(analysis.Exit[secondCfg.End].Any(alias => alias.Source.Equals("$arg_Second_0") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[secondCfg.End].Any(alias => alias.Source.Equals("c") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[secondCfg.End].Any(alias => alias.Source.Equals("$arg_Third_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      // third method
      Assert.AreEqual(1, analysis.Entry[thirdCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[thirdCfg.Start].Any(alias => alias.Source.Equals("$arg_Third_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      Assert.AreEqual(3, analysis.Exit[thirdCfg.End].Count);
      Assert.IsTrue(analysis.Exit[thirdCfg.End].Any(alias => alias.Source.Equals("$arg_Third_0") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[thirdCfg.End].Any(alias => alias.Source.Equals("d") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[thirdCfg.End].Any(alias => alias.Source.Equals("$arg_Forth_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      // forth method
      Assert.AreEqual(1, analysis.Entry[forthCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[forthCfg.Start].Any(alias => alias.Source.Equals("$arg_Forth_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      Assert.AreEqual(3, analysis.Exit[forthCfg.End].Count);
      Assert.IsTrue(analysis.Exit[forthCfg.End].Any(alias => alias.Source.Equals("$arg_Forth_0") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[forthCfg.End].Any(alias => alias.Source.Equals("e") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[forthCfg.End].Any(alias => alias.Source.Equals("$arg_Fifth_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      // fifth method
      Assert.AreEqual(1, analysis.Entry[fifthCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[fifthCfg.Start].Any(alias => alias.Source.Equals("$arg_Fifth_0") && alias.Target.Equals(rootArgumentAlias.Target)));

      Assert.AreEqual(3, analysis.Exit[fifthCfg.End].Count);
      Assert.IsTrue(analysis.Exit[fifthCfg.End].Any(alias => alias.Source.Equals("$arg_Fifth_0") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[fifthCfg.End].Any(alias => alias.Source.Equals("f") && alias.Target.Equals(rootArgumentAlias.Target)));
      Assert.IsTrue(analysis.Exit[fifthCfg.End].Any(alias => alias.Source.Equals("$arg_Start_0") && alias.Target.Equals(rootArgumentAlias.Target)));
    }

    [TestMethod]
    public void DirectReturnValueAlias() {
      var invokerSource = @"var x = 1; var y = Method(x);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private int Method(int a) {
    return a;
  }
}";
      //var 
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Method", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(4, analysis.Exit[invokerCfg.End].Count);
      var rootArgumentAlias = analysis.Exit[invokerCfg.End].First();
      Assert.AreEqual("1", rootArgumentAlias.Target.ToString());
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("x")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("y")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Method")));
      Assert.IsTrue(analysis.Exit[invokerCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));

      // invoked method
      Assert.AreEqual(1, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual(rootArgumentAlias.Target, analysis.Entry[methodCfg.Start].Single().Target);

      Assert.AreEqual(3, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Method_0")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("a")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].Any(alias => alias.Source.Equals("$result_Method")));
      Assert.IsTrue(analysis.Exit[methodCfg.End].All(alias => alias.Target.Equals(rootArgumentAlias.Target)));
    }

    [TestMethod]
    public void ReturnValueAliasWithRecursion() {
      var invokerSource = @"var x = 1; var y = Recursive(x);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private int Recursive(int a) {
    if(a > 0) {
      Recursive(a - 1);
    } else if(a < 0) {
      Recursive(a + 1);
    }
    return a;
  }
}";
      //var 
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var methodCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var methodCfg = ControlFlowGraphFactory.Create(methodCode, "Recursive", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Recursive", methodCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(8, analysis.Exit[invokerCfg.End].Count);
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Recursive") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("1")));

      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Recursive") && alias.Target.ToString().Equals("a - 1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("a - 1")));

      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Recursive") && alias.Target.ToString().Equals("a + 1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("a + 1")));

      // invoked method
      Assert.AreEqual(3, analysis.Entry[methodCfg.Start].Count);

      Assert.AreEqual(9, analysis.Exit[methodCfg.End].Count);
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("$result_Recursive") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("1")));

      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("a - 1")));
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("$result_Recursive") && alias.Target.ToString().Equals("a - 1")));
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("a - 1")));

      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("$arg_Recursive_0") && alias.Target.ToString().Equals("a + 1")));
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("$result_Recursive") && alias.Target.ToString().Equals("a + 1")));
      Assert.IsTrue(analysis.Entry[methodCfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("a + 1")));
    }

    [TestMethod]
    public void ReturnValueAliasThroughNestedInvocations() {
      var invokerSource = @"var x = 1; var y = Method1(x);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private int Method1(int a) {
    return Method2(a);
  }

  private int Method2(int b) {
    if(b > 0) {
      return 0;
    }
    return b;
  }
}";
      //var 
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var method1Code = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Method1"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var method2Code = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Method2"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var method1Cfg = ControlFlowGraphFactory.Create(method1Code, "Method1", true);
      var method2Cfg = ControlFlowGraphFactory.Create(method2Code, "Method2", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Method1", method1Cfg }, { "Method2", method2Cfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(6, analysis.Exit[invokerCfg.End].Count);
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Method1_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Method1") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("1")));

      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Method1") && alias.Target.ToString().Equals("0")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("0")));

      // invoked method1
      Assert.AreEqual(1, analysis.Entry[method1Cfg.Start].Count);
      Assert.IsTrue(analysis.Entry[method1Cfg.Start].Any(alias => alias.Source.Equals("$arg_Method1_0") && alias.Target.ToString().Equals("1")));

      Assert.AreEqual(7, analysis.Exit[method1Cfg.End].Count);
      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("$arg_Method1_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("$result_Method1") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("a") && alias.Target.ToString().Equals("1")));

      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("$arg_Method2_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("$result_Method2") && alias.Target.ToString().Equals("1")));

      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("$result_Method1") && alias.Target.ToString().Equals("0")));
      Assert.IsTrue(analysis.Entry[method1Cfg.End].Any(alias => alias.Source.Equals("$result_Method2") && alias.Target.ToString().Equals("0")));

      // invoked method2
      Assert.AreEqual(1, analysis.Entry[method2Cfg.Start].Count);
      Assert.IsTrue(analysis.Entry[method2Cfg.Start].Any(alias => alias.Source.Equals("$arg_Method2_0") && alias.Target.ToString().Equals("1")));

      Assert.AreEqual(4, analysis.Exit[method2Cfg.End].Count);
      Assert.IsTrue(analysis.Entry[method2Cfg.End].Any(alias => alias.Source.Equals("$arg_Method2_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[method2Cfg.End].Any(alias => alias.Source.Equals("b") && alias.Target.ToString().Equals("1")));

      Assert.IsTrue(analysis.Entry[method2Cfg.End].Any(alias => alias.Source.Equals("$result_Method2") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[method2Cfg.End].Any(alias => alias.Source.Equals("$result_Method2") && alias.Target.ToString().Equals("0")));
    }

    [TestMethod]
    public void AliasFromIdentityExpressionBody() {
      var invokerSource = @"var x = 1; var y = Identity(x);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private T Identity<T>(T b) => b;
}";
      //var 
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var identityCode = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var identityCfg = ControlFlowGraphFactory.Create(identityCode, "Identity", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Identity", identityCfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      var analysis = AliasAnalysis.Analyze(invokerCfg, callGraph, procedureCfgs.Values, Enumerable.Empty<VariableAlias>());

      // Outer body
      Assert.AreEqual(0, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual(4, analysis.Exit[invokerCfg.End].Count);
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$arg_Identity_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("$result_Identity") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("x") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[invokerCfg.End].Any(alias => alias.Source.Equals("y") && alias.Target.ToString().Equals("1")));

      // invoked identity
      Assert.AreEqual(1, analysis.Entry[identityCfg.Start].Count);
      Assert.IsTrue(analysis.Entry[identityCfg.Start].Any(alias => alias.Source.Equals("$arg_Identity_0") && alias.Target.ToString().Equals("1")));

      Assert.AreEqual(3, analysis.Exit[identityCfg.End].Count);
      Assert.IsTrue(analysis.Entry[identityCfg.End].Any(alias => alias.Source.Equals("$arg_Identity_0") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[identityCfg.End].Any(alias => alias.Source.Equals("$result_Identity") && alias.Target.ToString().Equals("1")));
      Assert.IsTrue(analysis.Entry[identityCfg.End].Any(alias => alias.Source.Equals("b") && alias.Target.ToString().Equals("1")));
    }
  }
}
