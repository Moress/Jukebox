
namespace Jukebox.Server.Services {
	using System.ServiceModel;
	using Jukebox.Server.Models;

	[ServiceContract(SessionMode = SessionMode.Required)]
	interface IPlaylistServiceCallback {
		[OperationContract(IsOneWay = true)]
		void OnTrackAdded(Track track);

		[OperationContract(IsOneWay = true)]
		void OnTrackRemoved(Track track);

		[OperationContract(IsOneWay = true)]
		void OnTrackStateChanged(Track track);
	}
}
