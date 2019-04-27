using GuiTest.Images;
using System;
using System.Threading.Tasks;

namespace GuiTest.ViewModels {
  public class DitherViewModel : ImageViewModel {
    protected override async Task LoadImageAsync(string fileName) {
      Ready = false;
      var original = OriginalImage = await BitmapLoader.LoadGrayscale(fileName);
      AlteredImage = await Task.Run(() => {
        return BitmapTransformer.ToBitmap(_Dither(BitmapTransformer.ToGrayscalePixelArray(original)));
      });
      Ready = true;
    }

    // Source: https://en.wikipedia.org/wiki/Polytope_model
    // Not parallelizable?
    private static byte[,] _Dither(byte[,] imageData) {
      var width = imageData.GetLength(0);
      var height = imageData.GetLength(1);
      var result = new byte[width, height];

      int Error(int x, int y) {
        return result[x, y] - imageData[x, y];
      }

      for(var y = 0; y < height; ++y) {
        for(var x = 0; x < width; ++x) {
          int v = imageData[x, y];

          if(x > 0) {
            v -= Error(x - 1, y) / 2;
          }

          if(y > 0) {
            v -= Error(x, y - 1) / 4;
          }

          if(y > 0 && x < width - 1) {
            v -= Error(x + 1, y - 1) / 4;
          }

          result[x, y] = Convert.ToByte((v < 128) ? 0 : 255);
          imageData[x, y] = Convert.ToByte((v < 0) ? 0 : (v < 255) ? v : 255);
        }
      }

      return result;
    }
  }
}
