
namespace Jukebox.Server.Models {
	using System.Collections.ObjectModel;
	using System.Runtime.Serialization;

	[DataContract]
	class Playlist {
		public Playlist() {
			Tracks = new ObservableCollection<Track>();
		}

		[DataMember]
		public string Theme { get; set; }

		[DataMember]
		public ObservableCollection<Track> Tracks { get; set; }
	}
}
