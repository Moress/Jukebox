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
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Jukebox.Client2.JukeboxService
{
    public partial class TrackSourceComboItem
    {
        public static ObservableCollection<TrackSourceComboItem> GetList()
        {
            ObservableCollection<TrackSourceComboItem> result = new ObservableCollection<TrackSourceComboItem>();

            Type enumType = typeof(TrackSource);

            var fields = from field in enumType.GetFields()
                         where field.IsLiteral
                         select field;


            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(enumType);
                result.Add(new TrackSourceComboItem()
                {
                    Source = (TrackSource)value,
                    IsSelected = true
                });
            }

            return result;
        }
    }
}
