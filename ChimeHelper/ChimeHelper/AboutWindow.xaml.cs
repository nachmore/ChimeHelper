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
    private AboutWindow()
    {
      InitializeComponent();
    }

    public static void CreateAndShow()
    {
      if (_aboutWindow == null || _aboutWindow.IsLoaded == false) 
        _aboutWindow = new AboutWindow();

      _aboutWindow.Show();
      _aboutWindow.Activate();
    }
  }
}
