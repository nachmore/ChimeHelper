using GitHubReleaseChecker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ChimeHelperUX.ChimeHelperTray;

namespace ChimeHelperUX
{
  /// <summary>
  /// Manages state for the ChimeHelper application, including the Tray and timer for detecting meetings
  /// </summary>
  class ChimeHelperState
  {

    #region Singleton

    private static ChimeHelperState _instance;
    public static ChimeHelperState Instance
    {
      get
      {
        if (_instance == null)
          _instance = new ChimeHelperState();

        return _instance;
      }
    }

    #endregion

    private const int DEFAULT_CHECK_INTERVAL_MIN = 15;

    /// <summary>
    /// The number of minutes to offset the interval since we want to be ready a few minutes
    /// *before* a meeting's start, as opposed to checking aligned to meeting start times.
    ///
    /// For example, interval = 15, offset = 2 -> will check at:
    /// :58, :13, :28, :43
    /// </summary>
    private const int DEFAULT_INTERVAL_OFFSET_MIN = 3;

    /// <summary>
    /// The interval of time after which our current cache of meetings is considered to be stale
    /// Note that there is no actual cache, but it is bound to the control, this mainly controls
    /// showing the loading icon and clearing the UI
    /// </summary>
    private const int STALE_INTERVAL_MIN = 1;

    private Timer _timer;
    private DateTime _lastCheck;

    private enum TimerState { STOPPED, FIRST, SECOND, ONGOING }

    // this could be passes as stateInfo to the Timer, but is useful for debugging
    private TimerState _timerState;

    public int TimerIntervalMinutes { get; set; }
    public ReleaseChecker UpdateState { get; set; }


    private static Version _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    private static double _versionDouble = Double.Parse($"{_version.Major}.{_version.Minor}");
    private static string _versionString = $"v{_version.Major}.{_version.Minor}";
    private static DateTime _versionBuildDate = new DateTime(2020, 1, 1).AddDays(_version.Build).AddMinutes(_version.MinorRevision);

    public double Version { get { return _versionDouble; } }
    public string VersionString { get { return _versionString; } }
    public DateTime BuildDate { get { return _versionBuildDate; } }

    private ChimeHelperState(int timerIntervalMinutes = DEFAULT_CHECK_INTERVAL_MIN)
    {
      TimerIntervalMinutes = timerIntervalMinutes;
    }

    public void StartState()
    {
      Debug.Assert(_timerState == TimerState.STOPPED);

      if (_timerState == TimerState.STOPPED)
      {
        StartMeetingTimer();
        StartCheckForUpdatesTimer();

        Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
      }
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

    private void StartMeetingTimer()
    {
      // timer runs as:
      // 1. 1st run happens after the first second
      // 2. 2nd run aligns to the closest TimerIntervalMinutes (initialPeriod)
      // 3. 3rd run+ happens every TimerIntervalMinutes

      _timerState = TimerState.FIRST;

      var initialPeriod = TimerIntervalMinutes - DEFAULT_INTERVAL_OFFSET_MIN - (DateTime.Now.Minute % TimerIntervalMinutes);

      _timer = new Timer(CheckForChimeMeetings, true, 1000, DEFAULT_CHECK_INTERVAL_MIN * 60 * 1000);

      Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] Spawning Timer with period: " + (DEFAULT_CHECK_INTERVAL_MIN * 60 * 1000));
    }

    public async void CheckForChimeMeetingsAsync()
    {

      await Task.Run(() =>
      {
        // TODO: this really needs to be its own function / reevaluate this being part of state
        App.Current.Dispatcher.Invoke(new Action(() =>
        {
          App.TrayIcon.DataContext = ChimeHelperTray.MEETINGS_LOADING;
        }));

        CheckForChimeMeetings(null);
      });
    }

    private void CheckForChimeMeetings(object stateInfo)
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
          // we want to be able to run aligned to the interval boundary - offset (so if it's 15 minutes,
          // with a 2 minute offset then check on the :58, :13, :28, :43), so immediately reset the
          // timer so that it triggers regardless of how long the actual check takes to run

          Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] Timer Moving to Ongoing");

          _timerState = TimerState.ONGOING;
          _timer.Change(TimerIntervalMinutes * 60 * 1000, TimerIntervalMinutes * 60 * 1000);
        }
      }

      _lastCheck = DateTime.Now;

      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      var meetingMenuItems = new ChimeMeetingMenuItems();

      foreach (var meeting in meetings)
      {
        meetingMenuItems.AddRange(ChimeMeetingMenuItem.Create(meeting));
      }

      App.Current.Dispatcher.BeginInvoke(new Action(() =>
      {
        App.TrayIcon.DataContext = (meetingMenuItems.Count > 0 ? meetingMenuItems : ChimeHelperTray.NO_MEETINGS);
      }));
    }

    private void StartCheckForUpdatesTimer()
    {
      var updateTimer = new Timer(CheckForUpdates, null, 0, 24 * 60 * 60);
    }

    private void CheckForUpdates(object state)
    {
      UpdateState = new ReleaseChecker("nachmore", "AmazonChimeHelper");
      UpdateState.MonitorForUpdates(VersionString);
    }
  }
}
