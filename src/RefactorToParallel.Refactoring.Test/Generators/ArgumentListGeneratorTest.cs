using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Refactoring.Generators;
using System.Collections.Generic;

namespace RefactorToParallel.Refactoring.Test.Generators {
  [TestClass]
  public class ArgumentListGeneratorTest {
    [TestMethod]
    public void CreateArgumentListWithoutExpressions() {
      Assert.AreEqual("()", ArgumentListGenerator.CreateArgumentList().ToString());
    }

    [TestMethod]
    public void CreateArgumentListWithOneExpression() {
      var identifier = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("a"));
      Assert.AreEqual("(a)", ArgumentListGenerator.CreateArgumentList(identifier).ToString());
    }

    [TestMethod]
    public void CreateArgumentListWithThreeExpressions() {
      var arguments = new List<ExpressionSyntax> {
        SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("a")),
        SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("b")),
        SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("c"))
      };
      Assert.AreEqual("(a,b,c)", ArgumentListGenerator.CreateArgumentList(arguments).ToString());
    }
  }
}
