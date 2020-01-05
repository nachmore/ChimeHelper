using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static ChimeOutlookHelper.ChimeOutlookHelper;

namespace ChimeHelper
{
  class ChimeHelperTray
  {
    private readonly TaskbarIcon _tray;

    protected const string DEFAULT_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan.ico";
    protected const string NO_MEETINGS_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan-off.ico";
    protected const string LOADING_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan-loading.ico";

    /// <summary>
    /// Explicitly separate from ChimeOutlookHelper.ChimeMeeting as these represent single meeting pins
    /// </summary>
    public class ChimeMeetingMenuItem
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
    }

    public class ChimeMeetingMenuItems<T> : List<T>
    {
      public string IconSource { get; set; }
      public string ToolTipText { get; set; }

      public ChimeMeetingMenuItems(string iconURI = DEFAULT_ICON, string tooltip = "")
      {
        IconSource = iconURI;
        ToolTipText = tooltip;
      }

      public ICommand JoinMeetingCommand
      {
        get
        {
          return new DelegateCommand(
            (object parameter) =>
            {
              var datagrid = (DataGrid)parameter;
              var meeting = (ChimeMeetingMenuItem)datagrid.SelectedItem;

              // will be null when we clear the selected item, since that also triggers a change event
              // or when the DG's DataContext is refreshed
              if (meeting != null)
              {
                // clears the selection style
                datagrid.SelectedCells.Clear();

                // clears the actual selected item, allows the same item to be selected again
                datagrid.UnselectAll();
                System.Diagnostics.Process.Start(String.Format(ChimeOutlookHelper.ChimeOutlookHelper.MEETING_URL_FORMAT, meeting.Pin));
              }
            }
          );

        }
      }

      public ICommand AboutMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            MainWindow.CreateAndShow();
          }
         );
        }
      }

      public ICommand ExitMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            App.Current.Shutdown();
          }
         );
        }
      }
    }
        
    public static readonly ChimeMeetingMenuItems<ChimeMeetingMenuItem> NO_MEETINGS = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>(NO_MEETINGS_ICON, "Chime Helper: No Meetings Found") {
      new ChimeMeetingMenuItem()
        {
          Subject = "No meetings!",
          IsEmpty = true
        }
    };

    public static readonly ChimeMeetingMenuItems<ChimeMeetingMenuItem> MEETINGS_LOADING = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>(LOADING_ICON, "Chime Helper: Loading...") {
      new ChimeMeetingMenuItem()
      {
        Subject = "Loading...",
        IsEmpty = true
      }
    };

    public ChimeHelperTray(TaskbarIcon trayIcon)
    {
      _tray = trayIcon;
      _tray.DataContext = MEETINGS_LOADING;
    }

  }
}
