using Crucible;
using CrucibleWemViewerPlugin.Model;
using Nito.Mvvm;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CrucibleWemViewerPlugin
{
  public class MainViewModel :INotifyPropertyChanged
  {
    private readonly CrucibleWemFile _model;

    internal MainViewModel(CrucibleWemFile model)
    {
      _model = model;

      NumberOfBytesInternal = NotifyTask.Create(LoadDataAsync);      
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public NotifyTask<int> NumberOfBytesInternal { get; private set; }

    public string InternalFileSize { get; private set; }

    private async Task<int> LoadDataAsync()
    {
      MainWindow.SetStatus($"Loading data...");

      var n = await _model.GetNumberOfRawBytesAsync();
      
      MainWindow.SetStatus($"Loaded data.");

      return n;
    }
  }
}
