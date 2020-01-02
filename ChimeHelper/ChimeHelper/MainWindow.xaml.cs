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

    /// <summary>
    /// Explicitly separate from ChimeOutlookHelper.ChimeMeeting as these represent single meeting pins
    /// </summary>
    private class ChimeMeetingMenuItem
    {
      public string Subject { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public string Pin { get; set; }

      public ChimeMeetingMenuItem() { }

      public static List<ChimeMeetingMenuItem> Create(ChimeMeeting meeting)
      {
        var rv = new List<ChimeMeetingMenuItem>();

        foreach (var pin in meeting.Pins)
        {
          rv.Add(new ChimeMeetingMenuItem(meeting, pin));
        }

        return rv;
      }

      private ChimeMeetingMenuItem(ChimeMeeting meeting, string pin)
      {
        this.Subject = meeting.Subject;
        this.StartTime = meeting.StartTime;
        this.EndTime = meeting.EndTime;
        this.Pin = pin;
      }

      public ICommand JoinMeetingCommand
      {
        get
        {
          return new DelegateCommand(
            (object parameter) => {
              var meeting = (ChimeMeetingMenuItem)parameter;

              System.Diagnostics.Process.Start(String.Format(ChimeOutlookHelper.ChimeOutlookHelper.MEETING_URL_FORMAT, meeting.Pin));
            }
          );
          
        }
      }
    }

    List<ChimeMeetingMenuItem> _meetings;

    public MainWindow()
    {

      InitializeComponent();

    }

    private void button_Click(object sender, RoutedEventArgs e)
    {

      /*

 for (var i = 0; i < 4; i++)
 {
   _meetings.Add(new ChimeMeetingMenuItem() { Subject = @"{i} random subject" });
 }

 App.TrayIcon.DataContext = _meetings;*/

      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      _meetings = new List<ChimeMeetingMenuItem>();

      foreach (var meeting in meetings)
      {
        _meetings.AddRange(ChimeMeetingMenuItem.Create(meeting));
      }

      App.TrayIcon.DataContext = _meetings;
    }
  }
}
