using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Extensions {
  [TestClass]
  public class PropertyExtensionsTest {
    [TestMethod]
    public void AutoPropertyHasNoSideEffects() {
      var code = @"
class Test { 
  public int Value { get; set; }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PropertyDeclarationSyntax>()
        .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
        .Single();

      Assert.IsTrue(property.IsLocalAutoProperty(semanticModel));
    }

    [TestMethod]
    public void PropertyWithExpressionBodyIsNoAutoProperty() {
      var code = @"
class Test {
  private readonly int _value;

  public int Value => _value;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PropertyDeclarationSyntax>()
        .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
        .Single();

      Assert.IsFalse(property.IsLocalAutoProperty(semanticModel));
    }

    [TestMethod]
    public void VirtualAutoPropertyIsNotLocal() {
      var code = @"
class Test { 
  public virtual int Value { get; set; }

  public void Add(int value) {
    var result = Value + value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PropertyDeclarationSyntax>()
        .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
        .Single();

      Assert.IsFalse(property.IsLocalAutoProperty(semanticModel));
    }

    [TestMethod]
    public void PropertyWithSideEffectsIsNoAutoProperty() {
      var code = @"
class Test {
  private int count;
  public int Count {
    get { return count; }
    set { count = value; }
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PropertyDeclarationSyntax>()
        .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
        .Single();

      Assert.IsFalse(property.IsLocalAutoProperty(semanticModel));
    }

    [TestMethod]
    public void PropertyWithExpressionAccessorIsNoAutoProperty() {
      var code = @"
class Test {
  private int count;
  public int Count {
    get => return count;
    set => count = value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PropertyDeclarationSyntax>()
        .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
        .Single();

      Assert.IsFalse(property.IsLocalAutoProperty(semanticModel));
    }

    [TestMethod]
    public void PropertyWithExpressionBodyTargettingFieldIsNoAutoProperty() {
      var code = @"
class Test {
  private int count;
  public int Count => count;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PropertyDeclarationSyntax>()
        .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
        .Single();

      Assert.IsFalse(property.IsLocalAutoProperty(semanticModel));
    }

    [TestMethod]
    public void ExternallyDefinedPropertyIsNoLocalAutoProperty() {
      var code = @"
using System.Collections.Generic;

class Test {
  public void Compute() {
    var list = new List<int>();
    var size = list.Count;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var property = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<IdentifierNameSyntax>()
        .Where(identifier => identifier.Identifier.Text.Equals("Count"))
        .Select(identifier => semanticModel.GetSymbolInfo(identifier).Symbol as IPropertySymbol)
        .Single();

      Assert.IsNotNull(property);
      Assert.IsFalse(property.IsLocalAutoProperty(semanticModel));
    }
  }
}
