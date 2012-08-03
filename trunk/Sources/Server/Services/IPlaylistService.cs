
namespace Jukebox.Server.Services {
	using System.Collections.Generic;
	using System.ServiceModel;
	using Jukebox.Server.Models;

	[ServiceContract(
		SessionMode = SessionMode.Required,
		CallbackContract = typeof(IPlaylistServiceCallback))]
	interface IPlaylistService {
		[OperationContract]
		Playlist GetPlaylist();

        [OperationContract]
        void SetPlaylist(Playlist playlist);

		[OperationContract(IsOneWay = true)]
		void Add(Track track);

		[OperationContract(IsOneWay = true)]
		void Remove(Track track);

        /// <summary>
        ///  Принимает голос от пользователя, что песню нужно пропустить.
        ///  <returns>Сообщение о том, сколько голосов уже принято.</returns>
        /// </summary>
        [OperationContract]
        string Next();

        [OperationContract]
        string Shuffle();
	}
}
