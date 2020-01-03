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
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public static MainWindow _mainWindow;
    private MainWindow()
    {
      InitializeComponent();
    }

    public static void CreateAndShow()
    {
      if (_mainWindow == null) 
        _mainWindow = new MainWindow();

      _mainWindow.Show();
    }
    
    
    private void button_Click(object sender, RoutedEventArgs e)
    {
      /*
      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      _meetings = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>();

      foreach (var meeting in meetings)
      {
        _meetings.AddRange(ChimeMeetingMenuItem.Create(meeting));
      }
      
      App.TrayIcon.DataContext = (_meetings.Count > 0 ? _meetings : NO_MEETINGS);
      */
    }
  }
}
