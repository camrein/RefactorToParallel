using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.TestUtil.Code;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.ControlFlow {
  [TestClass]
  public class CallGraphFactoryTest {
    public ControlFlowGraph CreateControlFlowGraph(string body) {
      return ControlFlowGraphFactory.Create(TestCodeFactory.CreateCode(body), true);
    }

    private bool _IsExpectedTransfer(FlowNode node, string sourceMethod, string targetMethod, bool transferTo, bool transferBack) {
      return node is FlowTransfer transfer
        && transfer.TransfersTo == transferTo && transfer.TransfersBack == transferBack
        && transfer.SourceMethod.Equals(sourceMethod) && transfer.TargetMethod.Equals(targetMethod);
    }

    private bool _IsExpectedBoundary(FlowNode node, string methodName, FlowKind kind) {
      return node is FlowBoundary boundary
        && boundary.MethodName.Equals(methodName) && boundary.Kind == kind;
    }

    [TestMethod]
    public void BodyWithoutInvocations() {
      var invokerSource = @"var a = 10;";
      var invokerCfg = CreateControlFlowGraph(invokerSource);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph>();
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      Assert.AreEqual(0, callGraph.Edges.Count);
    }

    [TestMethod]
    public void BodyWithSingleInvocation() {
      var invokerSource = @"var a = 10; Method(a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Method(int x) {
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

      Assert.AreEqual(4, callGraph.Edges.Count);
      Assert.IsTrue(callGraph.Edges.Any(edge => edge.From is FlowInvocation && _IsExpectedTransfer(edge.To, "<root>", "Method", true, false)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "<root>", "Method", true, false) && _IsExpectedBoundary(edge.To, "Method", FlowKind.Start)));

      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedBoundary(edge.From, "Method", FlowKind.End) && _IsExpectedTransfer(edge.To, "Method", "<root>", false, true)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Method", "<root>", false, true) && edge.To is FlowInvocation));

      Debug.WriteLine(new[] { invokerCfg }.Concat(procedureCfgs.Values).ToDot(callGraph));
    }

    [TestMethod]
    public void BodyWithRecursiveInvocation() {
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

      Assert.AreEqual(8, callGraph.Edges.Count);

      // Initial call
      Assert.IsTrue(callGraph.Edges.Any(edge => edge.From is FlowInvocation && _IsExpectedTransfer(edge.To, "<root>", "Recursive", true, false)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "<root>", "Recursive", true, false) && _IsExpectedBoundary(edge.To, "Recursive", FlowKind.Start)));

      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedBoundary(edge.From, "Recursive", FlowKind.End) && _IsExpectedTransfer(edge.To, "Recursive", "<root>", false, true)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive", "<root>", false, true) && edge.To is FlowInvocation));

      // Recursive call
      Assert.IsTrue(callGraph.Edges.Any(edge => edge.From is FlowInvocation && _IsExpectedTransfer(edge.To, "Recursive", "Recursive", true, false)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive", "Recursive", true, false) && _IsExpectedBoundary(edge.To, "Recursive", FlowKind.Start)));

      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedBoundary(edge.From, "Recursive", FlowKind.End) && _IsExpectedTransfer(edge.To, "Recursive", "Recursive", false, true)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive", "Recursive", false, true) && edge.To is FlowInvocation));

      Debug.WriteLine(new[] { invokerCfg }.Concat(procedureCfgs.Values).ToDot(callGraph));
    }

    [TestMethod]
    public void NestedRecursion() {
      var invokerSource = @"var a = 10; Recursive1(a);";
      var invokerCfg = CreateControlFlowGraph(invokerSource);

      var methodSource = @"
class Test {
  private void Recursive1(int x) {
    Recursive2(x);
  }

  private void Recursive2(int x) {
    if(x > 0) {
      Recursive1(x - 1);
    }
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(methodSource);
      var recursive1Code = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Recursive1"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();
      var recursive2Code = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Recursive2"))
        .Select(declaration => CodeFactory.CreateMethod(declaration, semanticModel))
        .Single();

      var recursive1Cfg = ControlFlowGraphFactory.Create(recursive1Code, "Recursive1", true);
      var recursive2Cfg = ControlFlowGraphFactory.Create(recursive2Code, "Recursive2", true);
      var procedureCfgs = new Dictionary<string, ControlFlowGraph> { { "Recursive1", recursive1Cfg }, { "Recursive2", recursive2Cfg } };
      var callGraph = CallGraphFactory.Create(invokerCfg, procedureCfgs);

      Assert.AreEqual(12, callGraph.Edges.Count);

      // Initial call
      Assert.IsTrue(callGraph.Edges.Any(edge => edge.From is FlowInvocation && _IsExpectedTransfer(edge.To, "<root>", "Recursive1", true, false)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "<root>", "Recursive1", true, false) && _IsExpectedBoundary(edge.To, "Recursive1", FlowKind.Start)));

      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedBoundary(edge.From, "Recursive1", FlowKind.End) && _IsExpectedTransfer(edge.To, "Recursive1", "<root>", false, true)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive1", "<root>", false, true) && edge.To is FlowInvocation));

      // Recursive 1 call
      Assert.IsTrue(callGraph.Edges.Any(edge => edge.From is FlowInvocation && _IsExpectedTransfer(edge.To, "Recursive1", "Recursive2", true, false)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive1", "Recursive2", true, false) && _IsExpectedBoundary(edge.To, "Recursive2", FlowKind.Start)));

      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedBoundary(edge.From, "Recursive2", FlowKind.End) && _IsExpectedTransfer(edge.To, "Recursive2", "Recursive1", false, true)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive2", "Recursive1", false, true) && edge.To is FlowInvocation));

      // Recursive 2 call
      Assert.IsTrue(callGraph.Edges.Any(edge => edge.From is FlowInvocation && _IsExpectedTransfer(edge.To, "Recursive2", "Recursive1", true, false)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive2", "Recursive1", true, false) && _IsExpectedBoundary(edge.To, "Recursive1", FlowKind.Start)));

      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedBoundary(edge.From, "Recursive1", FlowKind.End) && _IsExpectedTransfer(edge.To, "Recursive1", "Recursive2", false, true)));
      Assert.IsTrue(callGraph.Edges.Any(edge => _IsExpectedTransfer(edge.From, "Recursive1", "Recursive2", false, true) && edge.To is FlowInvocation));

      Debug.WriteLine(new[] { invokerCfg }.Concat(procedureCfgs.Values).ToDot(callGraph));
    }
  }
}
