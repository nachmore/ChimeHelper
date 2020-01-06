using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChimeHelper.XAMLExtenders
{
  /// <summary>
  /// Signature based on https://docs.microsoft.com/en-us/dotnet/api/microsoft.toolkit.uwp.ui.converters.booltoobjectconverter?view=win-comm-toolkit-dotnet-stable&viewFallbackFrom=uwp-toolkit-dotnet
  /// (no implementation code was looked at / copied)
  /// 
  /// Basic converter that takes a True and a False value and returns the correct one based on the input value
  /// </summary>
  class BoolToObjectConverter : IValueConverter
  {

    public object TrueValue { get; set; }
    public object FalseValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var boolVal = (bool)value;

      if (boolVal)
        return TrueValue;
      else
        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
