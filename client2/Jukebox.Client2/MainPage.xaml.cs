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
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Windows.Threading;
using System.Windows.Interop;

namespace Jukebox.Client2
{
    public partial class MainPage : UserControl
    {
        /// <summary>
        /// Основная модель данных.
        /// </summary>
        MainViewModel _model = new MainViewModel();

        /// <summary>
        /// Таймер для получения изменений с сервера.
        /// </summary>
        DispatcherTimer _mainTimer = new DispatcherTimer();

        int counter;

        public MainPage()
        {
            InitializeComponent();
            var host = Application.Current.Host;
            var initParams = host.InitParams;

            if (initParams.Keys.Contains("ServerHost"))
                ServiceManager.Host = initParams["ServerHost"];
            else
                ServiceManager.Host = "net.tcp://localhost:4502/";

            if (initParams.Keys.Contains("InitSong"))
                QueryTextBox.Text = initParams["InitSong"];

            ServiceManager.ChannelsWereChanged += new EventHandler(OnChannelsWereChanged);
            ServiceManager.RecreateAllChannels();

            SearchResultsControl1.DataContext = _model;
            PlaylistControl1.DataContext = _model;

            _mainTimer.Tick += new EventHandler(OnMainTimerTick);
            _mainTimer.Interval = new TimeSpan(0, 0, 0,0,100);
            _mainTimer.Start();

            // Сразу запускаем
            OnMainTimerTick(null, null);

            NowPlayingControl1.RefreshButton1.RefreshButtonClick += new EventHandler(RefreshButton1_RefreshButtonClick);
            NowPlayingControl1.NextButton1.NextButtonClick += new EventHandler(NextButton1_NextButtonClick);
            NowPlayingControl1.VolumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(VolumeSlider_ValueChanged);
        }

        void NextButton1_NextButtonClick(object sender, EventArgs e)
        {
            var playlistService = ServiceManager.GetPlaylistServiceClient();
            playlistService.NextAsync();
        }

        void RefreshButton1_RefreshButtonClick(object sender, EventArgs e)
        {
            OnMainTimerTick(null, null);
        }

        void OnAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Обновляем список
            OnMainTimerTick(this, null);
            var host = Application.Current.Host;
        }

        void OnGetPlaylistCompleted(object sender, GetPlaylistCompletedEventArgs e)
        {
            var List1 = _model.TracksInPlaylist;
            var List2 = e.Result.Tracks;
            var Count = List1.Where(x => !List2.Any(x1 => x1.Id == x.Id && x1.State == x.State))
            .Union(List2.Where(x => !List1.Any(x1 => x1.Id == x.Id && x1.State == x.State))).Count();
            if (Count != 0)
            {
                _model.TracksInPlaylist = e.Result.Tracks;
            }
        }

        void OnMainTimerTick(object sender, EventArgs e)
        {
            var playlistService = ServiceManager.GetPlaylistServiceClient();
            playlistService.GetPlaylistAsync();
            var playerService = ServiceManager.GetPlayerServiceClient();
            playerService.GetCurrentTrackAsync();
            playerService.GetVolumeLevelAsync();
        }

        private void QueryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _model.FoundTracks = null;
                QueryTextBox.IsEnabled = false;

                var service = ServiceManager.GetSearchServiceClient();
                service.SearchAsync(QueryTextBox.Text);
            }
        }

        void OnSearchCompleted(object sender, SearchCompletedEventArgs e)
        {
            try
            {
                _model.FoundTracks = e.Result;
                QueryTextBox.SelectAll();
            }
            finally
            {
                QueryTextBox.IsEnabled = true;
            }
        }

        void OnChannelsWereChanged(object sender, EventArgs e)
        {
            var searchService = ServiceManager.GetSearchServiceClient();
            searchService.SearchCompleted += new EventHandler<SearchCompletedEventArgs>(OnSearchCompleted);

            var playlistService = ServiceManager.GetPlaylistServiceClient();
            playlistService.GetPlaylistCompleted += new EventHandler<GetPlaylistCompletedEventArgs>(OnGetPlaylistCompleted);
            playlistService.AddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(OnAddCompleted);
            playlistService.NextCompleted += new EventHandler<NextCompletedEventArgs>(OnNextCompleted);

            var playerService = ServiceManager.GetPlayerServiceClient();
            playerService.GetCurrentTrackCompleted += new EventHandler<GetCurrentTrackCompletedEventArgs>(OnGetCurrentTrackCompleted);
            playerService.GetVolumeLevelCompleted += new EventHandler<GetVolumeLevelCompletedEventArgs>(OnGetVolumeLevelCompleted);           
        }

        void OnNextCompleted(object sender, NextCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
        }

        void OnGetCurrentTrackCompleted(object sender, GetCurrentTrackCompletedEventArgs e)
        {
            if (NowPlayingControl1.DataContext != e.Result)
            {
                NowPlayingControl1.DataContext = e.Result;
            }
        }

        void OnGetVolumeLevelCompleted(object sender, GetVolumeLevelCompletedEventArgs e)
        {
            NowPlayingControl1.VolumeSlider.Value = e.Result;
            NowPlayingControl1.VolumeLevel.Text = (int)(e.Result * 100) + "%";
        }

        void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var playerService = ServiceManager.GetPlayerServiceClient();
            playerService.SetVolumeLevelAsync(e.NewValue);
        }
    }
}
