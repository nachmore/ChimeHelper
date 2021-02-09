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
  public partial class SettingsWindow : Window
  {

    #region Singleton
    public static SettingsWindow _settingsWindow;

    // Not using "Instance" since this Singleton is more of an action
    public static void CreateAndShow()
    {
      if (_settingsWindow == null || _settingsWindow.IsLoaded == false)
        _settingsWindow = new SettingsWindow();

      _settingsWindow.Show();
      _settingsWindow.Activate();

      WindowPositioner.MoveToMouse(_settingsWindow);
    }
    #endregion

    public string PersonalId { get; set; }
    public string PersonalizedId { get; set; }
    public bool NotifyOnMeetingStart { get; set; }

    private SettingsWindow()
    {
      InitializeComponent();

      LoadSettings();

      PersonalId = Properties.Settings.Default.ChimeBridgePersonalID;
      PersonalizedId = Properties.Settings.Default.ChimeBridgePersonalizedID;
      NotifyOnMeetingStart = Properties.Settings.Default.NotifyOnMeetingStart;

      DataContext = this;
    }

    public static void LoadSettings()
    {
      if (Properties.Settings.Default.UpgradeRequired)
      {
        Properties.Settings.Default.Upgrade();
        
        Properties.Settings.Default.UpgradeRequired = false;
        Properties.Settings.Default.Save();
      }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      Properties.Settings.Default.ChimeBridgePersonalID = PersonalId?.Replace(" ", "");
      Properties.Settings.Default.ChimeBridgePersonalizedID = PersonalizedId;
      Properties.Settings.Default.NotifyOnMeetingStart = NotifyOnMeetingStart;

      Properties.Settings.Default.Save();
      Properties.Settings.Default.Reload();

      Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
