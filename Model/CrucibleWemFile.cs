using Crucible;
using Crucible.Filesystem;
using System;
using System.IO;
using WEMSharp;

namespace CrucibleWemViewerPlugin.Model
{
  internal class CrucibleWemFile
  {
    private readonly IFilesystemEntry _fileSystemEntry;

    public CrucibleWemFile(IFilesystemEntry filesystemEntry)
    {
      _fileSystemEntry = filesystemEntry;
      Convert();
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
      
      var wemFile=new WEMFile(wemFilePath, WEMForcePacketFormat.NoForcePacketFormat);
      MainWindow.SetStatus($"Wrote {wemFilePath}");

      wemFile.GenerateOGG(oggFilePath, "packed_codebooks_aoTuV_603", false, false);

      MainWindow.SetStatus($"Wrote {oggFilePath}");
    }

  }
}
