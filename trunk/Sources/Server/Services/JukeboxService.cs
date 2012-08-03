
namespace Jukebox.Server.Services {
	using System.Collections.Generic;
	using System.Collections.Specialized;
    using System.Collections.ObjectModel;
	using System.IO;
    using System;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using Jukebox.Server.DataProviders;
	using Jukebox.Server.Models;
	using System.Diagnostics;
    using System.ServiceModel.Channels;

	[ServiceBehavior(
		InstanceContextMode = InstanceContextMode.Single,
		ConcurrencyMode = ConcurrencyMode.Multiple,
        MaxItemsInObjectGraph = int.MaxValue)]
	class JukeboxService : IPolicyService, IPlaylistService, ISearchService, IPlayerService, IUserService {
		public JukeboxService() {
			Player.Instance.TrackChanged += OnCurrentTrackChanged;
			Player.Instance.Playlist.Tracks.CollectionChanged += OnPlaylistChanged;
			Player.Instance.TrackStateChanged += new System.EventHandler<PlayerEventArgs>(OnTrackStateChanged);
		}
		
		// IPolicyService --------------------------------------------------------------------------

		public Stream GetSilverlightPolicy() {
			Debug.Print("Policy has been sent.");
			if (InstanceContext == null) InstanceContext = OperationContext.Current.InstanceContext;
			
			WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
			return new MemoryStream(File.ReadAllBytes("clientaccesspolicy.xml"));
		}

		// ISearchService --------------------------------------------------------------------------

		public SearchResult Search(string query, List<TrackSourceComboItem> sources, int pageIndex, int pageSize) {
            return DataProviderManager.Instance.Search(query, sources, pageIndex, pageSize);
        }

		// IPlaylistService ------------------------------------------------------------------------

		public Playlist GetPlaylist() {
			return Player.Instance.Playlist;
		}

        public void SetPlaylist(Playlist playlist) {
            foreach (Track t1 in Player.Instance.Playlist.Tracks)
            {
                for (int i = 0; i < playlist.Tracks.Count; i ++)
                {
                    Track t2 = playlist.Tracks[i];
                    if (t1.Id == t2.Id)
                    {
                        playlist.Tracks[i] = t1;
                    }
                }
            }
            Player.Instance.Playlist = playlist;
        }

		public void Add(Track track) {
            string userAddress = GetUserAddress();
			Player.Instance.Playlist.Tracks.Add(track);
            UserManager.Instance.AddActionPoints(userAddress, 1);
		}

		public void Remove(Track track) {
			Player.Instance.Playlist.Tracks.Remove(track);
		}

        /// <summary>
        /// Голоса за пропуск этой песни.
        /// </summary>
        private Dictionary<string, bool> _nextVotes = new Dictionary<string,bool>();

        public string Next()
        {
            string clientId = GetUserAddress();


            if (Player.Instance.CurrentTrack == null)
            {
                return "Сейчас не проигрывается ни одна песня.";
            }

            if (_nextVotes.ContainsKey(clientId))
            {
                return "Вы уже голосовали против этой песни.";
            }

            _nextVotes[clientId] = true;

            int votes = _nextVotes.Count;

            if (votes == Config.GetInstance().VotesToSkip)
            {
                Player.Instance.Abort();
            }

            return string.Format("Проголосовало {0} из {1}.", votes, Config.GetInstance().VotesToSkip);
        }

        public string Shuffle()
        {
            string userAddress = GetUserAddress();

            if (Player.Instance.Playlist.Tracks.Count == 0)
            {
                return "Плейлист пуст.";
            }

            if (!UserManager.Instance.CanUserPerformAction(userAddress))
            {
                return string.Format("Вы исчерпали лимит действий. Восстановление через: {0}.", UserManager.Instance.GetTimeForNextRestore().ToString(@"hh\:mm\:ss"));
            }

            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            foreach (Track track in Player.Instance.Playlist.Tracks.OrderBy(x => Guid.NewGuid()))
            {
                tracks.Add(track);
            }
            Player.Instance.Playlist.Tracks = tracks;
            UserManager.Instance.UserPerformAction(userAddress, ActionType.ShuffleAction);

            return "";
        }

		// IPlayerService --------------------------------------------------------------------------

		public Track GetCurrentTrack() {
			return Player.Instance.CurrentTrack;
		}

        public double GetVolumeLevel()
        {
            return Player.Instance.VolumeLevel;
        }

        public string SetVolumeLevel(double value)
        {
            string userAddress = GetUserAddress();
            if (!UserManager.Instance.CanUserPerformAction(userAddress))
            {
                return "";
            }

            UserManager.Instance.UserChangeValueAction(userAddress, ActionType.VolumeChangeAction, Player.Instance.VolumeLevel, value);

            Player.Instance.VolumeLevel = value;
            return "";
        }

        public string PlayOrPause()
        {
            string userAddress = GetUserAddress();

            if (Player.Instance.CurrentTrack == null)
            {
                return "Сейчас не проигрывается ни одна песня.";
            }

            if (!UserManager.Instance.CanUserPerformAction(userAddress))
            {
                return string.Format("Вы исчерпали лимит действий. Восстановление через: {0}.", UserManager.Instance.GetTimeForNextRestore().ToString(@"hh\:mm\:ss"));
            }

            if (!Player.Instance.CurrentISound.Paused)
            {
                Player.Instance.CurrentISound.Paused = true;
                UserManager.Instance.UserPerformAction(userAddress, ActionType.PauseOrPlayAction);
            }
            else
            {
                Player.Instance.CurrentISound.Paused = false;
                UserManager.Instance.UserPerformAction(userAddress, ActionType.PauseOrPlayAction);
            }

            return "";
        }

        // IUserService --------------------------------------------------------------------------
        public User GetUser()
        {
            return UserManager.Instance.GetUserInfo(GetUserAddress());
        }

		private void OnCurrentTrackChanged(object sender, PlayerEventArgs e) {
            _nextVotes.Clear();
			/*foreach (IPlayerServiceCallback a in InstanceContext.IncomingChannels.Where(x => x is IPlayerServiceCallback)) {
				a.OnCurrentTrackChanged(e.Track);
			}*/
		}

		private void OnPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e) {
			/*foreach (IPlaylistServiceCallback a in InstanceContext.IncomingChannels.Where(x => x is IPlaylistServiceCallback)) {
				if (e.NewItems != null) { foreach (Track track in e.NewItems) { a.OnTrackAdded(track); } }
				if (e.OldItems != null) { foreach (Track track in e.OldItems) { a.OnTrackRemoved(track); } }
			}*/
		}

		private void OnTrackStateChanged(object sender, PlayerEventArgs e) {
			/*foreach (IPlaylistServiceCallback a in InstanceContext.IncomingChannels.Where(x => x is IPlaylistServiceCallback)) {
				a.OnTrackStateChanged(e.Track);
			}*/
		}

        private string GetUserAddress()
        {
            OperationContext context = OperationContext.Current;
            MessageProperties prop = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            string clientId = endpoint.Address;

            return clientId;
        }

		private InstanceContext InstanceContext { get; set; }
    }
}
