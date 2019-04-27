using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorToParallel.Core.Extensions;
using System.Linq;

namespace RefactorToParallel.Core.Test.Extensions {
  [TestClass]
  public class CollectionExtensionsTest {
    [TestMethod]
    public void GetNonExistentKeyFromDictionary() {
      var dictionary = new Dictionary<string, int>();

      Assert.AreEqual(0, dictionary.GetOrCreate("1"));
      Assert.AreEqual(1, dictionary.Count);
      Assert.IsTrue(dictionary.ContainsKey("1"));
      Assert.AreEqual(0, dictionary["1"]);
    }

    [TestMethod]
    public void GetExistentKeyFromDictionary() {
      var dictionary = new Dictionary<string, int> { { "5", 3 } };

      Assert.AreEqual(3, dictionary.GetOrCreate("5"));
      Assert.AreEqual(1, dictionary.Count);
      Assert.IsTrue(dictionary.ContainsKey("5"));
      Assert.AreEqual(3, dictionary["5"]);
    }

    [TestMethod]
    public void GetNonExistentKeyFromDictionaryWithGivenBuilder() {
      var dictionary = new Dictionary<string, object>();

      Assert.AreEqual("test", dictionary.GetOrCreate("123", () => "test"));
      Assert.AreEqual(1, dictionary.Count);
      Assert.IsTrue(dictionary.ContainsKey("123"));
      Assert.AreEqual("test", dictionary["123"]);
    }

    [TestMethod]
    public void GetExistentKeyFromDictionaryWithGivenBuilder() {
      var dictionary = new Dictionary<string, object> { { "4", "original" } };

      Assert.AreEqual("original", dictionary.GetOrCreate("4", () => "test"));
      Assert.AreEqual(1, dictionary.Count);
      Assert.IsTrue(dictionary.ContainsKey("4"));
      Assert.AreEqual("original", dictionary["4"]);
    }

    [TestMethod]
    public void EmptyEnumerableEnqueued() {
      var queue = new Queue<string>();
      queue.Enqueue("test");
      queue.EnqueueAll(Enumerable.Empty<string>());
      Assert.AreEqual(1, queue.Count);
    }

    [TestMethod]
    public void SingleItemEnqueued() {
      var queue = new Queue<string>();
      queue.Enqueue("test1");
      queue.EnqueueAll(new[] { "test2" });
      Assert.AreEqual(2, queue.Count);
      Assert.AreEqual("test1", queue.Dequeue());
      Assert.AreEqual("test2", queue.Dequeue());
    }

    [TestMethod]
    public void MultipleItemsEnqueued() {
      var queue = new Queue<string>();
      queue.Enqueue("test1");
      queue.EnqueueAll(new[] { "test2", "test3", "test4", "test5", "test6" });
      Assert.AreEqual(6, queue.Count);
      Assert.AreEqual("test1", queue.Dequeue());
      Assert.AreEqual("test2", queue.Dequeue());
      Assert.AreEqual("test3", queue.Dequeue());
      Assert.AreEqual("test4", queue.Dequeue());
      Assert.AreEqual("test5", queue.Dequeue());
      Assert.AreEqual("test6", queue.Dequeue());
    }
  }
}
