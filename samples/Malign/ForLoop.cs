using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Malign {
  public class ForLoop {
    /// <summary>
    /// Empty loops will probably removed during optimization and therefore
    /// should not be parallelized.
    /// </summary>
    //public void EmptyLoop() {
    //  for(var i = 0; i < 100; ++i) {
    //    // empty
    //  }
    //}

    /// <summary>
    /// The loop variable is written within the loop's body.
    /// </summary>
    public void SkippingLoop() {
      for(var i = 0; i < 100; ++i) {
        // Do work...

        // Skip next index
        ++i;
      }
    }

    /// <summary>
    /// The loop's boundary variable is written within the loop's body.
    /// </summary>
    public void ModifyingBoundary() {
      var boundary = 100;
      for(var i = 0; i < boundary; ++i) {
        // Do work...

        // Reduce boundary
        boundary = 50;
      }
    }

    /// <summary>
    /// Multiple variables are part of the loop's signature.
    /// </summary>
    public void MultiVariable() {
      for(int i = 0, j = 0; i < 100 && j < 100; j++, i++) {
        // Do work ...
      }
    }

    /// <summary>
    /// One of the conditions are written within the loop.
    /// If that's not the case, one of the conditions could be extracted to an
    /// enclosing if-statement.
    /// </summary>
    public void MultipleConditionsWithLoopRelevance() {
      var running = true;
      for(var i = 0; i < 100 && running; ++i) {
        if(i > 10) {
          running = false;
        }
      }
    }

    /// <summary>
    /// Overlapping accesses to a shared array.
    /// </summary>
    public void OverlappingArrayWriteAccess() {
      var array = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < array.Length; ++i) {
        // Overlapping write accesses
        array[i] = array[i - 1] + array[i] + array[i + 1];
      }
    }
    
    /// <summary>
    /// Overlapping accesses through aliasing of a shared array.
    /// </summary>
    public void OverlappingArrayWriteAccessThroughAliasing() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      var array2 = array1;

      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        array1[i] = array2[i - 1];
      }
    }

    /// <summary>
    /// Overlapping accesses through aliasing of a shared array inside the loop.
    /// </summary>
    public void OverlappingArrayWriteAccessThroughInLoopAliasing() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        var array2 = array1;
        array1[i] = array2[i - 1];
      }
    }

    /// <summary>
    /// The aliasing of two arrays is hidden behind non-array typed variable
    /// that is later cast back to an array.
    /// </summary>
    public void AliasingHiddenBehindObjectCast() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      object array2 = array1;
      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        var array3 = (int[])array2;
        array1[i] = array3[i - 1];
      }
    }

    /// <summary>
    /// Overlapping accesses through aliasing of jagged arrays.
    /// </summary>
    public void OverlappingArrayWriteAccessThroughAliasingOfJaggedArrays() {
      var inner = new int[10];
      var jagged = new int[][] { inner, inner, inner };

      for(var i = 1; i < jagged.Length; ++i) {
        var nested = jagged[i];
        nested[i] = 5;
      }
    }

    /// <summary>
    /// Overlapping accesses through aliasing of a shared array inside the loop.
    /// </summary>
    public void OverlappingAccessesThroughCompleteInLoopAliasing() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        var array2 = array1;
        array2[i] = array2[i - 1];
      }
    }

    /// <summary>
    /// Depending on the passed parameters, array2 may alias array1.
    /// </summary>
    /// <param name="array1"></param>
    /// <param name="array2"></param>
    public void PossiblyOverlappingAccessesThroughAliasing(int[] array1, int[] array2) {
      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        array1[i] = array2[i - 1];
      }
    }

    /// <summary>
    /// Write access to a shared stream.
    /// </summary>
    public void StreamWriting() {
      var buffer = new byte[256];
      var stream = new MemoryStream(buffer);
      for(var i = 0; i < 256; ++i) {
        stream.WriteByte((byte)i);
      }
    }

    /// <summary>
    /// Read access to a shared stream.
    /// </summary>
    public void StreamReading() {
      var buffer = new byte[256];
      var stream = new MemoryStream(buffer);
      for(var i = 0; i < 256; ++i) {
        stream.ReadByte();
      }
    }

    /// <summary>
    /// Parallelizing locks may lead to deadlocks.
    /// </summary>
    public void LockAcquisition() {
      var shared = new object();
      for(var i = 0; i < 100; ++i) {
        lock(shared) { }
      }
    }

    /// <summary>
    /// No reason to parallelize loop's which start new tasks or threads.
    /// </summary>
    public void TaskStartedInsideLoop() {
      for(var i = 0; i < 100; ++i) {
        Task.Run(() => { });
      }
    }

    /// <summary>
    /// From a syntactic view, list write-accesses can look
    /// just like the ones of arrays. But lists do not support
    /// concurrent accesses.
    /// </summary>
    public void NonOverlappingWriteAccessToList() {
      var list = new List<int>(Enumerable.Repeat(0, 100));
      for(int i = 0; i < list.Count; ++i) {
        list[i] = i;
      }
    }

    /// <summary>
    /// When parallelizing this statement, the resulting
    /// value is no longer deterministic.
    /// </summary>
    public void WriteAccessToSharedVariable() {
      var shared = 10;
      for(var i = 0; i < 100; ++i) {
        shared = i;
      }
    }

    /// <summary>
    /// Race condition due to read and write access
    /// to a shared variable.
    /// </summary>
    public void ReadWriteAccessToSharedVariable() {
      var shared = 10;
      for(var i = 0; i < 100; ++i) {
        shared += i;
      }
    }

    /// <summary>
    /// Race condition due to read and write access
    /// to a shared variable.
    /// </summary>
    public void ReadWriteAccessToSharedVariableInSeparateOperations() {
      var shared = 10;
      for(var i = 0; i < 100; ++i) {
        var current = shared;
        // do work...
        shared = current + 1;
      }
    }

    /// <summary>
    /// The exchange is dependant on the previous iteration.
    /// </summary>
    public void InterlockedExchangeOfSharedVariable() {
      var shared = -1;

      for(var i = 0; i < 100; ++i) {
        Interlocked.CompareExchange(ref shared, i, i - 1);
      }
    }

    /// <summary>
    /// Break cannot be reasonably represented in a parallel loop.
    /// </summary>
    public void ForLoopWithLoopBreak() {
      for(var i = 0; i < 100; ++i) {
        if(i > 50) {
          break;
        }
      }
    }

    /// <summary>
    /// Return cannot be reasonably represented in a parallel loop.
    /// </summary>
    public void ForLoopWithLoopReturn() {
      for(var i = 0; i < 100; ++i) {
        if(i > 50) {
          return;
        }
      }
    }

    /// <summary>
    /// Variable is written with multiple different constant values.
    /// </summary>
    public void OnlyOverwriting() {
      var satisfied = false;
      for(int i = 0; i < 100; ++i) {
        if(i > 10) {
          satisfied = true;
        }

        if(i > 100) {
          satisfied = false;
        }
      }
      Console.WriteLine(satisfied);
    }

    /// <summary>
    /// The statements are related. On their own, they would be fine for
    /// parallelization, but their dependence is not parallelizable.
    /// </summary>
    public void UnrelatedAtomicStatements() {
      var array = new int[100];
      var shared = 0;
      for(var i = 0; i < 100; ++i) {
        var value = Interlocked.Increment(ref shared);
        array[i] = value;
      }
    }

    /// <summary>
    /// Accesses to thread-locals may not be parallelized. Read-Only accesses
    /// could possibly moved outside the parallel loop.
    /// </summary>
    public void UseOfThreadLocals() {
      var shared = new ThreadLocal<int>();
      for(var i = 0; i < 100; ++i) {
        // If parallelized, the tasks will not access the same values
        // as they're executed on different threads.
        var value = shared.Value;
      }
    }

    public void WriteAccessToArrayPassedToSafeApi() {
      var data = new byte[] { 0, 1, 2, 3 };
      var transformed = new string[data.Length];
      for(var i = 0; i < transformed.Length; ++i) {
        data[i] = (byte)i;
        transformed[i] = Convert.ToBase64String(data, 0, 4);
      }
    }

    public void WriteAccessToArrayPassedAsEnumerableToSafeApi() {
      var array = new string[] { "a", "b", "c", "d" };
      var enumerable = array.AsEnumerable();
      for(var i = 0; i < array.Length; ++i) {
        array[i] = string.Join(", ", enumerable);
      }
    }
  }
}
