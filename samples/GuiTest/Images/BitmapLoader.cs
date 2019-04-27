using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuiTest.Images {
  public static class BitmapLoader {
    public static Task<BitmapSource> LoadRgb(string fileName) {
      return _LoadFromFile(fileName, PixelFormats.Rgb24);
    }

    public static Task<BitmapSource> LoadGrayscale(string fileName) {
      return _LoadFromFile(fileName, PixelFormats.Gray8);
    }

    private static Task<BitmapSource> _LoadFromFile(string fileName, PixelFormat format) {
      return Task.Run(() => {
        var source = new BitmapImage();
        source.BeginInit();
        source.UriSource = new Uri(fileName);
        source.EndInit();
        source.Freeze();
        return _ToFormat(source, format);
      });
    }

    private static BitmapSource _ToFormat(BitmapSource source, PixelFormat format) {
      var bitmap = new FormatConvertedBitmap();
      bitmap.BeginInit();
      bitmap.DestinationFormat = format;
      bitmap.Source = source;
      bitmap.EndInit();
      bitmap.Freeze();
      return bitmap;
    }
  }
}
