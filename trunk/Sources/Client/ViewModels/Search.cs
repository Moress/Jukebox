
namespace Jukebox.Client.ViewModels {
	using System.Collections.ObjectModel;
	using Jukebox.Client.JukeboxService;

	public class Search : ViewModel {
		public void Find(string query) {
			ServiceManager.SearchService.SearchCompleted += OnSearchCompleted;
			ServiceManager.SearchService.SearchAsync(query);
		}

		public void AddTrack(Track track) {
			ServiceManager.PlaylistService.AddAsync(track);
		}

		private void OnSearchCompleted(object sender, SearchCompletedEventArgs e) {
			if (e.Error != null) throw e.Error;

			SearchResults = e.Result;
			OnPropertyChanged("SearchResults");
		}

		public ObservableCollection<Track> SearchResults { get; set; }
	}
}
