using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
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

    public App() : base()
    {
      SetupUnhandledExceptionHanders();
    }

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
        eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, @"Global\nachmore.ChimeHelper.IsRunning", out bool createdNew);
        eventWaitHandle.SetAccessControl(security);
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

    // Based on: https://blog.danskingdom.com/Catch-and-display-unhandled-exceptions-in-your-WPF-app/
    private void SetupUnhandledExceptionHanders()
    {
      // Catch exceptions from all threads in the AppDomain.
      AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
          ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain UnhandledException", false);

      // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
      TaskScheduler.UnobservedTaskException += (sender, args) =>
          ShowUnhandledException(args.Exception, "TaskScheduler UnobservedTaskException", false);

      // Catch exceptions from the UI thread
      DispatcherUnhandledException += (sender, args) =>
      {
        // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
        if (!Debugger.IsAttached)
        {
          args.Handled = true;
          ShowUnhandledException(args.Exception, "Dispatcher UnhandledException", true);
        }
      };
    }

    void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
    {
      var messageBoxTitle = $"Chime Helper: {unhandledExceptionType}";

      var messageBoxMessage = $"Apologies, an unexpected error has occurred! Hit Ctrl+C and send us the output so that we can fix the issue!\n\n{e}";
      var messageBoxButtons = MessageBoxButton.OK;

      if (promptUserForShutdown)
      {
        messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
        messageBoxButtons = MessageBoxButton.YesNo;
      }

      // Let the user decide if the app should die or not (if applicable).
      if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons, MessageBoxImage.Error) == MessageBoxResult.Yes)
      {
        Application.Current.Shutdown();
      }
    }
  }
}
