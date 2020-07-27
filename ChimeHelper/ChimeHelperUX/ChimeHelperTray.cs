using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static ChimeOutlookHelper.ChimeOutlookHelper;

namespace ChimeHelperUX
{
  class ChimeHelperTray
  {
    private readonly TaskbarIcon _tray;

    protected const string DEFAULT_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan.ico";
    protected const string NO_MEETINGS_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan-off.ico";
    protected const string LOADING_ICON = "pack://application:,,,/ChimeHelper;component/Icons/fan-loading.ico";

    /// <summary>
    /// Explicitly separate from ChimeOutlookHelper.ChimeMeeting as these represent single meeting pins
    /// </summary>
    public class ChimeMeetingMenuItem
    {
      public string Subject { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }


      private string _pin;
      public string Pin 
      {
        get { return _pin; }
        set 
        {
          if (long.TryParse(value, out long parsedPin))
          {
            // we convert the pin to a number in order to parse it out nicely, but if there
            // are leading zeros (0) we need to add them back in since they will be stripped 
            // out by the conversion, we do this by padding to the size of the original number
            // + the 2 extra spaces that we've added.
            _pin = parsedPin.ToString("### #### ###").PadLeft(value.Length + 2, '0');
          }
          else
          {
            _pin = value;
          }
        }
      }

      public ChimeMeetingMenuItem() { }

      public static List<ChimeMeetingMenuItem> Create(ChimeMeeting meeting)
      {
        var rv = new List<ChimeMeetingMenuItem>();

        foreach (var pin in meeting.Pins)
        {
          rv.Add(new ChimeMeetingMenuItem(meeting, pin));
        }

        return rv;
      }

      private ChimeMeetingMenuItem(ChimeMeeting meeting, string pin)
      {
        this.Subject = meeting.Subject;
        this.StartTime = meeting.StartTime;
        this.EndTime = meeting.EndTime;
        this.Pin = pin;
      }
    }

    public class ChimeMeetingMenuItems<T> : List<T>
    {
      public string IconSource { get; set; }
      public string ToolTipText { get; set; }
      public bool IsEmpty { get { return Count == 0; } }

      public ChimeMeetingMenuItems(string iconURI = DEFAULT_ICON, string tooltip = "Chime Helper")
      {
        IconSource = iconURI;
        ToolTipText = tooltip;
      }

      public ICommand JoinMeetingCommand
      {
        get
        {
          return new DelegateCommand(
            (object parameter) =>
            {
              var datagrid = (DataGrid)parameter;
              var meeting = (ChimeMeetingMenuItem)datagrid.SelectedItem;

              // will be null when we clear the selected item, since that also triggers a change event
              // or when the DG's DataContext is refreshed
              if (meeting != null)
              {
                // clears the selection style
                datagrid.SelectedCells.Clear();

                // clears the actual selected item, allows the same item to be selected again
                datagrid.UnselectAll();

                ChimeHelper.Chime.Join(meeting.Pin);

                // hide the popup on meeting selection
                var popup = datagrid.FindAncestor<Popup>();
                popup.IsOpen = false;
              }
            }
          );
        }
      }

      public ICommand StartMeetingMenuCommand
      {
        get
        {
          return new DelegateCommand(
            (object parameter) =>
            {
              // if this came from the popup then close the popup
              if (parameter is Popup)
              {
                ((Popup)parameter).IsOpen = false;
              }

              var personalID = Properties.Settings.Default.ChimeBridgePersonalID ?? Properties.Settings.Default.ChimeBridgePersonalizedID;
              var personalizedID = Properties.Settings.Default.ChimeBridgePersonalizedID ?? personalID;

              if (string.IsNullOrEmpty(personalID) && string.IsNullOrEmpty(personalizedID))
              {
                SettingsWindow.CreateAndShow();
              }
              else
              {
                ChimeHelper.Chime.Join(personalizedID);
              }
            }
          );
        }
      }

