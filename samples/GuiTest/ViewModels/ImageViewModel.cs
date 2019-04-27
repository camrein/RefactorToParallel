using GuiTest.Commands;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GuiTest.ViewModels {
  public abstract class ImageViewModel : BaseViewModel {
    private BitmapSource _originalImage;
    private BitmapSource _alteredImage;
    private bool _ready = true;

    public ICommand SelectImage { get; }

    public BitmapSource OriginalImage {
      get => _originalImage;
      set {
        _originalImage = value;
        OnPropertyChanged();
      }
    }

    public BitmapSource AlteredImage {
      get => _alteredImage;
      set {
        _alteredImage = value;
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

    public ImageViewModel() {
      SelectImage = new AsyncRelayCommand(_SelectImage);
    }

    private async Task _SelectImage(object args) {
      var dialog = new OpenFileDialog();
      if(dialog.ShowDialog() == true) {
        await LoadImageAsync(dialog.FileName);
      }
    }

    protected abstract Task LoadImageAsync(string fileName);
  }
}
