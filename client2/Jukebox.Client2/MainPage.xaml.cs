using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ListBoxDragReorder;
using Telerik.Windows.Controls.DragDrop;
using Telerik.Windows;

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
               SearchResultsControl1.QueryTextBox.Text = initParams["InitSong"];

            ServiceManager.ChannelsWereChanged += new EventHandler(OnChannelsWereChanged);
            ServiceManager.RecreateAllChannels();

            _model.Sources = TrackSourceComboItem.GetList();
           
            SearchResultsControl1.QueryTextBox.KeyDown += new KeyEventHandler(QueryTextBox_KeyDown);

            SearchResultsControl1.DataContext = _model;
            PlaylistControl1.DataContext = _model;
            PlaylistInfoControl1.DataContext = _model;
            NowPlayingControl1.DataContext = _model;

            _mainTimer.Tick += new EventHandler(OnMainTimerTick);
            _mainTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            _mainTimer.Start();

            // Сразу запускаем
            OnMainTimerTick(null, null);

            NowPlayingControl1.RefreshButton1.RefreshButtonClick += new EventHandler(RefreshButton1_RefreshButtonClick);
            NowPlayingControl1.NextButton1.NextButtonClick += new EventHandler(NextButton1_NextButtonClick);
            NowPlayingControl1.PlayAndPauseButton1.PlayOrPauseButtonClick += new EventHandler(PlayAndPauseButton1_PlayOrPauseButtonClick);
            NowPlayingControl1.ShuffleButton1.ShuffleButtonClick += new EventHandler(ShuffleButton1_ShuffleButtonClick);
            NowPlayingControl1.VolumeSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(VolumeSlider_ValueChanged);


            PlaylistControl1.PlaylistListBox.AddHandler(RadDragAndDropManager.DropQueryEvent, new EventHandler<DragDropQueryEventArgs>(OnDropQuery));
            PlaylistControl1.PlaylistListBox.AddHandler(RadDragAndDropManager.DragQueryEvent, new EventHandler<DragDropQueryEventArgs>(OnDragQuery));
            PlaylistControl1.PlaylistListBox.AddHandler(RadDragAndDropManager.DropInfoEvent, new EventHandler<DragDropEventArgs>(OnDropInfo));
            PlaylistControl1.PlaylistListBox.AddHandler(RadDragAndDropManager.DragInfoEvent, new EventHandler<DragDropEventArgs>(OnDragInfo));
        }

        void ShuffleButton1_ShuffleButtonClick(object sender, EventArgs e)
        {
            var playlistService = ServiceManager.GetPlaylistServiceClient();
            playlistService.ShuffleAsync();
        }

        void PlayAndPauseButton1_PlayOrPauseButtonClick(object sender, EventArgs e)
        {
            var playerService = ServiceManager.GetPlayerServiceClient();
            playerService.PlayOrPauseAsync();
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
            if (List1.Count != List2.Count)
            {
                _model.TracksInPlaylist = e.Result.Tracks;
            }
            else
            {
                for (int i = 0; i < List1.Count; i++)
                {
                    if ((List1[i].Id != List2[i].Id) ||
                        (List1[i].State != List2[i].State))
                    {
                        _model.TracksInPlaylist = e.Result.Tracks;
                        break;
                    }
                }
            }

            _model.TrackCount = _model.TracksInPlaylist.Count;

            TimeSpan totalDuration = new TimeSpan(0, 0, 0);
            foreach (Track t in _model.TracksInPlaylist)
            {
                totalDuration += t.Duration;
            }

            if (_model.CurrentTrack != null)
            {
                _model.TrackCount++;
                Track currentTrack = _model.CurrentTrack;
                totalDuration += (currentTrack.Duration - currentTrack.PlayPosition);
            }

            _model.TotalDuration = totalDuration.ToString(@"hh\:mm\:ss");
        }

        void OnMainTimerTick(object sender, EventArgs e)
        {
            var playlistService = ServiceManager.GetPlaylistServiceClient();
            playlistService.GetPlaylistAsync();
            var playerService = ServiceManager.GetPlayerServiceClient();
            playerService.GetCurrentTrackAsync();
            playerService.GetVolumeLevelAsync();
            var userService = ServiceManager.GetUserServiceClient();
            userService.GetUserAsync();
        }

        private void QueryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _model.FoundTracks = null;

                var busyIndicator = searchBusyIndicator;
                busyIndicator.BusyContent = "Поиск...";
                _model.FoundTracks = new SearchResultPagedView() {
                    Query = SearchResultsControl1.QueryTextBox.Text,
                    BusyIndicator = busyIndicator,
                    Sources = _model.Sources,
                    PageSize = 100};
                _model.FoundTracks.CallSearch();
            }
        }



        void OnChannelsWereChanged(object sender, EventArgs e)
        {
            var playlistService = ServiceManager.GetPlaylistServiceClient();
            playlistService.GetPlaylistCompleted += new EventHandler<GetPlaylistCompletedEventArgs>(OnGetPlaylistCompleted);
            playlistService.AddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(OnAddCompleted);
            playlistService.NextCompleted += new EventHandler<NextCompletedEventArgs>(OnNextCompleted);
            playlistService.ShuffleCompleted += new EventHandler<ShuffleCompletedEventArgs>(OnShuffleCompleted);

            var playerService = ServiceManager.GetPlayerServiceClient();
            playerService.GetCurrentTrackCompleted += new EventHandler<GetCurrentTrackCompletedEventArgs>(OnGetCurrentTrackCompleted);
            playerService.GetVolumeLevelCompleted += new EventHandler<GetVolumeLevelCompletedEventArgs>(OnGetVolumeLevelCompleted);
            playerService.SetVolumeLevelCompleted += new EventHandler<SetVolumeLevelCompletedEventArgs>(OnSetVolumeLevelCompleted);
            playerService.PlayOrPauseCompleted += new EventHandler<PlayOrPauseCompletedEventArgs>(OnPlayOrPauseCompleted);

            var userService = ServiceManager.GetUserServiceClient();
            userService.GetUserCompleted += new EventHandler<GetUserCompletedEventArgs>(OnGetUserCompleted);
        }
        void OnShuffleCompleted(object sender, ShuffleCompletedEventArgs e)
        {
            if (e.Result != "")
            {
                MessageBox.Show(e.Result);
            }
        }

        void OnSetVolumeLevelCompleted(object sender, SetVolumeLevelCompletedEventArgs e)
        {
            if (e.Result != "")
            {
                MessageBox.Show(e.Result);
            }
        }

        void OnGetUserCompleted(object sender, GetUserCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _model.UserActionPoints = e.Result.ActionPoints;
            }
        }

        void OnNextCompleted(object sender, NextCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
        }

        void OnPlayOrPauseCompleted(object sender, PlayOrPauseCompletedEventArgs e)
        {
            if (e.Result != "")
            {
                MessageBox.Show(e.Result);
            }
        }

        void OnGetCurrentTrackCompleted(object sender, GetCurrentTrackCompletedEventArgs e)
        {
            if (_model.CurrentTrack == null || _model.CurrentTrack != e.Result)
            {
                _model.CurrentTrack = e.Result;
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
        

        private void OnDragInfo(object sender, DragDropEventArgs e)
		{
			if (e.Options.Status == DragStatus.DragComplete)
			{
				var listBox = e.Options.Source.FindItemsConrolParent() as ListBox;

				var itemsSource = listBox.ItemsSource as IList<Track>;
				var operation = e.Options.Payload as DragDropOperation;

				itemsSource.Remove(operation.Payload as Track);
			}
		}

		private void OnDropInfo(object sender, DragDropEventArgs e)
		{
			if (e.Options.Status == DragStatus.DropPossible)
			{
				var listBox = e.Options.Destination.FindItemsConrolParent() as ListBox;
				VisualStateManager.GoToState(listBox, "DropPossible", false);
				var destination = e.Options.Destination;

				//Get the DropCueElemet:
				var dropCueElement = (VisualTreeHelper.GetChild(listBox, 0) as FrameworkElement).FindName("DropCueElement") as FrameworkElement;

				var operation = e.Options.Payload as DragDropOperation;

				//Get the parent of the destination:
				var visParent = VisualTreeHelper.GetParent(destination) as UIElement;

				//Get the spacial relation between the destination and its parent:
				var destinationStackTopLeft = destination.TransformToVisual(visParent).Transform(new Point());

				var yTranslateValue = operation.DropPosition == DropPosition.Before ? destinationStackTopLeft.Y : destinationStackTopLeft.Y + destination.ActualHeight;

				dropCueElement.RenderTransform = new TranslateTransform() { Y = yTranslateValue };
			}

			//Hide the DropCue:
			if (e.Options.Status == DragStatus.DropImpossible || e.Options.Status == DragStatus.DropCancel || e.Options.Status == DragStatus.DropComplete)
			{
				var listBox = e.Options.Destination.FindItemsConrolParent() as ListBox;
				VisualStateManager.GoToState(listBox, "DropImpossible", false);
			}

			//Place the item:
			if (e.Options.Status == DragStatus.DropComplete)
			{
				var listBox = e.Options.Destination.FindItemsConrolParent() as ListBox;

				var itemsSource = listBox.ItemsSource as IList<Track>;
				var destinationIndex = itemsSource.IndexOf(e.Options.Destination.DataContext as Track);

				var operation = e.Options.Payload as DragDropOperation;
				var insertIndex = operation.DropPosition == DropPosition.Before ? destinationIndex : destinationIndex + 1;

				itemsSource.Insert(insertIndex, operation.Payload as Track);

				listBox.Dispatcher.BeginInvoke(() =>
				{
					listBox.SelectedIndex = insertIndex;
				});

                UpdatePlaylist();
			}
		}

        public void UpdatePlaylist()
        {
            var playlistService = ServiceManager.GetPlaylistServiceClient();
            Playlist playlist = new Playlist();
            playlist.Tracks = new ObservableCollection<Track>();
            foreach (Track t in PlaylistControl1.PlaylistListBox.ItemsSource)
            {
                playlist.Tracks.Add(t);
            }
            playlistService.SetPlaylistAsync(playlist);
        }

		private void OnDropQuery(object sender, DragDropQueryEventArgs e)
		{
			if (e.Options.Status == DragStatus.DropDestinationQuery)
			{
				var destination = e.Options.Destination;
				var listBox = destination.FindItemsConrolParent() as ListBox;

				//Cannot place na item relative to itself:
				if (e.Options.Source == e.Options.Destination)
				{
					return;
				}

				VisualStateManager.GoToState(listBox, "DropPossible", false);

				//Get the spacial relation between the destination item and the vis. root:
				var destinationTopLeft = destination.TransformToVisual(null).Transform(new Point());

				//Should the new Item be moved before or after the destination item?:
				bool placeBefore = (e.Options.CurrentDragPoint.Y - destinationTopLeft.Y) < destination.ActualHeight / 2;

				var operation = e.Options.Payload as DragDropOperation;

				operation.DropPosition = placeBefore ? DropPosition.Before : DropPosition.After;

				e.QueryResult = true;
				e.Handled = true;
			}
		}

		private void OnDragQuery(object sender, DragDropQueryEventArgs e)
		{
			if (e.Options.Status == DragStatus.DragQuery)
			{
				e.QueryResult = true;
				e.Handled = true;

				var sourceControl = e.Options.Source;

				var dragCue = RadDragAndDropManager.GenerateVisualCue(sourceControl);
				dragCue.HorizontalAlignment = HorizontalAlignment.Left;
				dragCue.Content = sourceControl.DataContext;
                dragCue.ContentTemplate = App.Current.Resources["TrackDragCueTemplate"] as DataTemplate;
                dragCue.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
				e.Options.DragCue = dragCue;

				e.Options.Payload = new DragDropOperation() { Payload = sourceControl.DataContext };
			}

			if (e.Options.Status == DragStatus.DropSourceQuery)
			{
				e.QueryResult = true;
				e.Handled = true;
			}
		}
    }
}