      public ICommand ShowJoinMeetingDialogMenuCommand
      {
        get
        {
          return new DelegateCommand(
            (object paramater) =>
            {
              JoinMeetingDialog.CreateAndShow();
            }
          );
        }
      }

      public ICommand ShortChimeStringMenuCommand
      {
        get
        {
          return new DelegateCommand(
            (object parameter) =>
            {

              var personalID = Properties.Settings.Default.ChimeBridgePersonalID;
              var personalizedID = Properties.Settings.Default.ChimeBridgePersonalizedID;

              if (string.IsNullOrEmpty(personalID) &&
                  string.IsNullOrEmpty(personalizedID))
              {
                SettingsWindow.CreateAndShow();
              } 
              else
              {

                string chimeText;

                if (personalID != null && personalizedID != null)
                {
                  chimeText = $"Chime ({personalizedID} / {personalID})";
                } 
                else if (personalID != null)
                {
                  chimeText = $"Chime ({personalID})";
                } 
                else
                {
                  chimeText = $"Chime ({personalizedID})";
                }

                Clipboard.SetText(chimeText);
              }
            }
         );
        }
      }
      public ICommand FullChimeStringMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            var personalID = Properties.Settings.Default.ChimeBridgePersonalID ?? Properties.Settings.Default.ChimeBridgePersonalizedID;
            var personalizedID = Properties.Settings.Default.ChimeBridgePersonalizedID ?? personalID;

            if (string.IsNullOrEmpty(personalID) && string.IsNullOrEmpty(personalizedID))
            {
              SettingsWindow.CreateAndShow();
              return;
            }

            var fullChimeText = 
$@"You have been invited to an online meeting, powered by Amazon Chime.

1.Click to join the meeting: https://chime.aws/{personalizedID}
  Meeting ID: {personalizedID}
  Meeting PIN: {personalID}

2.You can use your computer’s microphone and speakers; however, a headset is recommended.Or, call in using your phone:

  Meeting PIN: {personalID}

  One-click Dial-in (US)    : +1 206 - 462 - 5569,,3795341803#
  United States Toll - Free : +1 855 - 552 - 4463
  United States(1)          : +1 206 - 462 - 5569
  United States(2)          : +1 929 - 432 - 4463
  Australia Toll-Free(1)    : +61 1800 910 205
  Australia Toll-Free(2)    : +61 1800 791 104
  Australia                 : +61 2 8311 0237
  Singapore Toll-Free       : +65 1800 622 3606
  Singapore                 : +65 3158 2702

  International: https://chime.aws/dialinnumbers/
";

            Clipboard.SetText(fullChimeText);
          }
         );
        }
      }
      public ICommand SettingsMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            SettingsWindow.CreateAndShow();
          }
         );
        }
      }

      public ICommand CheckNowMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            ChimeHelperState.Instance.CheckForChimeMeetingsAsync();
          }
         );
        }
      }

      public ICommand AboutMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            AboutWindow.CreateAndShow();
          }
         );
        }
      }

      public ICommand ExitMenuCommand
      {
        get
        {
          return new DelegateCommand(
          (object parameter) =>
          {
            App.Current.Shutdown();
          }
         );
        }
      }

    }

    public static readonly ChimeMeetingMenuItems<ChimeMeetingMenuItem> NO_MEETINGS =
      new ChimeMeetingMenuItems<ChimeMeetingMenuItem>(
        iconURI: NO_MEETINGS_ICON,
        tooltip: "Chime Helper: No Meetings Found"
      );

    public static readonly ChimeMeetingMenuItems<ChimeMeetingMenuItem> MEETINGS_LOADING =
      new ChimeMeetingMenuItems<ChimeMeetingMenuItem>(
        iconURI: LOADING_ICON,
        tooltip: "Chime Helper: Loading..."
      );
    
    public ChimeHelperTray(TaskbarIcon trayIcon)
    {
      _tray = trayIcon;
      _tray.DataContext = MEETINGS_LOADING;
    }

  }
}
