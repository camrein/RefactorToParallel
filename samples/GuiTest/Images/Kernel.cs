
using System.Collections.Generic;

namespace GuiTest.Images {
  public class Kernel {
    public int[,] Matrix { get; }
    public double Bias { get; }
    public double Factor { get; }
    public string Label { get; }

    public static IReadOnlyList<Kernel> List { get; } = new []{
      new Kernel("Identity", new int[,] {
        {0, 0, 0},
        {0, 1, 0},
        {0, 0, 0}
      }),

      new Kernel("Edge Detection", new int[,] {
        {-1, -1, -1},
        {-1, 8, -1},
        {-1, -1, -1}
      }),

      new Kernel("Sharpen 3x3", new int[,] {
        {-1, -1, -1},
        {-1, 9, -1},
        {-1, -1, -1}
      }),

      new Kernel("Sharpen 5x5", new int[,] {
        {-1, -1, -1, -1, -1},
        {-1,  2,  2,  2, -1},
        {-1,  2,  8,  2, -1},
        {-1,  2,  2,  2, -1},
        {-1, -1, -1, -1, -1}
      }, 1.0 / 8.0),

      new Kernel("Emboss", new int[,] {
        {-2, -1, 0},
        {-1, 1, 1},
        {0, 1, 2}
      }),

      new Kernel("Motion blur 3x3", new int[,] {
        {1, 0, 0},
        {0, 1, 0},
        {0, 0, 1},
      }, 1.0 / 3.0),

      new Kernel("Motion blur 9x9", new int[,] {
        {1, 0, 0, 0, 0, 0, 0, 0, 0},
        {0, 1, 0, 0, 0, 0, 0, 0, 0},
        {0, 0, 1, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 1, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 1, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 1, 0, 0},
        {0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 0, 0, 0, 0, 0, 1}
      }, 1.0 / 9.0),

      new Kernel("Gaussian Blur", new int[,] {
        {1, 2, 1},
        {2, 4, 2},
        {1, 2, 1}
      }, 1.0 / 16.0),

      new Kernel("High Pass", new int[,] {
        {-1, -2, -1},
        {-2, 12, -2},
        {-1, -2, -1}
      }, 1.0 / 16.0, 128.0)
    };

    public Kernel(string label, int[,] matrix) : this(label, matrix, 1.0) { }

    public Kernel(string label, int[,] matrix, double factor) : this(label, matrix, factor, 0.0) { }

    public Kernel(string label, int[,] matrix, double factor, double bias) {
      Label = label;
      Matrix = matrix;
      Bias = bias;
      Factor = factor;
    }

    public override string ToString() {
      return Label;
    }
  }
}
