namespace RefactorToParallel.Core.Util {
  /// <summary>
  /// Used to calculate hash codes of given objects.
  /// </summary>
  public class Hash {
    private const int HashPrime = 59;
    private const int NullHash = 0;

    private readonly int _value;

    private Hash() : this(1) { }

    private Hash(int value) {
      _value = value;
    }

    /// <summary>
    /// Creates a new hash object with a given object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">Object to start the hash chain with.</param>
    /// <returns>The generated hash object.</returns>
    public static Hash With<T>(T obj) {
      return new Hash().And(obj);
    }

    /// <summary>
    /// Updates the current hash value with the given object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">Object to append to the hash chain.</param>
    /// <returns>The updated hash object.</returns>
    public Hash And<T>(T obj) {
      return new Hash(HashPrime * _value + (obj?.GetHashCode() ?? NullHash));
    }

    /// <summary>
    /// Gets the current hash.
    /// </summary>
    /// <returns>The current hash.</returns>
    public int Get() {
      return _value;
    }
  }
}
