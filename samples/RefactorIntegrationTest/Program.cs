using System;

namespace RefactorIntegrationTest {
  class Program {
    static void Main(string[] args) {
      var array = new []{ "a", "x", "b", "y", "z", "d", "f", "a", "m" };
      //Sort.QuickSort(array);
      //Sort.MergeSort(array);
      Sort.InsertionSort(array);
      Console.WriteLine(string.Join(",", array));
    }
  }
}
