using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Core.Util;

namespace RefactorToParallel.Core.Test.Util {
  [TestClass]
  public class HashTest {
    [TestMethod]
    public void SingleHashEqual() {
      var test = new object();
      Assert.AreEqual(Hash.With(test).Get(), Hash.With(test).Get());
    }

    [TestMethod]
    public void SingleHashUnequal() {
      Assert.AreNotEqual(Hash.With(new object()).Get(), Hash.With(new object()).Get());
    }

    [TestMethod]
    public void DoubleHashEqual() {
      var test1 = new object();
      var test2 = new object();
      Assert.AreEqual(Hash.With(test1).And(test2).Get(), Hash.With(test1).And(test2).Get());
    }

    [TestMethod]
    public void HashWithNullEqual() {
      var test1 = new object();
      var test2 = new object();
      Assert.AreEqual(Hash.With(test1).And(test2).And((object)null).Get(), Hash.With(test1).And(test2).And((object)null).Get());
    }

    [TestMethod]
    public void HashWithNullUnequal() {
      var test1 = new object();
      var test2 = new object();
      Assert.AreNotEqual(Hash.With(test1).And(test2).And((object)null).Get(), Hash.With(test1).And(new object()).And((object)null).Get());
    }
  }
}
