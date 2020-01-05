using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static ChimeOutlookHelper.ChimeOutlookHelper;

namespace ChimeHelper
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
      var buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2).ToString("u");

      Version = $"{version} ({buildDate})";
    }
  }
}
