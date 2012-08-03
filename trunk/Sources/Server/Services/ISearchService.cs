
namespace Jukebox.Server.Services {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Jukebox.Server.Models;
    using System.Diagnostics;
	
	[ServiceContract(SessionMode = SessionMode.Required)]
	interface ISearchService {
		[OperationContract]
		SearchResult Search(string query, List<TrackSourceComboItem> sources, int pageIndex, int pageSize);

		//[OperationContract]
		//IList<string> Suggestions(string query);
	}
}
