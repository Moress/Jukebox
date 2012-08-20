using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Jukebox.Client2.JukeboxService;
using System.Collections.ObjectModel;

namespace Jukebox.Client2
{
    public partial class PlaylistControl : UserControl
    {
        public PlaylistControl()
        {
            InitializeComponent();
        }

        private void UpInPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistListBox.SelectedItem != null)
            {
                Track selectedTrack = PlaylistListBox.SelectedItem as Track;
                var itemsSource = PlaylistListBox.ItemsSource as IList<Track>;
                itemsSource.Remove(selectedTrack);
                itemsSource.Insert(0, selectedTrack);
                ((MainPage)App.Current.RootVisual).UpdatePlaylist();
            }
        }
    }
}
