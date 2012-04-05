
namespace Jukebox.Server {
	using System;
	using System.Diagnostics;
	using System.ServiceModel;
	using System.ServiceModel.Description;
	using Jukebox.Server.DataProviders;
	using Jukebox.Server.Services;

	class JukeboxServer {
		public void Run(Uri uri) {
            Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Starting JukeboxService on {0}", uri);
       
			var a = new DataProviderManager();
			var b = new Player(Context.GetInstance().DeviceId);
            a.DataProviders.Add(new FileSystemDataProvider());
            a.DataProviders.Add(new VKComDataProvider());

			ServiceHost fservice = new ServiceHost(typeof(JukeboxService), uri);
			fservice.AddServiceEndpoint(
				typeof(IPlayerService),
				new NetTcpBinding(SecurityMode.None),
				"/Player");
			fservice.AddServiceEndpoint(
				typeof(IPlaylistService),
				new NetTcpBinding(SecurityMode.None),
				"/Playlist");
			fservice.AddServiceEndpoint(
				typeof(ISearchService),
				new NetTcpBinding(SecurityMode.None),
				"/Search");
			if (HostSocketPolicyServer) {
				fservice.AddServiceEndpoint(
					typeof(IPolicyService),
					new WebHttpBinding(),
					"http://localhost:80/")
					.Behaviors.Add(new WebHttpBehavior());
			}
			fservice.Description.Behaviors.Add(new ServiceMetadataBehavior {
				HttpGetEnabled = true,
				HttpGetUrl = new Uri("http://localhost:8080/Metadata")
			});
			fservice.Open();
		}

		public bool HostSocketPolicyServer { get; set; }
	}
}
