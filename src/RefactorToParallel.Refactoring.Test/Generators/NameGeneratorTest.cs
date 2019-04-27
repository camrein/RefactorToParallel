using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Refactoring.Generators;

namespace RefactorToParallel.Refactoring.Test.Generators {
  [TestClass]
  public class NameGeneratorTest {
    [TestMethod]
    public void CreateNameWithTwoComponents() {
      Assert.AreEqual("System.Threading", NameGenerator.CreateName("System", "Threading").ToString());
    }

    [TestMethod]
    public void CreateNameWithThreeComponents() {
      Assert.AreEqual("Microsoft.VisualStudio.TestTools", NameGenerator.CreateName("Microsoft", "VisualStudio", "TestTools").ToString());
    }

    [TestMethod]
    public void CreateNameWithFourComponents() {
      Assert.AreEqual("Microsoft.CodeAnalysis.CSharp.Syntax", NameGenerator.CreateName("Microsoft", "CodeAnalysis", "CSharp", "Syntax").ToString());
    }
  }
}
