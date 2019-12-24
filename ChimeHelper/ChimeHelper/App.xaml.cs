using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace ChimeHelper
{


  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public static TaskbarIcon TrayIcon { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      TrayIcon = (TaskbarIcon)FindResource("ChimeHelperTaskbarIcon");
    }

  }
}
