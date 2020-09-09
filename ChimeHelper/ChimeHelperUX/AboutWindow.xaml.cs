﻿using ChimeHelperUX.UXHelpers;
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
      var buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2).ToString("u");

      Version = $"{version} ({buildDate})";
    }
  }
}
