using ChimeHelperUX.UXHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChimeHelperUX
{
  /// <summary>
  /// Interaction logic for Settings.xaml
  /// </summary>
  public partial class MeetingNotificationWindow : Window, INotifyPropertyChanged
  {

    const int AUTO_HIDE_SEC = 2 * 60;
    private Timer _autoHide;

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    #region Singleton
    public static MeetingNotificationWindow _meetingNotificationWindow;

    // Not using "Instance" since this Singleton is more of an action
    public static void CreateAndShow(ChimeMeetingMenuItem meetingStartingNow)
    {
      if (_meetingNotificationWindow == null || _meetingNotificationWindow.IsLoaded == false)
        _meetingNotificationWindow = new MeetingNotificationWindow();

      _meetingNotificationWindow.CurrentMeetings = (ChimeMeetingMenuItems)(App.TrayIcon.DataContext);

      _meetingNotificationWindow.ResetAutoHideTimer();

      _meetingNotificationWindow.DefaultMeeting = meetingStartingNow;

      // refresh DataContext since we just set default meeting
      _meetingNotificationWindow.DataContext = _meetingNotificationWindow;

      WindowPositioner.MoveToMouse(_meetingNotificationWindow);
      _meetingNotificationWindow.Show();
      _meetingNotificationWindow.Topmost = true;
    }
    #endregion

    private ChimeMeetingMenuItem _defaultMeeting;
    public ChimeMeetingMenuItem DefaultMeeting
    {
      get
      {
        return _defaultMeeting;
      }

      set
      {
        _defaultMeeting = value;
        NotifyPropertyChanged();
      }
    }

    ChimeMeetingMenuItems _currentMeetings;
    public ChimeMeetingMenuItems CurrentMeetings
    {
      get
      {
        return _currentMeetings;
      }

      set
      {
        _currentMeetings = value;
        NotifyPropertyChanged();
      }
    }
        
    private void ResetAutoHideTimer()
    {
      if (_autoHide != null)
      {
        // cancel the existing timer
        _autoHide.Change(Timeout.Infinite, Timeout.Infinite);
      }

      _autohideRemainingSeconds = AUTO_HIDE_SEC;
      _autoHide = new Timer(new TimerCallback(HideWindowCheck), null, 1000, 1000);
    }

    private string _autohideRemainingTimeString;

    public string AutoHideRemainingTimeString
    {
      get
      {
        return _autohideRemainingTimeString;
      }
      set
      {
        _autohideRemainingTimeString = value;

        NotifyPropertyChanged();
      }
    }

    private int _autohideRemainingSeconds;
    public int AutoHideRemainingSeconds
    {

      get 
      {
        return _autohideRemainingSeconds;
      }
      set
      {
        _autohideRemainingSeconds = value;

        var span = new TimeSpan(0, 0, value);
        AutoHideRemainingTimeString = span.ToString(@"m\:ss");
        
        NotifyPropertyChanged();
      }
    }

    private void HideWindowCheck(object state)
    {
      AutoHideRemainingSeconds--;

      if (AutoHideRemainingSeconds < 0)
      {
        CloseMeetingNotification();
      }
    }

    private MeetingNotificationWindow()
    {
      InitializeComponent();

      this.Closed += MeetingNotificationWindow_Closed;
    }

    private void MeetingNotificationWindow_Closed(object sender, EventArgs e)
    {
      if (_autoHide != null)
        _autoHide.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void btnJoinCall_Click(object sender, RoutedEventArgs e)
    {
      ChimeHelper.Chime.Join(DefaultMeeting.Pin);
      Hide();
    }

    private void btnIgnore_Click(object sender, RoutedEventArgs e)
    {
      CloseMeetingNotification();
    }

    private void CloseMeetingNotification()
    {
      _autoHide.Change(Timeout.Infinite, Timeout.Infinite);

      Dispatcher.BeginInvoke(new Action(() =>
      {
        Hide();
      }));

    }

    public ICommand JoinMeetingCommand
    {
      get
      {
        return new ActionCommand(
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

              ChimeHelper.Chime.Join(meeting.Pin);

              Hide();
            }
          }
        );
      }
    }
  }
}
