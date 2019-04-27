using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorIntegrationTest {
  class SingleLineForLoop {
    public static void WithoutBlock() {
      // Test

      for(var i = 0; i < 100; ++i)
        Console.WriteLine(i);

      // TEst 2
    }

    public static void WithBlock() {
      // Test

      for(var i = 0; i < 100; ++i) {
        Console.WriteLine(i);
      }

      // TEst 2
    }
  }
}
