using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ChimeHelperUX.UXHelpers
{
  static class AncestorHelper
  {
    // Modified from: https://faithlife.codes/blog/2008/02/finding_ancestor_elements_in_w/

    public static DependencyObject GetParent(DependencyObject obj)
    {
      if (obj == null)
        return null;

      if (obj is ContentElement ce)
      {
        DependencyObject parent = ContentOperations.GetParent(ce);
        
        if (parent != null)
          return parent;


        return ce is FrameworkContentElement fce ? fce.Parent : null;
      }
      return VisualTreeHelper.GetParent(obj);
    }

    public static T FindAncestor<T>(DependencyObject obj)
      where T : DependencyObject
    {
      // always find a parent (or null)
      obj = GetParent(obj);

      while (obj != null)
      {
        if (obj is T objTest)
          return objTest;

        obj = GetParent(obj);
      }

      return null;
    }
  }
}
