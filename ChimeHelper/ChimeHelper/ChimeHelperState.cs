using System;
using System.Collections.Generic;
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
    private static ChimeHelperState _instance;
    public static ChimeHelperState Create()
    {
      if (_instance == null)
        _instance = new ChimeHelperState();

      return _instance;
    }

    private Timer _timer;

    private ChimeHelperState() 
    {
      StartMeetingTimer();
    }

    public void StartMeetingTimer()
    {
      _timer = new Timer(CheckForChimeMeetings, null, 1000, 15 * 60 * 1000);
    }

    public void CheckForChimeMeetings(object stateInfo)
    {
      var meetings = ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings();
      var _meetings = new ChimeMeetingMenuItems<ChimeMeetingMenuItem>();

      foreach (var meeting in meetings)
      {
        _meetings.AddRange(ChimeMeetingMenuItem.Create(meeting));
      }

      App.Current.Dispatcher.Invoke(new Action(() =>
      {
        App.TrayIcon.DataContext = (_meetings.Count > 0 ? _meetings : ChimeHelperTray.NO_MEETINGS);
      }));
    }

  }
}
