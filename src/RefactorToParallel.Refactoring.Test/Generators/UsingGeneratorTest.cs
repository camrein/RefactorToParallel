using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Refactoring.Generators;

namespace RefactorToParallel.Refactoring.Test.Generators {
  [TestClass]
  public class UsingGeneratorTest {
    [TestMethod]
    public void AddExistingUsing() {
      var usingToAdd = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Threading"));
      var compilation = SyntaxFactory.CompilationUnit().AddUsings(SyntaxFactory.UsingDirective(usingToAdd));
      Assert.AreEqual("using System.Threading;", compilation.NormalizeWhitespace().ToString());
      Assert.AreEqual("using System.Threading;", UsingGenerator.AddUsingIfMissing(compilation, usingToAdd).NormalizeWhitespace().ToString());
    }

    [TestMethod]
    public void AddNewUsing() {
      var system = SyntaxFactory.IdentifierName("System");
      var usingToAdd = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Threading"));
      var compilation = SyntaxFactory.CompilationUnit().AddUsings(SyntaxFactory.UsingDirective(system));
      Assert.AreEqual("using System;", compilation.NormalizeWhitespace().ToString());
      Assert.AreEqual("using System;\r\nusing System.Threading;", UsingGenerator.AddUsingIfMissing(compilation, usingToAdd).NormalizeWhitespace().ToString());
    }
  }
}
