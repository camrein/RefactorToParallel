using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Collectors;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Collectors {
  [TestClass]
  public class VariableAccessesTest {
    private VariableAccesses _CollectAccesses(string body) {
      return VariableAccesses.Collect(TestCodeFactory.CreateCode(body));
    }

    [TestMethod]
    public void NoVariableAccessesWithinConstantOnlyExpression() {
      var accesses = _CollectAccesses("if(1 + 5 - 4 + 10 < 0) {}");

      Assert.AreEqual(0, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);
    }

    [TestMethod]
    public void ReadAccessesWithinConstanExpressionsAndSingleVariable() {
      var accesses = _CollectAccesses("if(5 * x + 10 * 15 == 100) {}");

      Assert.AreEqual(1, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
    }

    [TestMethod]
    public void ReadAccessesWithVariablesOnly() {
      var accesses = _CollectAccesses("if(a * (z + 1) > 0) {}");

      Assert.AreEqual(2, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("a"));
      Assert.IsTrue(accesses.ReadVariables.Contains("z"));
    }

    [TestMethod]
    public void ArrayReadAccessWithConstantAccessor() {
      var accesses = _CollectAccesses("if(shared[0] >= 0) {}");

      Assert.AreEqual(1, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(1, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("shared"));
      Assert.AreEqual("shared", accesses.ReadArrays.Single().Name);
    }

    [TestMethod]
    public void ArrayReadAccessWithVariableAccessor() {
      var accesses = _CollectAccesses("if(shared[0 + y] > 0) {}");

      Assert.AreEqual(2, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(1, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("shared"));
      Assert.IsTrue(accesses.ReadVariables.Contains("y"));
      Assert.AreEqual("shared", accesses.ReadArrays.Single().Name);
    }

    [TestMethod]
    public void ReadAccessWithinStronglyNestedExpression() {
      var accesses = _CollectAccesses("var b = 1 * (3 + shared[x]) / 4 % 2;");

      Assert.AreEqual(2, accesses.ReadVariables.Count);
      Assert.AreEqual(1, accesses.WrittenVariables.Count);
      Assert.AreEqual(1, accesses.DeclaredVariables.Count);
      Assert.AreEqual(1, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("shared"));
      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
      Assert.IsTrue(accesses.DeclaredVariables.Contains("b"));
      Assert.IsTrue(accesses.WrittenVariables.Contains("b"));
      Assert.AreEqual("shared", accesses.ReadArrays.Single().Name);
    }

    [TestMethod]
    public void WrittenArrayResolvesIsTrackedAsReadAccessToVariable() {
      var accesses = _CollectAccesses("shared[x] = 1;");

      Assert.AreEqual(2, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.DeclaredVariables.Count);
      Assert.AreEqual(0, accesses.ReadArrays.Count);
      Assert.AreEqual(1, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("shared"));
      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
      Assert.AreEqual("shared", accesses.WrittenArrays.Single().Name);
    }

    [TestMethod]
    public void VariableAccessInsideMethodInvocation() {
      var accesses = _CollectAccesses("Method(x, y, 1);");

      Assert.AreEqual(2, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.DeclaredVariables.Count);
      Assert.AreEqual(0, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
      Assert.IsTrue(accesses.ReadVariables.Contains("y"));
    }

    [TestMethod]
    public void ArrayAccessInsideMethodInvocation() {
      var accesses = _CollectAccesses("Method(array[x], 1);");

      Assert.AreEqual(2, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.DeclaredVariables.Count);
      Assert.AreEqual(1, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
      Assert.IsTrue(accesses.ReadVariables.Contains("array"));
      Assert.AreEqual("array", accesses.ReadArrays.Single().Name);
    }

    [TestMethod]
    public void MethodInvocationInsideArrayAccessWithArrayAccess() {
      var accesses = _CollectAccesses("array1[Method(x)] = array2[Method(array2[y])] + z");

      Assert.AreEqual(5, accesses.ReadVariables.Count);
      Assert.AreEqual(0, accesses.WrittenVariables.Count);
      Assert.AreEqual(0, accesses.DeclaredVariables.Count);
      Assert.AreEqual(2, accesses.ReadArrays.Count);
      Assert.AreEqual(1, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
      Assert.IsTrue(accesses.ReadVariables.Contains("y"));
      Assert.IsTrue(accesses.ReadVariables.Contains("z"));
      Assert.IsTrue(accesses.ReadVariables.Contains("array1"));
      Assert.IsTrue(accesses.ReadVariables.Contains("array2"));
      Assert.AreEqual("array2", accesses.ReadArrays.Select(array => array.Name).Distinct().Single());
      Assert.AreEqual("array1", accesses.WrittenArrays.Single().Name);
    }

    [TestMethod]
    public void CombinationOfBinaryExpressions() {
      var accesses = _CollectAccesses("var n = a << (b ^ x);");

      Assert.AreEqual(3, accesses.ReadVariables.Count);
      Assert.AreEqual(1, accesses.WrittenVariables.Count);
      Assert.AreEqual(1, accesses.DeclaredVariables.Count);
      Assert.AreEqual(0, accesses.ReadArrays.Count);
      Assert.AreEqual(0, accesses.WrittenArrays.Count);

      Assert.IsTrue(accesses.ReadVariables.Contains("a"));
      Assert.IsTrue(accesses.ReadVariables.Contains("b"));
      Assert.IsTrue(accesses.ReadVariables.Contains("x"));
      Assert.AreEqual("n", accesses.WrittenVariables.Single());
      Assert.AreEqual("n", accesses.DeclaredVariables.Single());
    }
  }
}
