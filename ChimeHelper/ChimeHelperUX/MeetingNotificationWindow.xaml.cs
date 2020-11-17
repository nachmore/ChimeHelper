using ChimeHelperUX.UXHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  public partial class MeetingNotificationWindow : Window
  {

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

      WindowPositioner.MoveToMouse(_meetingNotificationWindow);
    }
    #endregion

    private MeetingNotificationWindow()
    {
      InitializeComponent();

      DataContext = this;
    }
  }
}
