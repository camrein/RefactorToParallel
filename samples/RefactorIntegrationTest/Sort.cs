using System;

namespace RefactorIntegrationTest {
  public static class Sort {
    public static void QuickSort<T>(T[] array) where T : IComparable {
      _QuickSort(array, 0, array.Length - 1);
    }

    private static void _QuickSort<T>(T[] array, int left, int right) where T : IComparable {
      var pivot = array[left];
      var i = left;
      var j = right;

      do {
        while(array[i].CompareTo(pivot) < 0) { ++i; }
        while(pivot.CompareTo(array[j]) < 0) { --j; }

        if(i <= j) {
          T temp = array[i];
          array[i] = array[j];
          array[j] = temp;

          ++i;
          --j;
        }
      } while(i < j);

      if(j > left) {
        _QuickSort(array, left, j);
      }

      if(i < right) {
        _QuickSort(array, i, right);
      }
    }

    public static void MergeSort<T>(T[] array) where T : IComparable {
      var a = array;
      var b = new T[a.Length];
      Array.Copy(a, b, a.Length);

      _MergeSort(b, 0, array.Length - 1, a);
    }

    private static void _MergeSort<T>(T[] b, int begin, int end, T[] a) where T : IComparable {
      if(end - begin < 2) {
        return;
      }

      var mid = (end + begin) / 2;
      _MergeSort(a, begin, mid, b);
      _MergeSort(a, mid, end, b);
      _Merge(b, begin, mid, end, a);
    }

    private static void _Merge<T>(T[] a, int begin, int mid, int end, T[] b) where T : IComparable {
      var i = begin;
      var j = mid;

      for(var k = begin; k < end; ++k) {
        if(i < mid && (j >= end || a[i].CompareTo(a[j]) <= 0)) {
          b[k] = a[i];
          ++i;
        } else {
          b[k] = a[j];
          ++j;
        }
      }
    }

    public static void InsertionSort<T>(T[] array) where T : IComparable {
      for(int i = 1; i < array.Length; ++i) {
        var x = array[i];
        var j = i - 1;

        while(j >= 0 && array[j].CompareTo(x) > 0) {
          array[j + 1] = array[j];
          --j;
        }

        array[j + 1] = x;
      }
    }
  }
}
