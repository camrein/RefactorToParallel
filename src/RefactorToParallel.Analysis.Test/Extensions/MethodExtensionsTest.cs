using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Extensions {
  [TestClass]
  public class MethodExtensionsTest {
    [TestMethod]
    public void StaticMethodIsNotVirtual() {
      var code = @"
class Test { 
  public static void Run() {}
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      Assert.IsFalse(method.IsVirtual());
    }

    [TestMethod]
    public void ConventionalInstanceMethodIsNotVirtual() {
      var code = @"
class Test { 
  public void Run() {}
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      Assert.IsFalse(method.IsVirtual());
    }

    [TestMethod]
    public void VirtualInstanceMethodIsVirtual() {
      var code = @"
class Test { 
  public virtual void Run() {}
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      Assert.IsTrue(method.IsVirtual());
    }

    [TestMethod]
    public void OverrideInstanceMethodIsVirtual() {
      var code = @"
class Test : A { 
  public override void Run() {}
}

class A {
  public virtual void Run() {}
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<TypeDeclarationSyntax>()
        .Where(type => type.Identifier.Text.Equals("Test"))
        .Single()
        .DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      Assert.IsTrue(method.IsVirtual());
    }

    [TestMethod]
    public void NullIsNotPureApi() {
      IMethodSymbol method = null;
      Assert.IsFalse(method.IsPureApi());
    }

    [TestMethod]
    public void InstanceMethodIsNotPureApi() {
      var code = @"
class Test { 
  public void Run() {
    int value = 10;
    value.Equals(5);
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Select(invocation => semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol)
        .Single();

      Assert.IsNotNull(method);
      Assert.IsFalse(method.IsPureApi());
    }

    [TestMethod]
    public void StaticMethodIsPureApi() {
      var code = @"
class Test { 
  public void Run() {
    var value = int.Parse(""10"");
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Select(invocation => semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol)
        .Single();

      Assert.IsNotNull(method);
      Assert.IsTrue(method.IsPureApi());
    }

    [TestMethod]
    public void StaticMethodAcceptingArraysIsNotPureApi() {
      var code = @"
class Test { 
  public static void Main(string[] args) {
    var value = string.Join("", "", args);
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Select(invocation => semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol)
        .Single();

      Assert.IsNotNull(method);
      Assert.IsFalse(method.IsPureApi());
    }

    [TestMethod]
    public void StaticMethodAcceptingNonPrimitiveArgumentIsNotPureApi() {
      var code = @"
using System.Linq;

class Test { 
  public static void Main(string[] args) {
    var value = string.Join("", "", args.AsEnumerable());
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Select(invocation => semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol)
        .Where(symbol => symbol.Name.Equals("Join"))
        .Single();

      Assert.IsNotNull(method);
      Assert.IsFalse(method.IsPureApi());
    }
  }
}
