
namespace Jukebox.Server.DataProviders {
	using System.Collections.Generic;
	using Jukebox.Server.Models;
    using System.Linq;

	class DataProviderManager {
		public DataProviderManager() {
			Instance = this;
			DataProviders = new List<IDataProvider>();
		}

		public SearchResult Search(string query, List<TrackSourceComboItem> sources, int pageIndex, int pageSize) {
            SearchResult searchResult = new SearchResult();

            List<Track> results = new List<Track>();
            foreach (IDataProvider dataProvider in DataProviders)
            {
                foreach (TrackSourceComboItem item in sources)
                {
                    if (item.IsSelected && item.Source == dataProvider.GetSourceType())
                    {
                        results.AddRange(dataProvider.Search(query));
                    }
                }
            }
            results = DeleteDuplicates(results);

            int skipCount = pageIndex * pageSize;

            searchResult.TotalCount = results.Count;
            searchResult.FoundTracks = results.Skip(skipCount).Take(pageSize).ToList();

            return searchResult;
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
                             select cachedtrack.Id;

            var nonCachedTracks = from nonCachedTrack in results
                                  where (nonCachedTrack.Source != TrackSource.Cache) && (!hashValues.Contains(nonCachedTrack.Id))
                                  select nonCachedTrack;
            clearResults.AddRange(nonCachedTracks);

            return clearResults;
        }
	}
}
