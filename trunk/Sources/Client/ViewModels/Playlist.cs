
namespace Jukebox.Client.ViewModels {
	using System.Collections.ObjectModel;
	using Jukebox.Client.JukeboxService;
	using System.Linq;

	public class Playlist : ViewModel {
		public Playlist() {
			pPlaylist = new JukeboxService.Playlist();
			pPlaylist.Tracks = new ObservableCollection<Track>();
		}

		public void UpdatePlaylist() {
			ServiceManager.PlaylistService.GetPlaylistCompleted += PlaylistService_GetPlaylistCompleted;
			ServiceManager.PlaylistService.OnTrackAddedReceived += PlaylistService_OnTrackAddedReceived;
			ServiceManager.PlaylistService.OnTrackRemovedReceived += PlaylistService_OnTrackRemovedReceived;
			ServiceManager.PlaylistService.GetPlaylistAsync();
		}

		void PlaylistService_GetPlaylistCompleted(object sender, GetPlaylistCompletedEventArgs e) {
			pPlaylist = e.Result;
			OnPropertyChanged("Tracks");
		}

		void PlaylistService_OnTrackRemovedReceived(object sender, OnTrackRemovedReceivedEventArgs e) {
			var track = pPlaylist.Tracks.FirstOrDefault(x => x.Id == e.track.Id);
			if (track != null) {
				pPlaylist.Tracks.Remove(track);
				OnPropertyChanged("Tracks");
			}
		}

		void PlaylistService_OnTrackAddedReceived(object sender, OnTrackAddedReceivedEventArgs e) {
			pPlaylist.Tracks.Add(e.track);
			OnPropertyChanged("Tracks");
		}

		public ObservableCollection<Track> Tracks { get { return pPlaylist.Tracks; } }

		private JukeboxService.Playlist pPlaylist;
	}
}
