using GuiTest.Commands;
using GuiTest.Images;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuiTest.ViewModels {
  public class ConvolutionViewModel : ImageViewModel {
    private Kernel _selectedKernel = Kernel.List[0];
    private long _renderTimeMs;

    public ICommand NextFilter { get; }

    public IReadOnlyList<Kernel> Kernels { get; } = Kernel.List;

    public Kernel SelectedKernel {
      get => _selectedKernel;
      set {
        _selectedKernel = value;
        _UpdateAlteredImage();
        OnPropertyChanged();
      }
    }

    public long RenderTimeMs {
      get => _renderTimeMs;
      set {
        _renderTimeMs = value;
        OnPropertyChanged();
      }
    }

    public ConvolutionViewModel() {
      NextFilter = new RelayCommand(_NextFilter);
    }

    protected override async Task LoadImageAsync(string fileName) {
      Ready = false;
      OriginalImage = await BitmapLoader.LoadRgb(fileName);

      var stopwatch = Stopwatch.StartNew();
      AlteredImage = await _GetAlteredImage();
      RenderTimeMs = stopwatch.ElapsedMilliseconds;
      Ready = true;
    }

    private void _NextFilter(object args) {
      var currentPosition = Kernels.TakeWhile(kernel => !SelectedKernel.Equals(kernel)).Count();
      var newFilter = (currentPosition + 1) % Kernels.Count;
      SelectedKernel = Kernels[newFilter];
    }

    private async void _UpdateAlteredImage() {
      Ready = false;
      var stopwatch = Stopwatch.StartNew();
      AlteredImage = await _GetAlteredImage();
      RenderTimeMs = stopwatch.ElapsedMilliseconds;
      Ready = true;
    }

    private async Task<BitmapSource> _GetAlteredImage() {
      var image = OriginalImage;
      var kernel = SelectedKernel;
      if(image == null) {
        return null;
      }
      
      return await Task.Run(() => {
        return BitmapTransformer.ToBitmap(_ApplyFilter(BitmapTransformer.ToColorPixelArray(image), kernel));
      });
    }

    //// Source:
    //// https://en.wikipedia.org/wiki/Kernel_(image_processing)
    //// http://lodev.org/cgtutor/filtering.html
    private static Color[,] _ApplyFilter(Color[,] pixels, Kernel kernel) {
      var width = pixels.GetLength(0);
      var height = pixels.GetLength(1);
      var input = new byte[width, height, 3];

      Parallel.For(0, width, x => {
        for(var y = 0; y < height; ++y) {
          input[x, y, 0] = pixels[x, y].R;
          input[x, y, 1] = pixels[x, y].G;
          input[x, y, 2] = pixels[x, y].B;
        }
      });

      var output = _ApplyFilter(input, kernel);
      var result = new Color[width, height];

      Parallel.For(0, width, x => {
        for(var y = 0; y < height; ++y) {
          result[x, y] = Color.FromRgb(output[x, y, 0], output[x, y, 1], output[x, y, 2]);
        }
      });

      return result;
    }

    private static byte[,,] _ApplyFilter(byte[,,] pixels, Kernel kernel) {
      var filter = kernel.Matrix;
      var factor = kernel.Factor;
      var bias = kernel.Bias;

      int imageWidth = pixels.GetLength(0);
      int imageHeight = pixels.GetLength(1);
      int filterWidth = filter.GetLength(0);
      int filterHeight = filter.GetLength(1);

      var result = new byte[imageWidth, imageHeight, 3];

      for(var x = 0; x < imageWidth; ++x) {
        for(var y = 0; y < imageHeight; ++y) {
          _UpdateColors(result, x, y, pixels, imageWidth, imageHeight, filter, filterWidth, filterHeight, factor, bias);
        }
      }

      return result;
    }

    private static T _Identity<T>(T value) => value;

    private static void _UpdateColors(byte[,,] result, int x, int y, byte[,,] pixels, int imageWidth, int imageHeight, int[,] filter, int filterWidth, int filterHeight, double factor, double bias) {
      int red = 0;
      int green = 0;
      int blue = 0;

      for(var fx = 0; fx < filterWidth; ++fx) {
        for(var fy = 0; fy < filterHeight; ++fy) {
          int ix = (x - filterWidth / 2 + fx + imageWidth) % imageWidth;
          int iy = (y - filterHeight / 2 + fy + imageHeight) % imageHeight;
          red += pixels[ix, iy, 0] * filter[fx, fy];
          green += pixels[ix, iy, 1] * filter[fx, fy];
          blue += pixels[ix, iy, 2] * filter[fx, fy];
        }
      }

      red = (int)(red * factor + bias);
      green = (int)(green * factor + bias);
      blue = (int)(blue * factor + bias);


      result[x, y, 0] = _ToByte(red);
      result[x, y, 1] = _ToByte(green);
      result[x, y, 2] = _ToByte(blue);
    }

    private static byte _ToByte(int value) {
      return (byte)(value < 0 ? 0 : (value > 255 ? 255 : value));
    }
  }
}
