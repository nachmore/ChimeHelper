using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using static ChimeOutlookHelper.ChimeOutlookHelper;

namespace ChimeHelper
{

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    private class ChimeMeetingMenuItem : IChimeMeeting
    {
      public string Subject { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public HashSet<string> Meetings { get; set; }

      public ChimeMeetingMenuItem(ChimeMeeting meeting)
      {
        this.Subject = meeting.Subject;
        this.StartTime = meeting.StartTime;
        this.EndTime = meeting.EndTime;
        this.Meetings = meeting.Meetings;
      }

      public ICommand JoinMeetingCommand
      {
        get
        {
          return new DelegateCommand(
            (object parameter) => {
              MessageBox.Show(((ChimeMeetingMenuItem)parameter).Subject);
            }
          );
          
        }
      }
    }

    List<ChimeMeetingMenuItem> _meetings;

    public MainWindow()
    {
      InitializeComponent();

      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      _meetings = new List<ChimeMeetingMenuItem>();

      foreach (var meeting in meetings)
      {
        _meetings.Add(new ChimeMeetingMenuItem(meeting));
      }

    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      App.TrayIcon.DataContext = _meetings;
    }
  }
}
