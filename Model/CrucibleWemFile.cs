using Crucible;
using Crucible.Filesystem;
using System;
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
      if (_rawData == null)
      {
        _rawData = await Task.Run(() => GetRawBytes());
      }
      return _rawData.Length;
    }

    private byte[] GetRawBytes()
    {
      return _fileSystemEntry.GetData();
    }

    public async Task<string> ConvertAsync()
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

      var codebookPath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Plugins\WemViewerPlugin_Resources\packed_codebooks_aoTuV_603.bin");
      try
      {

        wemFile.GenerateOGG(oggFilePath, codebookPath, false, false);
      }
      catch (Exception e)
      {
        // TODO: fail gacefully
        throw new InvalidOperationException($"Failed to write to {oggFilePath}.\\nCodebook path: {codebookPath}", e);
      }

      var revorbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Plugins\WemViewerPlugin_Resources\revorb.exe");

      var exitCode = await External.RunProcessAsync(revorbPath, oggFilePath);

      MainWindow.SetStatus($"Wrote {oggFilePath}");

      return oggFilePath;
    }
  }
}
