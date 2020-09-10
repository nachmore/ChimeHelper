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
      var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

      var date = new DateTime(2020, 1, 1).AddDays(version.Build).AddMinutes(version.Build);

      Version = $"{version.Major}.{version.Minor} Build Date: {date:yyyy-MM-dd HH:mm}";
    }
  }
}
