using CrucibleWemViewerPlugin.Model;
using Nito.Mvvm;
using System.Threading.Tasks;

namespace CrucibleWemViewerPlugin
{
  public class MainViewModel
  {
    private readonly CrucibleWemFile _model;

    internal MainViewModel(CrucibleWemFile model)
    {
      _model = model;

      NumberOfBytesInternal = NotifyTask.Create(LoadDataAsync);
      
    }

    public NotifyTask<int> NumberOfBytesInternal { get; private set; }

    public string InternalFileSize { get; private set; }

    private async Task<int> LoadDataAsync()
    {
      return await _model.GetNumberOfRawBytesAsync();
    }
  }
}
