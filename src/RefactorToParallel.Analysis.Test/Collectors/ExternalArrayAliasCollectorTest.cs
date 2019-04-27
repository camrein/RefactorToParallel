using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Collectors;
using RefactorToParallel.Analysis.ControlFlow;
using RefactorToParallel.Analysis.DataFlow.Basic;
using RefactorToParallel.Analysis.IR;
using RefactorToParallel.Analysis.IR.Expressions;
using RefactorToParallel.TestUtil.Code;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Collectors {
  [TestClass]
  public class ExternalArrayAliasCollectorTest {
    private DocumentFactory _documentFactory;

    private ISet<VariableAlias> _CollectAliases(string source) {
      var semanticModel = _documentFactory.CreateSemanticModel(source);
      var forStatement = semanticModel.SyntaxTree.GetRoot().DescendantNodes().OfType<ForStatementSyntax>().Single();

      var code = CodeFactory.Create(forStatement, semanticModel);
      var variableAccesses = VariableAccesses.Collect(code);
      var cfg = ControlFlowGraphFactory.Create(code);

      return ExternalArrayAliasCollector.Collect(semanticModel, forStatement, variableAccesses);
    }

    [TestInitialize]
    public void Initialize() {
      _documentFactory = DocumentFactory.Create();
    }

    [TestMethod]
    public void NoArraysWillLeadToNoAliases() {
      var source = @"
class Test {
  public void Method() {
    for(var i = 0; i < 10; ++i) { }
  }
}";

      var aliases = _CollectAliases(source);
      Assert.AreEqual(0, aliases.Count);
    }

    [TestMethod]
    public void SingleArrayWillAliasItself() {
      var source = @"
class Test {
  public void Method() {
    var array = new int[10];
    for(var i = 0; i < 10; ++i) {
      var current = array[0];
    }
  }
}";

      var aliases = _CollectAliases(source);
      Assert.AreEqual(1, aliases.Count);

      var alias = aliases.Single();
      Assert.AreEqual("array", alias.Source);

      Assert.IsInstanceOfType(alias.Target, typeof(VariableExpression));
      Assert.AreEqual("array", ((VariableExpression)alias.Target).Name);
    }

    [TestMethod]
    public void ParameterWithoutAssignmentCannotAliasLocalInstance() {
      var source = @"
class Test {
  public void Method(int[] array1) {
    var array2 = new int[10];
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ParameterAliasingLocalInstanceDueAssignment() {
      var source = @"
class Test {
  public void Method(int[] array1) {
    var array2 = new int[10];
    array1 = array2;
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void LocalArrayAliasesParameter() {
      var source = @"
class Test {
  public void Method(int[] array1) {
    var array2 = array1;
    array1 = array2;
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PrivateFieldsWithConstantInitializationCannotAliasEachOther() {
      var source = @"
class Test {
  private readonly int[] array1 = new int[10];
  private readonly int[] array2 = new int[10];

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PrivatePropertysWithConstantInitializationCannotAliasEachOther() {
      var source = @"
class Test {
  private int[] Array1 { get; } = new int[10];
  private int[] Array2 { get; } = new int[10];

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = Array1[i] + Array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PublicFieldsMayAliasEachOther() {
      var source = @"
class Test {
  public int[] array1 = new int[10];
  public int[] array2 = new int[10];

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PublicPropertiesMayAliasEachOther() {
      var source = @"
class Test {
  public int[] Array1 { get; set; } = new int[10];
  public int[] Array2 { get; set; } = new int[10];

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = Array1[i] + Array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PublicFieldAndPropertyMayAliasEachOther() {
      var source = @"
class Test {
  public int[] array1 = new int[10];
  public int[] Array2 { get; set; } = new int[10];

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + Array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ParameterMayAliasPrivateField() {
      var source = @"
class Test {
  private int[] array1 = new int[10];

  public void Method(int[] array2) {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ParameterMayAliasPrivateProperty() {
      var source = @"
class Test {
  private int[] Array1 { get; } = new int[10];

  public void Method(int[] array2) {
    for(var i = 0; i < 10; ++i) {
      var a = Array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PrivateFieldHavingNonConstructingAssignmentMayAlias() {
      var source = @"
class Test {
  private readonly int[] array1 = new int[10];
  private int[] array2 = new int[10];

  public int[] Array2 { get => array2; set => array2 = value; }

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PrivateFieldOnlyHavingConstructingAssignmentsCannotAlias() {
      var source = @"
class Test {
  private readonly int[] array1 = new int[10];
  private int[] array2 = new int[10];

  public void Reset() {
    array2 = new int[10];
  }

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PrivatePropertyOnlyHavingConstructingAssignmentsCannotAlias() {
      var source = @"
class Test {
  private readonly int[] array1 = new int[10];
  private int[] Array2 { get; } = new int[10];

  public void Reset() {
    Array2 = new int[10];
  }

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + Array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ExternalArraysOnlyAccessedThroughAliasing() {
      var source = @"
class Test {
  public void Method() {
    var shared1 = new int[10];
    var shared2 = new int[10];

    for(var i = 0; i < 10; ++i) {
      var array1 = shared1;
      var array2 = shared2;
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("shared1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("shared2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ExternalArraysOnlyAccessedThroughAliasingMayAliasEachotherAsWell() {
      var source = @"
class Test {
  private readonly int[] shared1 = new int[10];
  public int[] shared2;

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var array1 = shared1;
      var array2 = shared2;
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("shared1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("shared2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ArraysWithDifferentDimensionsCannotAliasEachother() {
      var source = @"
class Test {
  public int[] array1;
  public int[,] array2;

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i, 0];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void ArraysWithIncompatibleTypesCannotAliasEachother() {
      var source = @"
class Test {
  public int[] array1;
  public byte[] array2;

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreNotEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PublicPropertyMayAliasPrivateField() {
      var source = @"
class Test {
  public int[] Array1 { get; set; }
  private int[] array2;

  public int[] Alias2 => array2;

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = Array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }

    [TestMethod]
    public void PrivatePropertyMayAliasPublicField() {
      var source = @"
class Test {
  private int[] Array1 { get; set; }
  public int[] array2;

  public int[] Alias1 => Array1;

  public void Method() {
    for(var i = 0; i < 10; ++i) {
      var a = Array1[i] + array2[i];
    }
  }
}";

      var aliases = _CollectAliases(source).ToList();
      Assert.AreEqual(2, aliases.Count);
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("Array1")));
      Assert.IsTrue(aliases.Any(alias => alias.Source.Equals("array2")));
      Assert.AreEqual(aliases[0].Target, aliases[1].Target);
    }
  }
}
