using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Extensions {
  [TestClass]
  public class MemberExtensionsTest {
    [TestMethod]
    public void AccessWithShortHandForMathPiIsSafe() {
      var code = @"
using System;

class Test { 
  private readonly double value = Math.PI;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var memberAccess = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => declarator.Initializer.Value)
        .Cast<MemberAccessExpressionSyntax>()
        .Single();

      Assert.IsTrue(memberAccess.IsSafeFieldAccess(semanticModel));
    }

    [TestMethod]
    public void AccessWithFullyQualifiedNameForMathPiIsSafe() {
      var code = @"
class Test { 
  private readonly double value = System.Math.PI;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var memberAccess = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => declarator.Initializer.Value)
        .Cast<MemberAccessExpressionSyntax>()
        .Single();

      Assert.IsTrue(memberAccess.IsSafeFieldAccess(semanticModel));
    }

    [TestMethod]
    public void AccessWithUnresolvableSymbolIsNotSafe() {
      var code = @"
class Test { 
  private readonly double value = Math.PI;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var memberAccess = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => declarator.Initializer.Value)
        .Cast<MemberAccessExpressionSyntax>()
        .Single();

      Assert.IsFalse(memberAccess.IsSafeFieldAccess(semanticModel));
    }
  }
}
