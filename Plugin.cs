using Crucible;
using Crucible.Filesystem;
using CrucibleWemViewerPlugin.Model;
using System;
using System.IO;
using WEMSharp;

namespace CrucibleWemViewerPlugin
{
  public class Plugin : IPlugin
  {
    public static void Registration()
    {
      CrucibleApplication.RegisterFileUI(FileType.WEM, CreateTab);
    }

    private static FilesystemEntryTab CreateTab(IFilesystemEntry filesystemEntry)
    {
      try
      {
        var model = new CrucibleWemFile(filesystemEntry);       

        return new FilesystemEntryTab(filesystemEntry)
        {
          Header = filesystemEntry.Name,
          Content = new MainView() { DataContext = new MainViewModel() }
        };
      }
      catch (Exception e)
      {
        MainWindow.SetStatus($"WEM View Plugin failed to initialize: {e.Message}");
      }

      return null;
    }
  }
}
