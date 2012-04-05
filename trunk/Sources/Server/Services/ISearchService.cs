
namespace Jukebox.Server.Services {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Jukebox.Server.Models;
	
	[ServiceContract(SessionMode = SessionMode.Required)]
	interface ISearchService {
		[OperationContract]
		IList<Track> Search(string query);

		//[OperationContract]
		//IList<string> Suggestions(string query);
	}
}
