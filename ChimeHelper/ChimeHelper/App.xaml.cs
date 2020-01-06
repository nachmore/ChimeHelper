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

      ShutdownMode = ShutdownMode.OnExplicitShutdown;

      TrayIcon = (TaskbarIcon)FindResource("ChimeHelperTaskbarIcon");

      new ChimeHelperTray(TrayIcon);
      ChimeHelperState.Create();

      SettingsWindow.LoadSettings();
    }

  }
}
