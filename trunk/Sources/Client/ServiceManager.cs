
namespace Jukebox.Client {
	using System.ComponentModel;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using Jukebox.Client.JukeboxService;
using System;

	public static class ServiceManager {

        public static event EventHandler OpenComplete;

        private static int pOpenedServices = 0;

		public static void Open(string host) {
			CustomBinding binding = new CustomBinding(
				new BinaryMessageEncodingBindingElement(),
				new TcpTransportBindingElement());

			SearchService = new SearchServiceClient(binding, new EndpointAddress(host + "Search"));
			PlaylistService = new PlaylistServiceClient(binding, new EndpointAddress(host + "Playlist"));
			PlayerService = new PlayerServiceClient(binding, new EndpointAddress(host + "Player"));

            pOpenedServices = 0;

			SearchService.OpenCompleted += OnServiceOpenCompleted;
			PlaylistService.OpenCompleted += OnServiceOpenCompleted;
			PlayerService.OpenCompleted += OnServiceOpenCompleted;

			SearchService.OpenAsync();
			PlaylistService.OpenAsync();
			PlayerService.OpenAsync();
		}

		static void OnServiceOpenCompleted(object sender, AsyncCompletedEventArgs e) {
			if (e.Error != null) {
				throw e.Error;
			}
            pOpenedServices++;

            if (pOpenedServices == 3 && OpenComplete != null) {
                OpenComplete(sender, null);
            }
		}

		public static SearchServiceClient SearchService { get; set; }
		public static PlaylistServiceClient PlaylistService { get; set; }
		public static PlayerServiceClient PlayerService { get; set; }
	}
}
