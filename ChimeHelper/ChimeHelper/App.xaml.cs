using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
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

      InitExitEventWaiter();

      TrayIcon = (TaskbarIcon)FindResource("ChimeHelperTaskbarIcon");

      new ChimeHelperTray(TrayIcon);
      ChimeHelperState.Create();

      SettingsWindow.LoadSettings();
    }

    /// <summary>
    /// Monitors a global event that can be used to signal this process to exit, for example
    /// during install
    /// </summary>
    private void InitExitEventWaiter()
    {
      // create a rule that allows anybody in the "Users" group to synchronise with us
      var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
      var rule = new EventWaitHandleAccessRule(users, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify, AccessControlType.Allow);
      
      var security = new EventWaitHandleSecurity();
      security.AddAccessRule(rule);

      bool createdNew;
      var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\nachmore.ChimeHelper.IsRunning", out createdNew, security);

      new Thread(() =>
      {
        eventWaitHandle.WaitOne();

        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
          Application.Current.Shutdown();
        }));
      }).Start();

    }

  }
}
