using Crucible;
using Crucible.Filesystem;
using CrucibleWemViewerPlugin.Model;
using System;

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
        var viewModel = new MainViewModel(model);
        var view = new MainView() { DataContext = viewModel };

        var fsTab = new FilesystemEntryTab(filesystemEntry)
        {
          Header = "WEM Audio",
          Content = view
        };

        return fsTab;
      }
      catch (Exception e)
      {
        MainWindow.SetStatus($"WEM View Plugin failed to initialize: {e.Message}");
      }

      return null;
    }
  }
}
