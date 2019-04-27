using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.LoopDependence;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.TestUtil.Code;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.DataFlow.LoopDependence {
  [TestClass]
  public class LoopDependenceInterproceduralTest : LoopDependenceTestBase {
    public override ControlFlowGraph CreateControlFlowGraph(string body) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body), true);
    }

    [TestMethod]
    public void LoopIndexPassingToSingleArgumentMethod() {
      var invokerSource = @"Method(i);";
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

      var analysis = LoopDependenceAnalysis.Analyze(invokerCfg, "i", callGraph, procedureCfgs.Values);

      // Outer body
      Assert.AreEqual(4, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[invokerCfg.Start]));
      Assert.AreEqual(8, analysis.Exit[invokerCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, i != 0, i@, i|, i+}", GetString(analysis.Exit[invokerCfg.End]));

      // invoked method
      Assert.AreEqual(4, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+}", GetString(analysis.Entry[methodCfg.Start]));
      Assert.AreEqual(12, analysis.Exit[methodCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, y != 0, y@, y|, y+, z != 0, z@, z|, z+}", GetString(analysis.Exit[methodCfg.End]));
    }

    [TestMethod]
    public void LoopIndexPassingToSingleArgumentWithReturnValue() {
      var invokerSource = @"var a = Method(i);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int x) {
    return x;
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

      var analysis = LoopDependenceAnalysis.Analyze(invokerCfg, "i", callGraph, procedureCfgs.Values);

      // Outer body
      Assert.AreEqual(4, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[invokerCfg.Start]));
      Assert.AreEqual(16, analysis.Exit[invokerCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, $result_Method != 0, $result_Method@, $result_Method|, $result_Method+, a != 0, a@, a|, a+, i != 0, i@, i|, i+}", GetString(analysis.Exit[invokerCfg.End]));

      // invoked method
      Assert.AreEqual(4, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+}", GetString(analysis.Entry[methodCfg.Start]));
      Assert.AreEqual(12, analysis.Exit[methodCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, $result_Method != 0, $result_Method@, $result_Method|, $result_Method+, x != 0, x@, x|, x+}", GetString(analysis.Exit[methodCfg.End]));
    }

    [TestMethod]
    public void LoopIndexAndRegularPassingWithExchangeRemovesInformation() {
      var invokerSource = @"var a = 0; var b = Recursive(i, a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Recursive(int x, int y) {
    if(x > y) {
      Recursive(y, x);
    }
    return x;
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

      var analysis = LoopDependenceAnalysis.Analyze(invokerCfg, "i", callGraph, procedureCfgs.Values);

      // Outer body
      Assert.AreEqual(4, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[invokerCfg.Start]));
      Assert.AreEqual(16, analysis.Exit[invokerCfg.End].Count);
      Assert.AreEqual("{$arg_Recursive_0 != 0, $arg_Recursive_0@, $arg_Recursive_0|, $arg_Recursive_0+, $arg_Recursive_1 = 0, $arg_Recursive_1/, $arg_Recursive_1@, $result_Recursive@, a = 0, a/, a@, b@, i != 0, i@, i|, i+}", GetString(analysis.Exit[invokerCfg.End]));

      // invoked method
      Assert.AreEqual(4, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual("{$arg_Recursive_0@, $arg_Recursive_0@, $arg_Recursive_1@, $arg_Recursive_1@}", GetString(analysis.Entry[methodCfg.Start]));
      Assert.AreEqual(7, analysis.Exit[methodCfg.End].Count);
      Assert.AreEqual("{$arg_Recursive_0@, $arg_Recursive_0@, $arg_Recursive_1@, $arg_Recursive_1@, $result_Recursive@, x@, y@}", GetString(analysis.Exit[methodCfg.End]));
    }

    [TestMethod]
    public void LoopIndexWithMultipleReturnPoints() {
      var invokerSource = @"var a = Method(i);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private int Method(int x) {
    if(x > 0) {
      return 0;
    }
    return x;
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

      var analysis = LoopDependenceAnalysis.Analyze(invokerCfg, "i", callGraph, procedureCfgs.Values);

      // Outer body
      Assert.AreEqual(4, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[invokerCfg.Start]));
      Assert.AreEqual(11, analysis.Exit[invokerCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, $result_Method@, $result_Method@, a@, i != 0, i@, i|, i+}", GetString(analysis.Exit[invokerCfg.End]));

      // invoked method
      Assert.AreEqual(4, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+}", GetString(analysis.Entry[methodCfg.Start]));
      Assert.AreEqual(10, analysis.Exit[methodCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, $result_Method@, $result_Method@, x != 0, x@, x|, x+}", GetString(analysis.Exit[methodCfg.End]));
    }

    [TestMethod]
    public void LoopIndexLostDueMultipleInvocations() {
      var invokerSource = @"Method(i); Method(0);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private int Method(int x) {
    if(x > 0) {
      return 0;
    }
    return x;
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

      var analysis = LoopDependenceAnalysis.Analyze(invokerCfg, "i", callGraph, procedureCfgs.Values);

      // Outer body
      Assert.AreEqual(4, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[invokerCfg.Start]));
      Assert.AreEqual(9, analysis.Exit[invokerCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 = 0, $arg_Method_0/, $arg_Method_0@, $result_Method@, $result_Method@, i != 0, i@, i|, i+}", GetString(analysis.Exit[invokerCfg.End]));

      // invoked method
      Assert.AreEqual(2, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual("{$arg_Method_0@, $arg_Method_0@}", GetString(analysis.Entry[methodCfg.Start]));
      Debug.WriteLine(GetString(analysis.Exit[methodCfg.End]));
      Assert.AreEqual(5, analysis.Exit[methodCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0@, $arg_Method_0@, $result_Method@, $result_Method@, x@}", GetString(analysis.Exit[methodCfg.End]));
    }

    [TestMethod]
    public void LoopIndexLostThroughRecursion() {
      var invokerSource = @"Method(i);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private int Method(int x) {
    if(x > 0) {
      return Method(x - 1);
    }
    return x;
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

      var analysis = LoopDependenceAnalysis.Analyze(invokerCfg, "i", callGraph, procedureCfgs.Values);

      // Outer body
      Assert.AreEqual(4, analysis.Entry[invokerCfg.Start].Count);
      Assert.AreEqual("{i != 0, i@, i|, i+}", GetString(analysis.Entry[invokerCfg.Start]));
      Assert.AreEqual(10, analysis.Exit[invokerCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0 != 0, $arg_Method_0@, $arg_Method_0|, $arg_Method_0+, $result_Method@, $result_Method@, i != 0, i@, i|, i+}", GetString(analysis.Exit[invokerCfg.End]));

      // invoked method
      Assert.AreEqual(2, analysis.Entry[methodCfg.Start].Count);
      Assert.AreEqual("{$arg_Method_0@, $arg_Method_0@}", GetString(analysis.Entry[methodCfg.Start]));
      Debug.WriteLine(GetString(analysis.Exit[methodCfg.End]));
      Assert.AreEqual(5, analysis.Exit[methodCfg.End].Count);
      Assert.AreEqual("{$arg_Method_0@, $arg_Method_0@, $result_Method@, $result_Method@, x@}", GetString(analysis.Exit[methodCfg.End]));
    }
  }
}
