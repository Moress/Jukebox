
namespace Jukebox.Server.DataProviders {
	using System.Collections.Generic;
	using Jukebox.Server.Models;

	interface IDataProvider {
		IList<Track> Search(string query);
		byte[] Download(Track track);
	}
}
