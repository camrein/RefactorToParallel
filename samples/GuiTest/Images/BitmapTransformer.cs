using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuiTest.Images {
  public static class BitmapTransformer {
    public static byte[,] ToGrayscalePixelArray(BitmapSource source) {
      if(source.Format != PixelFormats.Gray8) {
        throw new ArgumentException($"{nameof(source)} has invalid pixel format");
      }

      var stride = _GetStride(source);
      return _ToPixelArray(source, (bytes, x, y) => bytes[x + y * stride]);
    }

    public static Color[,] ToColorPixelArray(BitmapSource source) {
      if(source.Format != PixelFormats.Rgb24) {
        throw new ArgumentException($"{nameof(source)} has invalid pixel format");
      }

      var stride = _GetStride(source);
      var bytesPerPixel = source.Format.BitsPerPixel / 8;
      return _ToPixelArray(source, (bytes, x, y) => Color.FromRgb(
          bytes[x * bytesPerPixel + y * stride],
          bytes[x * bytesPerPixel + y * stride + 1],
          bytes[x * bytesPerPixel + y * stride + 2]
        )
      );
    }

    private static T[,] _ToPixelArray<T>(BitmapSource source, Func<byte[], int, int, T> transform) {
      var stride = _GetStride(source);
      var bytes = new byte[stride * source.PixelHeight];
      source.CopyPixels(bytes, stride, 0);

      var pixels = new T[source.PixelWidth, source.PixelHeight];
      //for(var x = 0; x < source.PixelWidth; ++x) {
      //  for(var y = 0; y < source.PixelHeight; ++y) {
      //    pixels[x, y] = transform(bytes, x, y);
      //  }
      //}
      Parallel.For(0, source.PixelWidth, x => {
        for(var y = 0; y < source.PixelHeight; ++y) {
          pixels[x, y] = transform(bytes, x, y);
        }
      });

      return pixels;
    }

    public static BitmapSource ToBitmap(byte[,] pixels) {
      var format = PixelFormats.Gray8;
      var stride = _GetStride(pixels.GetLength(0), format);

      return _ToBitmap(pixels, format, (bytes, x, y, color) => {
        bytes[x + y * stride] = color;
      });
    }

    public static BitmapSource ToBitmap(Color[,] pixels) {
      return ToBitmap(pixels, color => color);
    }

    public static BitmapSource ToBitmap<T>(T[,] pixels, Func<T, Color> toRgb) {
      var format = PixelFormats.Rgb24;
      var bytesPerPixel = format.BitsPerPixel / 8;
      var stride = _GetStride(pixels.GetLength(0), format);

      return _ToBitmap(pixels, format, (bytes, x, y, value) => {
        var color = toRgb(value);
        bytes[x * bytesPerPixel + y * stride] = color.R;
        bytes[x * bytesPerPixel + y * stride + 1] = color.G;
        bytes[x * bytesPerPixel + y * stride + 2] = color.B;
      });
    }

    private static BitmapSource _ToBitmap<T>(T[,] pixels, PixelFormat format, Action<byte[], int, int, T> applyColor) {
      var width = pixels.GetLength(0);
      var height = pixels.GetLength(1);
      var stride = _GetStride(width, format);

      var bytes = new byte[stride * height];
      var bytesPerPixel = format.BitsPerPixel / 8;

      //for(var x = 0; x < width; ++x) {
      //  for(var y = 0; y < height; ++y) {
      //    applyColor(bytes, x, y, pixels[x, y]);
      //  }
      //}
      Parallel.For(0, width, x => {
        for(var y = 0; y < height; ++y) {
          applyColor(bytes, x, y, pixels[x, y]);
        }
      });

      var image = BitmapSource.Create(width, height, 96, 96, format, null, bytes, stride);
      image.Freeze();
      return image;
    }

    private static int _GetStride(BitmapSource source) {
      return _GetStride(source.PixelWidth, source.Format);
    }

    private static int _GetStride(int width, PixelFormat format) {
      return (width * format.BitsPerPixel + 7) / 8;
    }
  }
}
