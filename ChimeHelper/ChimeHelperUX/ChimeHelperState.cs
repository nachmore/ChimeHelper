using GitHubReleaseChecker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    private const int DEFAULT_INTERVAL_OFFSET_MIN = 1;

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

    int _lastMeetingNotificationMinute = -1;

    private static readonly Version _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    private static readonly double _versionDouble = Double.Parse($"{_version.Major}.{_version.Minor}");
    private static readonly string _versionString = $"v{_version.Major}.{_version.Minor}";
    private static readonly DateTime _versionBuildDate = new DateTime(2021, 1, 1).AddDays(_version.Build).AddMinutes(_version.MinorRevision);

    public double Version { get { return _versionDouble; } }
    public string VersionString { get { return _versionString; } }
    public DateTime BuildDate { get { return _versionBuildDate; } }

    private ChimeMeetingMenuItems _meetingMenuItemCache;

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
        StartCheckForUpdates();
        StartCheckForStartingMeeting();

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

          CheckForChimeMeetingsAsync();
        }
      }
    }

    private void StartMeetingTimer()
    {
      // timer runs as:
      // 1. 1st run happens after the first second
      // 2. 2nd run aligns to the closest TimerIntervalMinutes (initialPeriod) and approximately :00 seconds
      // 3. 3rd run+ happens every TimerIntervalMinutes

      var now = DateTime.Now;
      var initialPeriod = TimerIntervalMinutes - DEFAULT_INTERVAL_OFFSET_MIN - (now.Minute % TimerIntervalMinutes);

      // if we're already within the offset period then this will be negative, so the next iteration should start
      // within the next interval minus however much we're already into that interval.
      //
      // for example, if we're at :44 with a 15 minute interval and a 3 minute offset then the next run should
      // happen at :57. initialPeriod will be -2 so we'll run in (interval: 15) - 2: 13 minutes (which is :57).
      if (initialPeriod <= 0)
        initialPeriod += TimerIntervalMinutes;

      var initialPeriodMillis = initialPeriod * 60 * 1000 - (now.Second * 1000);

      _timer = new Timer(CheckForChimeMeetings, true, 1000, initialPeriodMillis);
      _timerState = TimerState.FIRST;

      Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] Spawning Timer with period: " + (initialPeriodMillis));
    }

    internal void CheckForChimeMeetings(object stateInfo)
    {
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

          var interval = TimerIntervalMinutes * 60 * 1000;

          _timerState = TimerState.ONGOING;
          _timer.Change(interval, interval);

          Debug.WriteLine(DateTime.Now + $":[ChimeHelperState] Timer Moving to Ongoing: Minute: {DateTime.Now.Minute} Interval: {TimerIntervalMinutes} minutes");
        }
      }

      // explicitly do not await the extra call but drive it forward
      _ = CheckForChimeMeetingsAsync();
    }

    internal Task CheckForChimeMeetingsAsync()
    {
      // explicitly make this synchronous code async so that it runs in the background
      // and allows UX to continue updating
      return Task.Run(async () =>
      {
        Debug.WriteLine(DateTime.Now + ":[ChimeHelperState] CheckForChimeMeetings() called");

        // TODO: this really needs to be its own function / reevaluate this being part of state
        App.Current.Dispatcher.Invoke(new Action(() =>
        {
          App.TrayIcon.DataContext = ChimeHelperTray.MEETINGS_LOADING;
        }));

        _lastCheck = DateTime.Now;

        var autoLaunchOutlook = Properties.Settings.Default.AutoLaunchOutlook;
        var meetings = await ChimeOutlookHelper.ChimeOutlookHelper.GetMeetingsAsync(autoLaunchOutlook);

        _meetingMenuItemCache = new ChimeMeetingMenuItems();

        foreach (var meeting in meetings)
        {
          _meetingMenuItemCache.AddRange(ChimeMeetingMenuItem.Create(meeting));
        }

        _ = App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {

          var menuItems = ChimeHelperTray.NO_MEETINGS;

          if (_meetingMenuItemCache.Count > 0)
          {
            menuItems = _meetingMenuItemCache;
          } 
          else if (!ChimeOutlookHelper.ChimeOutlookHelper.OutlookRunning())
          {
            menuItems = ChimeHelperTray.OUTLOOK_NOT_RUNNING;
          }

          App.TrayIcon.DataContext = menuItems;
        }));

        CheckForMeetingStart();
      });
    }

    private void StartCheckForUpdates()
    {
      UpdateState = new ReleaseChecker("nachmore", "ChimeHelper");
      UpdateState.UnhandledException += UpdateState_UnhandledException;
      UpdateState.MonitorForUpdates(VersionString);
    }

    private Timer _timerCheckForMeetingStart;

    private void StartCheckForStartingMeeting()
    {
      // we're already active or the user doesn't actually want this functionality
      if (_timerCheckForMeetingStart != null || !Properties.Settings.Default.NotifyOnMeetingStart)
        return;

      _timerCheckForMeetingStart = new Timer(
                                    CheckForMeetingStart,
                                    null,
                                    (60 - DateTime.Now.Second) * 1000,
                                    60 * 1000);
    }

    private void StopCheckForStartingMeeting()
    {
      if (_timerCheckForMeetingStart != null)
      {
        _timerCheckForMeetingStart.Change(Timeout.Infinite, Timeout.Infinite);
        _timerCheckForMeetingStart = null;
      }
    }

    /// <summary>
    /// Checks for the start of a meeting and pops up a join dialog if the user has requested one
    /// </summary>
    /// <param name="stateInfo"></param>
    public void CheckForMeetingStart(object stateInfo = null)
    {
      var now = DateTime.Now;

      // since this function can be called from both the timer as well as "manually", for example
      // when a meeting refresh occurs, ensure that it does not pop up a dialog more than once 
      // in a given minute (which will happen if the user closes the dialog then a refresh runs
      // and causes it to open again)

      if (_lastMeetingNotificationMinute == now.Minute)
        return;

      if (!Properties.Settings.Default.NotifyOnMeetingStart)
        return;

      if (_meetingMenuItemCache == null)
        return;

      // this is mainly for debugging, prevent multiple queued up ticks
      StopCheckForStartingMeeting();

      _lastMeetingNotificationMinute = -1;

      foreach (var item in _meetingMenuItemCache)
      {
        
        if (item.StartTime.Day == now.Day &&
            item.StartTime.Hour == now.Hour &&
            item.StartTime.Minute == now.Minute &&
            !ChimeHelper.Chime.IsMeetingAlreadyJoined(item.Subject))
        {

          _lastMeetingNotificationMinute = now.Minute;

          App.Current.Dispatcher.BeginInvoke(new Action(() =>
          {
            MeetingNotificationWindow.CreateAndShow(item);
          }));
          break;
        }
      }

      StartCheckForStartingMeeting();
    }

    private void UpdateState_UnhandledException(object sender, Exception e)
    {
      Debug.WriteLine($"(ignored) Exception checking for updates:\n\n{e}");
    }
  }
}
