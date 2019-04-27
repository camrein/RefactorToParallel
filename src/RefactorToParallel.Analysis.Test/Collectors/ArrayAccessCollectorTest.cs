using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Analysis.Collectors;
using RefactorToParallel.Analysis.ControlFlow;
using System.Collections.Generic;
using System.Linq;

namespace RefactorToParallel.Analysis.Test.Collectors {
  [TestClass]
  public class ArrayAccessCollectorTest {
    private ISet<ArrayAccess> _CollectAccesses(string body) {
      var code = TestCodeFactory.CreateCode(body);
      var cfg = ControlFlowGraphFactory.Create(code);
      return ArrayAccessCollector.Collect(cfg);
    }

    [TestMethod]
    public void NoArrayAccesses() {
      var accesses = _CollectAccesses("x = 5");
      Assert.AreEqual(0, accesses.Count);
    }

    [TestMethod]
    public void SingeOneDimensionalReadAccess() {
      var accesses = _CollectAccesses("if(array[(x)] > 0) {}");
      Assert.AreEqual(1, accesses.Count);

      var access = accesses.Single();
      Assert.AreEqual("array", access.Expression.Name);
      Assert.AreEqual(1, access.Expression.Accessors.Count);
      Assert.IsFalse(access.Write);
    }

    [TestMethod]
    public void SingeOneDimensionalWriteAccess() {
      var accesses = _CollectAccesses("shared[x] = 4;");
      Assert.AreEqual(1, accesses.Count);

      var access = accesses.Single();
      Assert.AreEqual("shared", access.Expression.Name);
      Assert.AreEqual(1, access.Expression.Accessors.Count);
      Assert.IsTrue(access.Write);
    }

    [TestMethod]
    public void ReadAndWriteAccess() {
      var accesses = _CollectAccesses("shared[x] = shared[x] + 1;").ToList();
      Assert.AreEqual(2, accesses.Count);

      Assert.AreEqual("shared", accesses[0].Expression.Name);
      Assert.AreEqual(1, accesses[0].Expression.Accessors.Count);
      Assert.IsTrue(accesses[0].Write);

      Assert.AreEqual("shared", accesses[1].Expression.Name);
      Assert.AreEqual(1, accesses[1].Expression.Accessors.Count);
      Assert.IsFalse(accesses[1].Write);
    }

    [TestMethod]
    public void NestedReadWithinWriteAccess() {
      var accesses = _CollectAccesses("shared[shared[x]] = 5;").ToList();
      Assert.AreEqual(2, accesses.Count);

      Assert.AreEqual("shared", accesses[0].Expression.Name);
      Assert.AreEqual(1, accesses[0].Expression.Accessors.Count);
      Assert.IsTrue(accesses[0].Write);

      Assert.AreEqual("shared", accesses[1].Expression.Name);
      Assert.AreEqual(1, accesses[1].Expression.Accessors.Count);
      Assert.IsFalse(accesses[1].Write);
    }

    [TestMethod]
    public void ArrayReadAccessWithinStronglyNestedExpression() {
      var accesses = _CollectAccesses("var b = 1 * (3 + shared[x]) / 4 % 2;").ToList();
      Assert.AreEqual(1, accesses.Count);
      Assert.AreEqual("shared", accesses[0].Expression.Name);
      Assert.AreEqual(1, accesses[0].Expression.Accessors.Count);
      Assert.IsFalse(accesses[0].Write);
    }

    [TestMethod]
    public void ArrayAccessesWithinConditionalExpression() {
      var accesses = _CollectAccesses("var x = a < 0 ? array1[0] : array2[0];").ToList();
      Assert.AreEqual(2, accesses.Count);
      Assert.AreEqual("array1", accesses[0].Expression.Name);
      Assert.AreEqual("array2", accesses[1].Expression.Name);
      Assert.IsFalse(accesses[0].Write);
      Assert.IsFalse(accesses[1].Write);
    }

    [TestMethod]
    public void ArrayAccessesWithinMethodInvocation() {
      var accesses = _CollectAccesses("Method(array1[0], array2[0])").ToList();
      Assert.AreEqual(2, accesses.Count);
      Assert.AreEqual("array1", accesses[0].Expression.Name);
      Assert.AreEqual("array2", accesses[1].Expression.Name);
      Assert.IsFalse(accesses[0].Write);
      Assert.IsFalse(accesses[1].Write);
    }

    [TestMethod]
    public void ArrayAccessesWithinMultipleNestedMethodAccesses() {
      var accesses = _CollectAccesses("Outer(Inner(array1[0]), array2[0])").ToList();
      Assert.AreEqual(2, accesses.Count);
      Assert.AreEqual("array1", accesses[0].Expression.Name);
      Assert.AreEqual("array2", accesses[1].Expression.Name);
      Assert.IsFalse(accesses[0].Write);
      Assert.IsFalse(accesses[1].Write);
    }

    [TestMethod]
    public void ArrayAccessesInGenericBinaryExpressions() {
      var accesses = _CollectAccesses("var x = array[0] >> array[1];").ToList();
      Assert.AreEqual(2, accesses.Count);
      Assert.AreEqual("array", accesses[0].Expression.Name);
      Assert.AreEqual("0", accesses[0].Expression.Accessors.Single().ToString());
      Assert.AreEqual("array", accesses[1].Expression.Name);
      Assert.AreEqual("1", accesses[1].Expression.Accessors.Single().ToString());
      Assert.IsFalse(accesses[0].Write);
      Assert.IsFalse(accesses[1].Write);
    }
  }
}
