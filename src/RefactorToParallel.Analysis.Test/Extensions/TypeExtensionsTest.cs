using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Extensions;
using RefactorToParallel.TestUtil.Code;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Extensions {
  [TestClass]
  public class TypeExtensionsTest {
    [TestMethod]
    public void IntIsPrimitiveType() {
      var code = @"
class Test { 
  private readonly int value = 5;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var field = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => (IFieldSymbol)semanticModel.GetDeclaredSymbol(declarator))
        .Single();

      Assert.IsTrue(field.Type.IsPrimitiveType());
    }

    [TestMethod]
    public void StringIsPrimitiveType() {
      var code = @"
class Test { 
  private readonly string value = 5;
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var field = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => (IFieldSymbol)semanticModel.GetDeclaredSymbol(declarator))
        .Single();

      Assert.IsTrue(field.Type.IsPrimitiveType());
    }

    [TestMethod]
    public void ObjectIsNoPrimitiveType() {
      var code = @"
class Test { 
  private readonly object value = new object();
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var field = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Select(declarator => (IFieldSymbol)semanticModel.GetDeclaredSymbol(declarator))
        .Single();

      Assert.IsFalse(field.Type.IsPrimitiveType());
    }

    [TestMethod]
    public void StringJoinIsMemberOfPrimitiveType() {
      var code = @"
class Test { 
  public void Run() {
    var concatenated = string.Join("", "", new int[] { 1, 2, 3, 4 });
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var invocation = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Single();

      Assert.IsTrue(invocation.IsMemberMethodOfPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void StringConcatenationOperatorNotOverloadedOperator() {
      var code = @"
class Test { 
  public void Run() {
    var concatenated = ""a"" + ""b"";
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void LogicalAndIsNotOverloadable() {
      var code = @"
class Test { 
  public void Run() {
    if(true && true) { }
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void LogicalOrIsNotOverloadable() {
      var code = @"
class Test { 
  public void Run() {
    if(true || true) { }
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void AddAssignmentOfIntegerIsMemberOfPrimitiveType() {
      var code = @"
class Test { 
  public void Run() {
    int value = 0;
    value += 5;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var assignement = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<AssignmentExpressionSyntax>()
        .Single();

      Assert.IsTrue(assignement.IsMemberMethodOfPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void NegationOfIntegerIsMemberOfPrimitiveType() {
      var code = @"
class Test { 
  public void Run() {
    int value = 0;
    value = -5;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var prefix = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PrefixUnaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(prefix.IsMemberMethodOfPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void PrefixDecrementOfIntegerIsNotOverloaded() {
      var code = @"
class Test { 
  public void Run() {
    int value = 0;
    --value;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var prefix = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<PrefixUnaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(prefix.IsMemberMethodOfPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void OverloadedAddExpressionIsRecognized() {
      var code = @"
class Test {
  public void Run() {
    var result = new Test() + new Test();
  }

  public static Test operator +(Test lhv, Test rhv) {
    return new Test();
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void UseOfUnresolvableOperatorIsOverloadadOperator() {
      var code = @"
class Test {
  public void Run() {
    var result = new object() + new object();
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void EqualityOperatorForObjectIsNotOverloaded() {
      var code = @"
class Test {
  public void Run() {
    if(new object() == new object()) {}
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void EqualityOperatorOverloadedCustomType() {
      var code = @"
class Test {
  public void Run() {
    if(new Integer() == new Integer()) {}
  }
}

class Integer {
  public static bool operator ==(Integer lhv, Integer b) {
    return false;
  }

  public static bool operator !=(Integer lhv, Integer b) {
    return false;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void EqualityOperatorCustomTypeWithoutOverloading() {
      var code = @"
class Test {
  public void Run() {
    if(new Integer() == new Integer()) {}
  }
}

class Integer() { }";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsOverloadedBinaryOperator(semanticModel));
    }

    [TestMethod]
    public void StringLiteralIsStringType() {
      var code = @"
class Test {
  public void Run() {
    var x = ""test"";
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var literal = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<LiteralExpressionSyntax>()
        .Single();

      Assert.IsTrue(semanticModel.GetTypeInfo(literal).Type.IsStringType());
    }

    [TestMethod]
    public void IntegerLiteralIsNotStringType() {
      var code = @"
class Test {
  public void Run() {
    var x = 5;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var literal = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<LiteralExpressionSyntax>()
        .Single();

      Assert.IsFalse(semanticModel.GetTypeInfo(literal).Type.IsStringType());
    }

    [TestMethod]
    public void StringVariableIsStringType() {
      var code = @"
class Test {
  public void Run() {
    var x = ""test"";
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var declarator = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Single();

      Assert.IsTrue(((ILocalSymbol)semanticModel.GetDeclaredSymbol(declarator)).Type.IsStringType());
    }

    [TestMethod]
    public void ObjectIsNotStringType() {
      var code = @"
class Test {
  public void Run() {
    var x = new object();
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var declarator = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<VariableDeclaratorSyntax>()
        .Single();

      Assert.IsFalse(((ILocalSymbol)semanticModel.GetDeclaredSymbol(declarator)).Type.IsStringType());
    }

    [TestMethod]
    public void ConcatenationOfTwoStringsEvaluatesToPrimitiveType() {
      var code = @"
class Test {
  public void Run() {
    var x = ""a"" + ""b"";
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(binary.IsEvaluatingToPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void ConcatenationOfStringAndIntegerEvaluatesToPrimitiveType() {
      var code = @"
class Test {
  public void Run() {
    var x = ""a"" + 1;
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsTrue(binary.IsEvaluatingToPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void SumOfTwoObjectsWithAddOverloadsDoesNotEvaluateToPrimitiveType() {
      var code = @"
class Test {
  public void Run() {
    var x =  (new Test() + new Test());
  }

  public static Test operator +(Test lhv, Test b) {
    return new Test();
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsEvaluatingToPrimitiveType(semanticModel));
    }

    [TestMethod]
    public void SumWithUnresolvableOperatorDoesNotEvaluateToPrimitiveType() {
      var code = @"
class Test {
  public void Run() {
    var x =  (new Test() + new Test());
  }
}";
      var semanticModel = DocumentFactory.Create().CreateSemanticModel(code);
      var binary = semanticModel.SyntaxTree.GetRoot().DescendantNodes()
        .OfType<BinaryExpressionSyntax>()
        .Single();

      Assert.IsFalse(binary.IsEvaluatingToPrimitiveType(semanticModel));
    }
  }
}
