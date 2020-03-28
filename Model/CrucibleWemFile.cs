using Crucible;
using Crucible.Filesystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WEMSharp;

namespace CrucibleWemViewerPlugin.Model
{
  internal class CrucibleWemFile
  {
    private readonly IFilesystemEntry _fileSystemEntry;
    private byte[] _rawData;

    public CrucibleWemFile(IFilesystemEntry filesystemEntry)
    {
      _fileSystemEntry = filesystemEntry;
    }

    public async Task<int> GetNumberOfRawBytesAsync()
    {
      if (_rawData != null)
      {
        _rawData = await Task.Run(() => GetRawBytes());
      }
      return _rawData.Length;
    }

    private byte[] GetRawBytes()
    {
      return _fileSystemEntry.GetData();
    }

    private void Convert()
    {
      var tmpDirectory = Path.GetTempPath();

      var fileName = $"Crucible_tmpWEM_{Guid.NewGuid()}";
      var wemFileName = $"{fileName}.wem";
      var oggFileName = $"{fileName}.ogg";

      var wemFilePath = Path.Combine(tmpDirectory, wemFileName);
      var oggFilePath = Path.Combine(tmpDirectory, oggFileName);

      File.WriteAllBytes(wemFilePath, Fileconverter.GetConvertedData(_fileSystemEntry));

      var wemFile = new WEMFile(wemFilePath, WEMForcePacketFormat.NoForcePacketFormat);
      MainWindow.SetStatus($"Wrote {wemFilePath}");

      wemFile.GenerateOGG(oggFilePath, "packed_codebooks_aoTuV_603", false, false);




      MainWindow.SetStatus($"Wrote {oggFilePath}");




    }    

  }

  public static class External
  {
    public static async Task<int> RunProcessAsync(string fileName, string args)
    {
      using (var process = new Process
      {
        StartInfo =
        {
            FileName = fileName, Arguments = args,
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        },
        EnableRaisingEvents = true
      })
      {
        return await RunProcessAsync(process).ConfigureAwait(false);
      }
    }
    private static Task<int> RunProcessAsync(Process process)
    {
      var tcs = new TaskCompletionSource<int>();

      process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);
      process.OutputDataReceived += (s, ea) => Console.WriteLine(ea.Data);
      process.ErrorDataReceived += (s, ea) => Console.WriteLine("ERR: " + ea.Data);

      bool started = process.Start();
      if (!started)
      {
        //you may allow for the process to be re-used (started = false) 
        //but I'm not sure about the guarantees of the Exited event in such a case
        throw new InvalidOperationException("Could not start process: " + process);
      }

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      return tcs.Task;
    }
  }


}
