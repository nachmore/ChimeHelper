using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChimeHelperUX.UXHelpers
{
  static class WindowPositioner
  {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
      public Int32 X;
      public Int32 Y;
    };
    public static Point GetMousePosition()
    {
      var w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);

      return new Point(w32Mouse.X, w32Mouse.Y);
    }

    public static void MoveToMouse(Window target, int leftOffset = 0, int topOffset = 0, int offscreenBuffer = 20)
    {
      var mouse = GetMousePosition();

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
  }
}
