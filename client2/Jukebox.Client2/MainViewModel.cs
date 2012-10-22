using System;
using System.Collections.Generic;
using System.Collections;
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
using System.ServiceModel;
using Jukebox.Client2.Misc;


namespace Jukebox.Client2
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /*/// <summary>
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
        }*/

        SearchResultPagedView _foundTracks;

        public SearchResultPagedView FoundTracks
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

        private int _trackCount;

        /// <summary>
        /// Количество песен в списке воспроизведения
        /// </summary>
        public int TrackCount
        {
            get
            {
                
                return _trackCount;
            }
            set
            {
                _trackCount = value;
                OnPropertyChanged("TrackCount");
            }
        }

        private string _totalDuration;

        /// <summary>
        /// Общая продолжительность (строкой)
        /// </summary>
        public string TotalDuration
        {
            get
            {
                return _totalDuration;
            }
            set
            {
                _totalDuration = value;
                OnPropertyChanged("TotalDuration");
            }
        }

        private ObservableCollection<TrackSourceComboItem> _sources;
        /// <summary>
        /// Источники поиска
        /// </summary>
        public ObservableCollection<TrackSourceComboItem> Sources
        {
            get
            {
                ObservableCollection<TrackSourceComboItem> cachedSources = IsolatedStorageManager.GetValueByKey("sources") as ObservableCollection<TrackSourceComboItem>;
                if (cachedSources != null)
                {
                    _sources = cachedSources;
                }
                return _sources;
            }
            set
            {
                _sources = value;
                IsolatedStorageManager.SetKeyValue("sources", _sources);
                OnPropertyChanged("Sources");
            }
        }

        public void UpdateSources()
        {
            Sources = _sources;
        }

        private int _userActionPoints;
        /// <summary>
        /// Очки действий
        /// </summary>
        public int UserActionPoints
        {
            get
            {
                return _userActionPoints;
            }
            set
            {
                _userActionPoints = value;
                OnPropertyChanged("UserActionPoints");
            }
        }
        
        private Track _currentTrack;

        public Track CurrentTrack
        {
            get
            {
                return _currentTrack;
            }
            set
            {
                _currentTrack = value;
                OnPropertyChanged("CurrentTrack");
            }
        }

        private bool _playerPanelIsExpanded;

        public bool PlayerPanelIsExpanded
        {
            get
            {
                try
                {
                    string isExpanded = IsolatedStorageManager.GetValueByKey("playerPanelIsExpanded") as string;
                    if (isExpanded != null)
                    {
                        return bool.Parse(isExpanded);
                    }
                    return _playerPanelIsExpanded;
                }
                catch
                {
                    return _playerPanelIsExpanded;
                }
            }
            set
            {
                IsolatedStorageManager.SetKeyValue("playerPanelIsExpanded", value.ToString());
                _playerPanelIsExpanded = value;
                OnPropertyChanged("PlayerPanelIsExpanded");
            }
        }

        public MainViewModel()
        {
            _instance = this;
        }

        private static MainViewModel _instance;

        public static MainViewModel Instance
        {
            get
            {
                return _instance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
  
    }
}
