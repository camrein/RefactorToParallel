using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.IR {
  [TestClass]
  public class CodeFactorySemanticChecksTest : CodeFactoryTestBase {

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void PreventsSimultaneousUsageOfLocalShadowAndOriginalField() {
      var code = @"
class Test { 
  private int value;

  void Method() {
    var value = this.value;
    var shadowed = value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void PreventsSimultaneousUsageOfParameterShadowAndOriginalField() {
      var code = @"
class Test { 
  private int value;

  void Method(int value) {
    var original = this.value;
    var shadowed = value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsShadowOnlyUsage() {
      var code = @"
class Test { 
  private int value;

  void Method(int value) {
    var shadowed = value;
    shadowed += 1 - value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsOriginalOnlyUsage() {
      var code = @"
class Test { 
  private int value;

  void Method(int value) {
    var original = this.value;
    original += this.value + 1;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsUsageOfPropertyShadow() {
      var code = @"
class Test { 
  private int Value { get; set; }

  void Method(int Value) {
    var shadowed = Value;
    shadowed += 1 - this.Value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsReadOnlyAccessToNonVirtualAutoProperty() {
      var code = @"
class Test { 
  public int Value { get; set; }

  public void Add(int value) {
    var result = Value + value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsReadOnlyAccessToExpressionAutoPropertyTargettingField() {
      var code = @"
class Test {
  private readonly int _value;

  public int Value => _value;

  public void Add(int value) {
    var result = Value + value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhobitsAccessToVirtualAutoProperty() {
      var code = @"
class Test { 
  public virtual int Value { get; set; }

  public void Add(int value) {
    var result = Value + value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhobitsAccessToOverridenAutoProperty() {
      var code = @"
class Test : B { 
  public override int Value { get; set; }

  public void Add(int value) {
    var result = Value + value;
  } 
}

class B {
  public virtual int Value { get; set; }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToExpressionAutoPropertyTargettingOtherProperty() {
      var code = @"
class Test {
  public int Value => Value2;
  public int Value2 { get; set; }

  public void Add(int value) {
    var result = Value + value;
  } 
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToExpressionAutoPropertyTargettingMethod() {
      var code = @"
class Test {
  public int Value => ComputeValue();

  public void Add(int value) {
    var result = Value + value;
  }

  public int ComputeValue() {
    return 5;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Where(declaration => declaration.Identifier.Text.Equals("Add"))
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToPropertyWithExpressionAccessors() {
      var code = @"
class Test {
  public readonly int value;

  public int Value { get => value; }

  public void Add(int value) {
    var result = Value + value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToPropertyWithCustomAccessors() {
      var code = @"
class Test {
  public readonly int value;

  public int Value {
    get { return value; }
  }

  public void Add(int value) {
    var result = Value + value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsAccessToParameter() {
      var code = @"
class Test {
  public void Add(int value) {
    var result = value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToRefParameter() {
      var code = @"
class Test {
  public void Add(ref int value) {
    var result = value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToOutParameter() {
      var code = @"
class Test {
  public void Add(out int value) {
    value = 5;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsAccessToConstMemberFields() {
      var source = @"
using System;

class Test {
  public void Run() {
    var pi = Math.PI;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      var code = CodeFactory.Create(method.Body, semanticModel);
      var expected = @"
DECL pi
pi = \literal";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    public void AllowsAccessToConstMemberFieldsWithFullyQualifiedName() {
      var source = @"
class Test {
  public void Run() {
    var pi = System.Math.PI;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      var code = CodeFactory.Create(method.Body, semanticModel);
      var expected = @"
DECL pi
pi = \literal";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToWritableMemberFields() {
      var source = @"
class Test {
  private static int value;

  public void Run() {
    var v = Test.value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsAccessToReadOnlyMemberFields() {
      var source = @"
class Test {
  private static readonly int value;

  public void Run() {
    var v = Test.value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      var code = CodeFactory.Create(method.Body, semanticModel);
      var expected = @"
DECL v
v = \literal";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsAccessToWritableMemberFieldsWithinChain() {
      var source = @"
class Test {
  private static readonly C c = new C();

  public void Run() {
    var v = Test.c.Value.Value.Value;
  }
}

class A {
  public readonly int Value;
}

class B {
  public A Value;
}

class C {
  public readonly B Value;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();

      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    public void AllowsLongChainOfReadOnlyFields() {
      var source = @"
class Test {
  private static readonly C c = new C();

  public void Run() {
    var v = Test.c.Value.Value.Value;
  }
}

class A {
  public readonly int Value;
}

class B {
  public readonly A Value;
}

class C {
  public readonly B Value;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();


      var code = CodeFactory.Create(method.Body, semanticModel);
      var expected = @"
DECL v
v = \literal";

      Assert.AreEqual(expected.Trim(), CodeStringifier.Generate(code));
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsArrayAccessWithinMemberChain() {
      var source = @"
class Test {
  private static readonly C c = new C();

  public void Run() {
    var v = Test.c.Value.Values[0].Value;
  }
}

class A {
  public readonly int Value;
}

class B {
  public readonly A[] Values;
}

class C {
  public readonly B Value;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();


      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsRetrievalOfArrayElementWithinMemberChain() {
      var source = @"
class Test {
  private static readonly C c = new C();

  public void Run() {
    var v = Test.c.Value.Values[0].Value;
  }
}

class A {
  public readonly int Value;
}

class B {
  public readonly A[] Values;
}

class C {
  public readonly B Value;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();


      CodeFactory.Create(method.Body, semanticModel);
    }

    [TestMethod]
    [ExpectedException(typeof(UnsupportedSyntaxException))]
    public void ProhibitsRetrievalOfArrayWithinMemberChain() {
      var source = @"
class Test {
  private static readonly C c = new C();

  public void Run() {
    var v = Test.c.Value.Value.Values;
  }
}

class A {
  public readonly int[] Values;
}

class B {
  public readonly A Value;
}

class C {
  public readonly B Value;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(source);
      var method = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<MethodDeclarationSyntax>()
        .Single();


      CodeFactory.Create(method.Body, semanticModel);
    }
  }
}
