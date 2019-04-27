using System;
using System.Collections.Generic;

namespace RefactorToParallel.Core.Extensions {
  /// <summary>
  /// Extension methods to work with collections.
  /// </summary>
  public static class CollectionExtensions {
    /// <summary>
    /// Try to retrieve the current value from the dictionary. If the key is not present,
    /// a new instance of the value is created.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key">The key to get the value of.</param>
    /// <returns>The present value or a newly created instance.</returns>
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
      => dictionary.GetOrCreate(key, () => new TValue());

    /// <summary>
    /// Try to retrieve the current value from the dictionary. If the key is not present,
    /// a new instance of the value is created.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key">The key to get the value of.</param>
    /// <param name="builder">The builder that creates an instance of the desired value type.</param>
    /// <returns>The present value or a newly created instance.</returns>
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> builder) {
      if(dictionary.TryGetValue(key, out var value)) {
        return value;
      }

      value = builder();
      dictionary.Add(key, value);
      return value;
    }

    /// <summary>
    /// Enqueues all items to the given queue.
    /// </summary>
    /// <typeparam name="TValue">The type of the items to enqueue.</typeparam>
    /// <param name="queue">The target queue.</param>
    /// <param name="items">The items to enqueue.</param>
    public static void EnqueueAll<TValue>(this Queue<TValue> queue, IEnumerable<TValue> items) {
      foreach(var item in items) {
        queue.Enqueue(item);
      }
    }
  }
}
