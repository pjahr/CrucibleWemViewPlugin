using Crucible;
using CrucibleWemViewerPlugin.Model;
using Microsoft.Win32;
using NAudio.Wave;
using Nito.Mvvm;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CrucibleWemViewerPlugin.ViewModel
{
  public class MainViewModel : INotifyPropertyChanged
  {
    private readonly CrucibleWemFile _model;
    private bool _playing;

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private CancellationToken _ct;

    internal MainViewModel(CrucibleWemFile model)
    {
      _model = model;
      FileNameInternal = _model.FileName;
      PathInternal = _model.PathInternal;
      LastModified = $"{_model.LastModified}";

      // async properties
      NumberOfBytesInternal = NotifyTask.Create(LoadDataAsync);
      OggFilePath = NotifyTask.Create(SaveAudioToTemporaryFile);

      PlayAudioCommand = new AsyncCommand(async () => await PlayAudioAsync());
      StopPlayingAudioCommand = new AsyncCommand(async () => await StopAudioAsync());
      SaveAudioToFileCommand = new AsyncCommand(async () => await SaveAudioToFileAsync());

      Playing = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string FileNameInternal { get; }
    public string PathInternal { get; }
    public string LastModified { get; }
    public Visibility PlayVisible { get; private set; }
    public Visibility StopVisible { get; private set; } 
    public NotifyTask<int> NumberOfBytesInternal { get; private set; }
    public NotifyTask<string> OggFilePath { get; private set; }
    public IAsyncCommand PlayAudioCommand { get; private set; }
    public IAsyncCommand StopPlayingAudioCommand { get; private set; }
    public IAsyncCommand SaveAudioToFileCommand { get; }

    private bool Playing
    {
      get => _playing;
      set
      {
        _playing = value;
        PlayVisible = _playing ? Visibility.Hidden : Visibility.Visible;
        StopVisible = _playing ? Visibility.Visible: Visibility.Hidden;
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayVisible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopVisible)));
      }
    }
    
    private async Task PlayAudioAsync()
    {
      try
      {
        using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(OggFilePath.Result))
        using (var waveOut = new WaveOutEvent())
        {
          waveOut.Init(vorbisStream);

          Playing = true;
          _cts = new CancellationTokenSource();
          _ct = _cts.Token;
          await Task.Run(() =>
          {
            waveOut.Play();
            while (waveOut.PlaybackState!=PlaybackState.Stopped)
            {
              _ct.ThrowIfCancellationRequested(); // cancel this task if requested
              Thread.Sleep(20);
            }
            // cleanup after playing
            _cts.Dispose();
            Playing = false;

          }, _ct);
        }
      }
      catch (OperationCanceledException)
      {
        // cleanup after cancel
        _cts.Dispose();
        Playing = false;
      }      
    }

    private async Task StopAudioAsync()
    {
      // request canceling of play task
      await Task.Run(() => _cts.Cancel());
    }

    private async Task SaveAudioToFileAsync()
    {
      var saveFileDialog = new SaveFileDialog()
      {
        Filter = "WAV file (*.wav)|*.wav"
      };

      if (saveFileDialog.ShowDialog() == false)
      {
        return;
      }

      using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(OggFilePath.Result))
      using (var waveOut = new WaveOutEvent())
      {
        await Task.Run(() => WaveFileWriter.CreateWaveFile(saveFileDialog.FileName, vorbisStream));
      }
    }

    private async Task<string> SaveAudioToTemporaryFile()
    {
      return await _model.ConvertAsync();
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
