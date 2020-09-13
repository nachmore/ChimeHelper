using ChimeHelperUX.UXHelpers;
using System;
using System.Windows;

namespace ChimeHelperUX
{

  public partial class AboutWindow : Window
  {
    public static AboutWindow _aboutWindow;

    public static void CreateAndShow()
    {
      if (_aboutWindow == null || _aboutWindow.IsLoaded == false)
        _aboutWindow = new AboutWindow();

      _aboutWindow.Show();
      _aboutWindow.Activate();

      WindowPositioner.MoveToMouse(_aboutWindow);
    }

    public string Version { get; set; }

    private AboutWindow()
    {
      InitializeComponent();

      InitVersion();

      DataContext = this;
    }

    private void InitVersion()
    {
      Version = $"{ChimeHelperState.Instance.VersionString} Build Date: {ChimeHelperState.Instance.BuildDate:yyyy-MM-dd HH:mm}";
    }
  }
}
