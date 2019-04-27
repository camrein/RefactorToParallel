
using System;
using System.Threading;

namespace Benign {
  public class ForLoop {
    private readonly int[] globalShared = new int[10];

    public void IndependentIterations() {
      for(var i = 0; i < 100; ++i) {
        // do independent work
        var value = i * 3 + 5;
        var final = value * 2;
      }
    }

    public void IndependentWriteAccessesToSharedArray() {
      var array = new int[100];
      for(var i = 0; i < 100; ++i) {
        array[i] = i;
      }
    }

    public void IndependentReadAccessesToSharedArray() {
      var array = new int[100];
      for(var i = 0; i < 100; ++i) {
        var value = array[i];
      }
    }

    public void IndependentReadWriteAccessesToSharedArray() {
      var array = new int[100];
      for(var i = 0; i < 100; ++i) {
        array[i] = array[i] + 2;
      }
    }

    public void ReadAndWriteAccessThroughObfuscatedIndices() {
      var array = new int[100];
      for(var i = 0; i < 100; ++i) {
        var x = 2 * i;
        var y = x + 1;
        array[2 * i + 1] = array[y] + 2 + array[x + 1];
      }
    }

    public void ReadAndWriteAccessWithStrongExpressionNesting() {
      var array = new int[100];
      for(var i = 0; i < 100; ++i) {
        array[i + 1 + 1 + 1 + 1 + 1] = array[i + 1 + 1 + 1 + 1 + 1];
      }
    }

    public void OverlappingReadAccessesToSharedArray() {
      var array = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < array.Length; ++i) {
        // Overlapping read accesses
        var value = array[i - 1] + array[i] + array[i + 1];
      }
    }


    public void ReadAndWriteAccessToFieldLevelArray() {
      var array = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < this.globalShared.Length; ++i) {
        // Overlapping read accesses
        globalShared[i] = this.globalShared[i] + 1;
      }
    }

    public void ReadAndWriteAccessesThroughAliasing() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < array1.Length; ++i) {
        var array2 = array1;
        array2[i] = array2[i];
      }
    }

    public void ReadAndWriteAccessesWithStringInterpolation() {
      var array = new string[] { "1", "1", "1", "1", "1", "1", "1"};
      for(var i = 1; i < array.Length; ++i) {
        array[i] = $"original value: {array[i]}";
      }
    }

    public void IndependentReadWriteAccessesThroughLoopConstantBranch() {
      var array = new int[100];
      var x = 10;
      for(var i = 0; i < 20; ++i) {
        var a = 1;
        if(x > 5) {
          // This branch is constant for the entire loop, thus
          // the parallelization is safe
          a = 2;
        }

        var b = i * a;
        array[b] = array[b] + 2;
      }
    }

    public void ArrayWriteAccessesConstantForLoop() {
      var array = new int[100];
      var x = 10;
      for(var i = 0; i < 20; ++i) {
        // For each access to this loop, exactly one of the two
        // branches is used for all iterations.
        if(x > 5) {
          array[i * 2] = array[i * 2] + 2;
        } else {
          array[i * 3] = array[i * 3] + 2;
        }
      }
    }

    public void NoOverlappingArrayWriteAccessThroughAliasing() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      var array2 = array1;
      for(var i = 1; i < array1.Length; ++i) {
        array1[i] = array2[i];
      }
    }

    public void NoOverlappingArrayWriteAccessThroughInLoopAliasing() {
      var array1 = new[] { 1, 1, 1, 1, 1, 1, 1, 1 };
      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        var array2 = array1;
        array1[i] = array2[i];
      }
    }

    public void ImpossibleAliasingBecauseOfDifferentDimensions(int[] array1, int[,] array2) {
      for(var i = 1; i < array1.Length; ++i) {
        // Overlapping write accesses
        array1[i] = array2[i + 1, 0];
        array2[i + 1, 1] = 5;
      }
    }

    public void LoopUsingSafeApi() {
      var input = new int[] { 1, 2, 3, 4, 5 };
      var output = new byte[input.Length];
      for(var i = 0; i < input.Length; ++i) {
        output[i] = Convert.ToByte(input[i]);
      }
    }

    public void InterlockedIncrementOfSharedVariable() {
      var shared = 1;

      for(var i = 0; i < 100; ++i) {
        Interlocked.Increment(ref shared);
      }
    }

    /// <summary>
    /// Continue needs to be transformed into a return statement.
    /// </summary>
    public void ForLoopWithLoopReturn() {
      for(var i = 0; i < 100; ++i) {
        if(i > 50) {
          continue;
        }
      }
    }

    /// <summary>
    /// Variable is only written within the value with a single (implicitly) constant value.
    /// </summary>
    public void OnlyOverwriting() {
      var satisfied = false;
      for(int i = 0; i < 100; ++i) {
        if(i > 10) {
          satisfied = true;
        }

        if(i > 100) {
          satisfied = true;
        }
      }
      Console.WriteLine(satisfied);
    }

    /// <summary>
    /// Multiple conditions but none is written inside the loop. One of the conditions
    /// can be extracted to an enclosing if-statement.
    /// </summary>
    public void MultipleConditionsWithLoopRelevance() {
      var running = true;
      for(var i = 0; i < 100 && running; ++i) {
        // Do work...
      }
    }

    /// <summary>
    /// The statements are unrelated.
    /// </summary>
    public void UnrelatedAtomicStatements() {
      var array = new int[100];
      var shared = 0;
      for(var i = 0; i < 100; ++i) {
        array[i] = i;
        Interlocked.Increment(ref shared);
      }
    }

    /// <summary>
    /// Non-conflicting array accesses may not be as trivial as a direct access with
    /// the loop index.
    /// https://en.wikipedia.org/wiki/GCD_test
    /// </summary>
    public void NonTrivialLoopIndependence() {
      var array = new int[100];

      for(var i = 0; i < 100; ++i) {
        array[2 * i] = array[4 * i + 1];
      }
    }

    /// <summary>
    /// Non-conflicting array accesses when respecting the loop boundaries.
    /// Possible to detect with: Kennedy Approach, Banerjee Test, I-Test, Omega Test
    /// </summary>
    public void LoopIndependenceThroughBoundaries() {
      var array = new int[100];

      for(var i = 0; i < 10; ++i) {
        array[i] = array[i + 10];
      }
    }
  }
}
