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
using Jukebox.Client2.JukeboxService;

namespace Jukebox.Client2
{
    public class TrackIsAddedToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? isAdded = value as bool?;
            if (isAdded == null || isAdded == true)
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class TrackSourceToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TrackSource? source = value as TrackSource?;
            if (source == null)
                return null;

            if (source == TrackSource.Cache)
                return new SolidColorBrush(Color.FromArgb(80, 130, 165, 177));
            if (source == TrackSource.VK)
                return new SolidColorBrush(Color.FromArgb(80, 50, 50, 200));

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class TrackStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var state = value as TrackState?;
            if (state == null)
                return null;

            if (state == TrackState.Failed)
                return new SolidColorBrush(Color.FromArgb(80, 219, 49, 0));
            if (state == TrackState.Downloading)
                return new SolidColorBrush(Color.FromArgb(80, 50, 200, 0));
            if (state == TrackState.Ready)
                return new SolidColorBrush(Color.FromArgb(80, 255, 255, 0));

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class TrackToStringTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var track = value as Track;
            if (value == null)
                return null;

            return string.Format("{0} ({1})", track.Title, track.Duration.ToString(@"mm\:ss"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
