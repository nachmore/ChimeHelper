using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChimeHelperUX.UXHelpers
{
  static class WindowPositioner
  {
    public static void MoveToMouse(Window target, int leftOffset = 0, int topOffset = 0, int offscreenBuffer = 20)
    {
      var transform = PresentationSource.FromVisual(target).CompositionTarget.TransformFromDevice;
      var mouse = transform.Transform(GetMousePosition(target));

      target.Left = mouse.X - target.ActualWidth + leftOffset;
      target.Top = mouse.Y - target.ActualHeight + topOffset;

      if (target.Left < SystemParameters.VirtualScreenLeft)
        target.Left = SystemParameters.VirtualScreenLeft + offscreenBuffer;

      if (target.Left + target.Width > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
        target.Left = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - target.Width - offscreenBuffer;

      if (target.Top < SystemParameters.VirtualScreenTop)
        target.Top = SystemParameters.VirtualScreenTop;

      if (target.Top + target.Height > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
        target.Top = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - target.Height - offscreenBuffer;
    }

    /// <summary>
    /// Retrieve the current mouse position as a System.Windows.Point (instead of the default System.Drawing.Point)
    /// </summary>
    /// <returns></returns>
    private static Point GetMousePosition(Control target)
    {
      var point = Mouse.GetPosition(target);
      point = target.PointToScreen(point);

      return new Point(point.X, point.Y);
    }


  }
}
