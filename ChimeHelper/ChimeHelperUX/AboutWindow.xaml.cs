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

      var monthDay = version.Build.ToString();
      var buildDate = version.Minor + "-" + monthDay.Insert(monthDay.Length / 2, "-");

      Version = $"{version.Major}.{buildDate}.{version.Revision}";
    }
  }
}
