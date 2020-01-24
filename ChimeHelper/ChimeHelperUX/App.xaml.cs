using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace ChimeHelperUX
{

  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public static TaskbarIcon TrayIcon { get; set; }

    private Thread _exitRequestWaiter;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      ShutdownMode = ShutdownMode.OnExplicitShutdown;

      InitExitEventWaiter();

      TrayIcon = (TaskbarIcon)FindResource("ChimeHelperTaskbarIcon");

      new ChimeHelperTray(TrayIcon);
      ChimeHelperState.Instance.StartState();

      SettingsWindow.LoadSettings();
    }

    protected override void OnExit(ExitEventArgs e)
    {
      base.OnExit(e);

      _exitRequestWaiter?.Interrupt();
    }

    /// <summary>
    /// Monitors a global event that can be used to signal this process to exit, for example
    /// during install
    /// </summary>
    private void InitExitEventWaiter()
    {
      // create a rule that allows anybody in the "Users" group to synchronise with us
      var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
      var rule = new EventWaitHandleAccessRule(users, EventWaitHandleRights.FullControl, AccessControlType.Allow);
      
      var security = new EventWaitHandleSecurity();
      security.AddAccessRule(rule);

      EventWaitHandle eventWaitHandle;

      try
      {
        eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\nachmore.ChimeHelper.IsRunning", out bool createdNew, security);
      }
      catch (Exception e)
      {
        Debug.WriteLine("InitExitEventWaiter: " + e);

        // Windows will sometimes get into a weird state and throw AccessDenied, in these cases ignore the exception and continue to launch
        // since this can often be when a different ChimeHelper is stuck
        return;
      }

      _exitRequestWaiter = new Thread(() =>
        {
          try
          {
            eventWaitHandle.WaitOne();

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
              Application.Current.Shutdown();
            }));
          }
          catch (ThreadInterruptedException) { } // expected while exiting
        });

      _exitRequestWaiter.Start();
    }

  }
}
