using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Verifier;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Verifier {
  [TestClass]
  public class LoopDeclarationVerifierTest {
    private SemanticModel _CreateSemanticModel(string body) {
      var code = $"class Test {{ void Test() {{ {body} }} }}";
      return DocumentFactory.Create().CreateSemanticModel(code);
    }

    private ForStatementSyntax _GetForStatement(SemanticModel semanticModel) {
      return semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<ForStatementSyntax>()
        .Single();
    }

    [TestMethod]
    public void NormalizedDefinitionWithSuffixIncrement() {
      var source = "for(var i = 0; i < 10; i++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void NormalizedDefinitionWithPrefixIncrement() {
      var source = "for(var i = 0; i < 10; ++i) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void NormalizedDefinitionWithAddAssignment() {
      var source = "for(var i = 0; i < 10; i += 1) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void NormalizedDefinitionWithAssignment() {
      var source = "for(var i = 0; i < 10; i = i + 1) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithNoSingleStep() {
      var source = "for(var i = 0; i < 10; i += 2) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithDecrement() {
      var source = "for(var i = 0; i < 10; i--) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithInitialiaizer() {
      var source = "for(i = 0; i < 10; i++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithArrayLengthAsUpperBound() {
      var source = @"
var array = new int[10];
for(i = 0; i < array.Length; i++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithMultipleVariables() {
      var source = @"for(int x, i = 0; i < 10; i++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithoutCompoundCondition() {
      var source = @"for(var i = 0; i < 10 && i > -1; i++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithNoIncrementOfLoopIndex() {
      var source = "int x = 0; for(var i = 0; i < 10; x++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionWithoutLoopIndex() {
      var source = "int x = 0; for(var i = 0; x < 10; i++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithMultipleIncrementers() {
      var source = "int x = 0; for(var i = 0; i < 10; i++, x++) {}";
      var semanticModel = _CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionAccessingAutoProperty() {
      var source = @"
class Test {
  public int Property { get; }

  public void Run() {
    for(var i = 0; i < Property; i++) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionAccessingNonAutoProperty() {
      var source = @"
class Test {
  public int Property { get { return 5; } }

  public void Run() {
    for(var i = 0; i < Property; i++) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionAccessingVirtualAutoProperty() {
      var source = @"
class Test {
  public virtual int Property { get; }

  public void Run() {
    for(var i = 0; i < Property; i++) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionAccessingMethod() {
      var source = @"
class Test {
  public int Method() { return 0; }

  public void Run() {
    for(var i = 0; i < Method(); i++) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsFalse(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionAccessingField() {
      var source = @"
class Test {
  private int field = 10;

  public void Run() {
    for(var i = 0; i < field; i++) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }

    [TestMethod]
    public void DefinitionWithConditionAccessingLocalVariable() {
      var source = @"
class Test {
  public void Run() {
    var local = 10;
    for(var i = 0; i < local; i++) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var forStatement = _GetForStatement(semanticModel);
      Assert.IsTrue(LoopDeclarationVerifier.IsNormalized(forStatement, semanticModel));
    }
  }
}
