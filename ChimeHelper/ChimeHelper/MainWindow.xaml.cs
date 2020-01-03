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

    /// <summary>
    /// Explicitly separate from ChimeOutlookHelper.ChimeMeeting as these represent single meeting pins
    /// </summary>
    private class ChimeMeetingMenuItem
    {
      public string Subject { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public string Pin { get; set; }

      public bool IsEmpty { get; set; }

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

    private const string DEFAULT_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan.ico";
    private const string NO_MEETINGS_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan-off.ico";
    private const string LOADING_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan-loading.ico";

    private class ChimeMeetingMenuItems<T> : List<T>
    {
      public string IconSource { get; set; }
      public string ToolTipText { get; set; }

      public ChimeMeetingMenuItems(string iconURI = DEFAULT_ICON, string tooltip = "")
      {
        IconSource = iconURI;
        ToolTipText = tooltip;
      }
    }

    private List<ChimeMeetingMenuItem> _meetings;

    private static readonly ChimeMeetingMenuItems<ChimeMeetingMenuItem> NO_MEETINGS = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>(NO_MEETINGS_ICON, "Chime Helper: No Meetings Found") {
      new ChimeMeetingMenuItem()
        {
          Subject = "No meetings!",
          IsEmpty = true
        }
    };

    private static readonly ChimeMeetingMenuItems<ChimeMeetingMenuItem> MEETINGS_LOADING = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>(LOADING_ICON, "Chime Helper: Loading...") {
      new ChimeMeetingMenuItem() 
      {
        Subject = "Loading...",
        IsEmpty = true
      }
    };

    public MainWindow()
    {
      InitializeComponent();

      App.TrayIcon.DataContext = MEETINGS_LOADING;
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {

      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      _meetings = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>();

      foreach (var meeting in meetings)
      {
        _meetings.AddRange(ChimeMeetingMenuItem.Create(meeting));
      }
      
      App.TrayIcon.DataContext = (_meetings.Count > 0 ? _meetings : NO_MEETINGS);
    }
  }
}
