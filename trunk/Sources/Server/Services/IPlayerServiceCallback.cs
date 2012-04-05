
namespace Jukebox.Server.Services {
	using System.ServiceModel;
	using Jukebox.Server.Models;

	[ServiceContract(SessionMode = SessionMode.Required)]
	interface IPlayerServiceCallback {
		[OperationContract(IsOneWay = true)]
		void OnCurrentTrackChanged(Track track);
	}
}
