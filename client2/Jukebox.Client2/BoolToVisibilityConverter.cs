using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace Jukebox.Client2
{
    public class BoolToVisibilityConverter: IValueConverter
    {
          public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
          {
              if (parameter == null)
              {
                  if ((bool)value == true)
                      return Visibility.Visible;
                  else
                      return Visibility.Collapsed;
              }
              else
              {
                  if ((bool)value == true)
                      return Visibility.Collapsed;
                  else
                      return Visibility.Visible;
              }
          }
  
          public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
          {
              return null;
          }
    }
}
