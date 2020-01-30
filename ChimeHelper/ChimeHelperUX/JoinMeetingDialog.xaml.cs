using ChimeHelper;
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
  /// Interaction logic for JoinMeeting.xaml
  /// </summary>
  public partial class JoinMeetingDialog : Window
  {

    #region Singleton
    public static JoinMeetingDialog _instance;

    // Not using "Instance" since this Singleton is more of an action
    public static void CreateAndShow()
    {
      if (_instance == null || _instance.IsLoaded == false)
        _instance = new JoinMeetingDialog();

      _instance.Show();
      _instance.Activate();
      
      WindowPositioner.MoveToMouse(_instance, leftOffset: 50, topOffset: 50);

      _instance.GetPinsFromClipboard();
    }

    #endregion

    public JoinMeetingDialog()
    {
      InitializeComponent();
    }

    private void GetPinsFromClipboard()
    {
      var clipboard = Clipboard.GetText();
      var pins = Chime.GetPinsFromText(clipboard);

      if (pins.Count > 0)
        txtPin.Text = pins[0];
    }

    private void btnJoin_Click(object sender, RoutedEventArgs e)
    {

      var pins = Chime.GetPinsFromText(txtPin.Text);

      // unlikely to be multiple pins, just take the first one
      if (pins.Count > 0)
      {
        Chime.Join(pins[0]);
      } 
      else if (!string.IsNullOrEmpty(txtPin.Text))
      {
        // if there are no extracted pins, but there is text then just use that text
        Chime.Join(txtPin.Text);
      }

      Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    // enter on Button are swallowed by the actual button
    private void Default_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter || e.Key == Key.Return)
      {
        btnJoin_Click(null, null);
      }
    }
  }
}
