using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Refactoring.Generators;

namespace RefactorToParallel.Refactoring.Test.Generators {
  [TestClass]
  public class MemberAccessGeneratorTest {
    [TestMethod]
    public void CreateMemberAccessWithOneMember() {
      Assert.AreEqual("Console.WriteLine", MemberAccessGenerator.CreateMemberAccess("Console", "WriteLine").ToString());
    }

    [TestMethod]
    public void CreateMemberAccessWithTwoMembers() {
      Assert.AreEqual("Task.Factory.StartNew", MemberAccessGenerator.CreateMemberAccess("Task", "Factory", "StartNew").ToString());
    }

    [TestMethod]
    public void CreateMemberAccessWithThreeMembers() {
      Assert.AreEqual("Task.Factory.Scheduler.Id", MemberAccessGenerator.CreateMemberAccess("Task", "Factory", "Scheduler", "Id").ToString());
    }
  }
}
