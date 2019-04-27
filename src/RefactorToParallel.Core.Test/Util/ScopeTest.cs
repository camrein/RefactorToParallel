using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Core.Util;

namespace RefactorToParallel.Core.Test.Util {
  [TestClass]
  public class ScopeTest {
    [TestMethod]
    public void Scope1Variable() {
      var test = new object();
      var original = test;
      Scope.Scoped(ref test, () => test = new object());
      Assert.AreSame(original, test);
    }

    [TestMethod]
    public void Scope1VariableAndOneNot() {
      var test1 = new object();
      var test2 = new object();

      var original1 = test1;
      var original2 = test2;

      Scope.Scoped(ref test1, () => {
        test1 = new object();
        test2 = new object();
      });

      Assert.AreSame(original1, test1);
      Assert.AreNotSame(original2, test2);
    }

    [TestMethod]
    public void Scope2Variables() {
      var test1 = new object();
      var test2 = new object();

      var original1 = test1;
      var original2 = test2;

      Scope.Scoped(ref test1, ref test2, () => {
        test1 = new object();
        test2 = new object();
      });

      Assert.AreSame(original1, test1);
      Assert.AreSame(original2, test2);
    }

    [TestMethod]
    public void Scope3Variables() {
      var test1 = new object();
      var test2 = new object();
      var test3 = new object();

      var original1 = test1;
      var original2 = test2;
      var original3 = test3;

      Scope.Scoped(ref test1, ref test2, ref test3, () => {
        test1 = new object();
        test2 = new object();
        test3 = new object();
      });

      Assert.AreSame(original1, test1);
      Assert.AreSame(original2, test2);
      Assert.AreSame(original3, test3);
    }
  }
}
