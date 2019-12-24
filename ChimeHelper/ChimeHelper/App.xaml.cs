using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace ChimeHelper
{


  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private TaskbarIcon taskbarIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      taskbarIcon = (TaskbarIcon)FindResource("ChimeHelperTaskbarIcon");
    }

  }
}
