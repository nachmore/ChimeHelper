using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ChimeHelper.ChimeHelperTray;

namespace ChimeHelper
{
  /// <summary>
  /// Manages state for the ChimeHelper application, including the Tray and timer for detecting meetings
  /// </summary>
  class ChimeHelperState
  {

    #region Singleton

    private static ChimeHelperState _instance;
    public static ChimeHelperState Create()
    {
      if (_instance == null)
        _instance = new ChimeHelperState();

      return _instance;
    }

    #endregion

    private const int DEFAULT_CHECK_INTERVAL_MIN = 15;

    /// <summary>
    /// The interval of time after which our current cache of meetings is considered to be stale
    /// Note that there is no actual cache, but it is bound to the control, this mainly controls
    /// showing the loading icon and clearing the UI
    /// </summary>
    private const int STALE_INTERVAL_MIN = 1;

    public int TimerIntervalMinutes { get; set; }

    private Timer _timer;
    private DateTime _lastCheck;

    private enum TimerState { FIRST, SECOND, ONGOING }

    // this could be passes as stateInfo to the Timer, but is useful for debugging
    private TimerState _timerState;

    private ChimeHelperState(int timerIntervalMinutes = DEFAULT_CHECK_INTERVAL_MIN) 
    {
      TimerIntervalMinutes = timerIntervalMinutes;

      StartMeetingTimer();

      Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
    {
      if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock) 
      {

        var timeSinceLastCheck = DateTime.Now.Subtract(_lastCheck).TotalMinutes;

        if (timeSinceLastCheck >= TimerIntervalMinutes)
        {

          if (timeSinceLastCheck >= STALE_INTERVAL_MIN)
          {
            // on unlock when refreshing we explicitly want the user to see that we are refreshing
            // but only if we believe that the data is way out of date
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
              App.TrayIcon.DataContext = ChimeHelperTray.MEETINGS_LOADING;
            }));
          }

          CheckForChimeMeetings(null);
        }
      }
    }

    public void StartMeetingTimer()
    {
      // timer runs as:
      // 1. 1st run happens after the first second
      // 2. 2nd run aligns to the closest TimerIntervalMinutes (initialPeriod)
      // 3. 3rd run+ happens every TimerIntervalMinutes

      _timerState = TimerState.FIRST;

      var initialPeriod = TimerIntervalMinutes - (DateTime.Now.Minute % TimerIntervalMinutes);

      _timer = new Timer(CheckForChimeMeetings, true, 1000, DEFAULT_CHECK_INTERVAL_MIN * 60 * 1000);

      Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] Spawning Timer with period: " + (DEFAULT_CHECK_INTERVAL_MIN * 60 * 1000));
    }

    public void CheckForChimeMeetings(object stateInfo)
    {
      Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] CheckForChimeMeetings() called");

      // stateInfo will be a bool when called from the Timer
      if (stateInfo is bool)
      {
        if (_timerState == TimerState.FIRST)
        {
          Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] Timer Moving to 2nd state");
          _timerState = TimerState.SECOND;
        }
        else if (_timerState == TimerState.SECOND)
        {
          // we want to be able to run aligned to the interval boundary (so if it's 15 minutes,
          // on the :00, :15, :30, :45), so immediately reset the timer so that it triggers regardless
          // of how long the actual check takes to run

          Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] Timer Moving to Ongoing");

          _timerState = TimerState.ONGOING;
          _timer.Change(TimerIntervalMinutes * 60 * 1000, TimerIntervalMinutes * 60 * 1000);
        }
      }

      _lastCheck = DateTime.Now;

      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      var meetingMenuItems = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>();

      foreach (var meeting in meetings)
      {
        meetingMenuItems.AddRange(ChimeMeetingMenuItem.Create(meeting));
      }

      App.Current.Dispatcher.Invoke(new Action(() =>
      {
        App.TrayIcon.DataContext = (meetingMenuItems.Count > 0 ? meetingMenuItems : ChimeHelperTray.NO_MEETINGS);
      }));
    }

  }
}
