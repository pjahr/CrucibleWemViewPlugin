using Crucible;
using CrucibleWemViewerPlugin.Model;
using NAudio.Wave;
using Nito.Mvvm;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CrucibleWemViewerPlugin.ViewModel
{
  public class MainViewModel : INotifyPropertyChanged
  {
    private readonly CrucibleWemFile _model;

    public string FileNameInternal { get; }
    public string PathInternal { get; }
    public string LastModified { get; }

    private bool _playing;
    private string _oggFilePath;

    internal MainViewModel(CrucibleWemFile model)
    {
      _model = model;
      FileNameInternal = _model.FileName;
      PathInternal = _model.Path;
      LastModified = $"{_model.LastModified}";

      NumberOfBytesInternal = NotifyTask.Create(LoadDataAsync);

      ConvertAudioCommand = new AsyncCommand(async () =>
      {
        OggFilePath = await _model.ConvertAsync();
      });

      PlayAudioCommand = new AsyncCommand(async () =>
      {
        using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(OggFilePath))
        using (var waveOut = new WaveOutEvent())
        {
          waveOut.Init(vorbisStream);
          waveOut.PlaybackStopped += Stop;
          Playing = true;
          await Task.Run(() =>
          {
            waveOut.Play();
            while (Playing)
            {
              Thread.Sleep(20);
            }
          });
        }
      });

    }

    private void Stop(object sender, StoppedEventArgs e)
    {
      Playing = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public NotifyTask<int> NumberOfBytesInternal { get; private set; }

    public IAsyncCommand ConvertAudioCommand { get; private set; }
    public IAsyncCommand PlayAudioCommand { get; private set; }

    public string OggFilePath
    {
      get => _oggFilePath; private set
      {
        _oggFilePath = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OggFilePath)));
      }
    }

    public bool Playing
    {
      get => _playing; private set
      {
        _playing = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playing)));
      }
    }

    private async Task<int> LoadDataAsync()
    {
      MainWindow.SetStatus($"Loading data...");

      var n = await _model.GetNumberOfRawBytesAsync();

      MainWindow.SetStatus($"Loaded data.");

      return n;
    }
  }
}
