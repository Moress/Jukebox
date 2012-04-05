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
using System.Collections.ObjectModel;
using Jukebox.Client2.JukeboxService;
using System.ComponentModel;

namespace Jukebox.Client2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Найденные песни.
        /// </summary>
        ObservableCollection<Track> _foundTracks = new ObservableCollection<Track>();

        /// <summary>
        /// Найденные песни.
        /// </summary>
        public ObservableCollection<Track> FoundTracks
        {
            get
            {
                return _foundTracks;
            }
            set
            {
                _foundTracks = value;
                OnPropertyChanged("FoundTracks");
            }
        }

        /// <summary>
        /// Песни в списке воспроизведения.
        /// </summary>
        ObservableCollection<Track> _tracksInPlaylist = new ObservableCollection<Track>();

        public ObservableCollection<Track> TracksInPlaylist
        {
            get
            {
                return _tracksInPlaylist;
            }
            set
            {
                _tracksInPlaylist = value;
                OnPropertyChanged("TracksInPlaylist");
            }
        }

        public void Test()
        {
           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
