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
    public static void CreateAndShow()
    {
      if (_meetingNotificationWindow == null || _meetingNotificationWindow.IsLoaded == false)
        _meetingNotificationWindow = new MeetingNotificationWindow();

      _meetingNotificationWindow.Show();
      _meetingNotificationWindow.Activate();
      _meetingNotificationWindow.Topmost = true;

      _meetingNotificationWindow.DataContext = App.TrayIcon.DataContext;
      _meetingNotificationWindow.RemainingTimeText.DataContext = _meetingNotificationWindow;

      _meetingNotificationWindow.ResetAutoHideTimer();

      WindowPositioner.MoveToMouse(_meetingNotificationWindow);
    }
    #endregion

    private void ResetAutoHideTimer()
    {
      if (_autoHide != null)
      {
        // cancel the existing timer
        _autoHide.Change(Timeout.Infinite, Timeout.Infinite);
      }

      _remainingAutoHideTime = AUTO_HIDE_SEC;
      _autoHide = new Timer(new TimerCallback(HideWindowCheck), null, 1000, 1000);
    }

    private int _remainingAutoHideTime;

    private string _remainingTimeString;

    public string RemainingTimeString
    {
      get
      {
        return _remainingTimeString;
      }
      set
      {
        _remainingTimeString = value;

        NotifyPropertyChanged();
      }
    }

    public int AutoHideRemainingSeconds
    {

      get 
      {
        return _remainingAutoHideTime;
      }
      set
      {
        _remainingAutoHideTime = value;

        var span = new TimeSpan(0, 0, value);
        RemainingTimeString = span.ToString(@"m\:ss");
        
        NotifyPropertyChanged();
      }
    }

    private void HideWindowCheck(object state)
    {
      AutoHideRemainingSeconds--;

      if (AutoHideRemainingSeconds < 0)
      {
        _autoHide.Change(Timeout.Infinite, Timeout.Infinite);

        Dispatcher.BeginInvoke(new Action(() => 
        {

          Hide();
        }));
      }
    }

    private MeetingNotificationWindow()
    {
      InitializeComponent();

      DataContext = this;
    }

    private void btnJoinCall_Click(object sender, RoutedEventArgs e)
    {

    }

    private void btnIgnore_Click(object sender, RoutedEventArgs e)
    {

    }

    private void refresh_Click(object sender, RoutedEventArgs e)
    {
      DataContext = null;
      DataContext = App.TrayIcon.DataContext;
    }
  }
}
