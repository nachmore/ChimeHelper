using System.Windows;
using System.Windows.Media;

namespace ChimeHelperUX.UXHelpers
{
  static class AncestorHelper
  {
    public static T FindAncestor<T>(DependencyObject obj)
      where T : DependencyObject
    {
      var popupRootFinder = VisualTreeHelper.GetParent((DependencyObject)obj);

      while (popupRootFinder != null)
      {
        var logicalRoot = LogicalTreeHelper.GetParent(popupRootFinder);

        if (logicalRoot is T t)
        {
          return t;
        }

        popupRootFinder = VisualTreeHelper.GetParent(popupRootFinder);
      }

      return null;
    }
  }
}
