
namespace Jukebox.Server.DataProviders {
	using System.Collections.Generic;
	using Jukebox.Server.Models;
    using System.Linq;

	class DataProviderManager {
		public DataProviderManager() {
			Instance = this;
			DataProviders = new List<IDataProvider>();
		}

		public IList<Track> Search(string query) {
            List<Track> results = new List<Track>();
            foreach (var dataProvider in DataProviders)
            {
                results.AddRange(dataProvider.Search(query));
            }
            results = DeleteDuplicates(results);
            return results;
		}

		public byte[] Download(Track track) {
            if (track.Source == TrackSource.VK)
            {
                return DataProviders[1].Download(track);
            }
            return null;
		}

		public static DataProviderManager Instance { get; private set; }

		public List<IDataProvider> DataProviders { get; set; }

        private List<Track> DeleteDuplicates(List<Track> results)
        {
            List<Track> clearResults = new List<Track>();

            var cachedTracks = from cachedtrack in results
                               where cachedtrack.Source == TrackSource.Cache
                               select cachedtrack;
            clearResults.AddRange(cachedTracks);

            var hashValues = from cachedtrack in results
                             where cachedtrack.Source == TrackSource.Cache
                             select cachedtrack.GetHash();

            var nonCachedTracks = from nonCachedTrack in results
                                  where (nonCachedTrack.Source != TrackSource.Cache) && (!hashValues.Contains(nonCachedTrack.GetHash()))
                                  select nonCachedTrack;
            clearResults.AddRange(nonCachedTracks);

            return clearResults;
        }
	}
}
