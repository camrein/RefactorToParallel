using GuiTest.Commands;
using GuiTest.Images;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace GuiTest.ViewModels {
  public class MandelbrotViewModel : BaseViewModel {
    private const int ImageWidth = 2560;
    private const int ImageHeight = 1440;
    private const int AnimationDelayMs = 1000;

    private static readonly Color[] ColorMap = {
      Color.FromRgb(66, 30, 15),
      Color.FromRgb(25, 7, 26),
      Color.FromRgb(9, 1, 47),
      Color.FromRgb(4, 4, 73),
      Color.FromRgb(0, 7, 100),
      Color.FromRgb(12, 44, 138),
      Color.FromRgb(24, 82, 177),
      Color.FromRgb(57, 125, 209),
      Color.FromRgb(134, 181, 229),
      Color.FromRgb(211, 236, 248),
      Color.FromRgb(241, 233, 191),
      Color.FromRgb(248, 201, 95),
      Color.FromRgb(255, 170, 0),
      Color.FromRgb(204, 128, 0),
      Color.FromRgb(153, 87, 0),
      Color.FromRgb(106, 52, 3)
    };

    private ImageSource _mandelbrot;
    private int _maxIterations = 32;
    private string _status;
    private bool _ready = true;
    private long _renderTimeMs;

    public ICommand UpdateMandelbrot { get; }

    public ICommand Animate { get; }

    public ImageSource Mandelbrot {
      get => _mandelbrot;
      set {
        _mandelbrot = value;
        OnPropertyChanged();
      }
    }

    public int MaxIterations {
      get => _maxIterations;
      set {
        _maxIterations = value;
        OnPropertyChanged();
      }
    }

    public string Status {
      get => _status;
      set {
        _status = value;
        OnPropertyChanged();
      }
    }

    public bool Ready {
      get => _ready;
      set {
        _ready = value;
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

    public MandelbrotViewModel() {
      UpdateMandelbrot = new AsyncRelayCommand(_UpdateMandelbrot);
      Animate = new AsyncRelayCommand(_Animate);
    }

    private async Task _UpdateMandelbrot(object arg) {
      Ready = false;
      Status = "generating";
      var stopwatch = Stopwatch.StartNew();
      Mandelbrot = await _GenerateMandelbrot(ImageWidth, ImageHeight, MaxIterations);
      RenderTimeMs = stopwatch.ElapsedMilliseconds;
      Status = "done";
      Ready = true;
    }

    private async Task _Animate(object arg) {
      Ready = false;
      var stopwatch = Stopwatch.StartNew();
      var firstFrame = 2;

      for(var i = firstFrame; i <= MaxIterations; ++i) {
        stopwatch.Restart();
        var mandelbrot = await _GenerateMandelbrot(ImageWidth, ImageHeight, i);

        var delay = AnimationDelayMs - Convert.ToInt32(stopwatch.ElapsedMilliseconds);
        Debug.WriteLine($"Elapsed #{i}: {stopwatch.ElapsedMilliseconds}");
        if(delay > 0 && firstFrame != i) {
          await Task.Delay(delay);
        }

        Status = "#" + i;
        Mandelbrot = mandelbrot;
      }
      Ready = true;
    }

    // Inner and outer loop can be parallelized
    private Task<ImageSource> _GenerateMandelbrot(int imageWidth, int imageHeight, int maxIterations) {
      return Task.Run(() => {
        var buffer = new int[imageWidth, imageHeight];

        for(var column = 0; column < imageWidth; ++column) {
          var x0 = _ScaleToMandelbrotX(column, imageWidth);
          for(var row = 0; row < imageHeight; ++row) {
            var y0 = _ScaleToMandelbrotY(row, imageHeight);
            buffer[column, row] = _ComputeIterations(x0, y0, maxIterations);
          }
        }

        return _GetImageSource(buffer, maxIterations);
      });
    }

    private static int _ComputeIterations(double x0, double y0, int maxIterations) {
      var x = 0d;
      var y = 0d;
      var iteration = 0;

      while(x * x + y * y < 2 * 2 && iteration < maxIterations) {
        var xtemp = x * x - y * y + x0;
        y = 2 * x * y + y0;
        x = xtemp;
        ++iteration;
      }

      return iteration;
    }

    private static double _ScaleToMandelbrotX(int x, int imageWidth) {
      // Compute the real part: (-2.5, 1)
      return (double)x / imageWidth * 3.5 - 2.5;
    }

    private static double _ScaleToMandelbrotY(int y, int imageHeight) {
      // Compute the real part: (-1, 1)
      return (double)y / imageHeight * 2 - 1.0;
    }

    private static ImageSource _GetImageSource(int[,] buffer, int maxIterations) {
      return BitmapTransformer.ToBitmap(buffer, iteration => _GetRgbColor(iteration, maxIterations));
    }

    private static Color _GetRgbColor(int iteration, int maxIterations) {
      return (iteration < maxIterations && iteration > 0) ? ColorMap[iteration % ColorMap.Length] : Color.FromRgb(0, 0, 0);
    }
  }
}
