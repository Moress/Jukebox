
namespace Jukebox.Server {
	using System;
	using System.Diagnostics;
	using System.ServiceModel;
	using System.ServiceModel.Description;
	using Jukebox.Server.DataProviders;
	using Jukebox.Server.Services;

	class JukeboxServer {
		public void Run() {
            Uri uri = new Uri(Config.GetInstance().Host);
            bool HostSocketPolicyServer = Config.GetInstance().HasSocketPolicyServer;
            Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Starting JukeboxService on {0}", uri);
       
			var a = new DataProviderManager();
			var b = new Player();
            var c = new UserManager();

            a.DataProviders.Add(new FileSystemDataProvider());
            a.DataProviders.Add(new VKComDataProvider());

            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None);
            
			ServiceHost fservice = new ServiceHost(typeof(JukeboxService), uri);

			fservice.AddServiceEndpoint(
				typeof(IPlayerService),
                tcpBinding,
				"/Player");
			fservice.AddServiceEndpoint(
				typeof(IPlaylistService),
                tcpBinding,
				"/Playlist");
			fservice.AddServiceEndpoint(
				typeof(ISearchService),
                tcpBinding,
				"/Search");
            fservice.AddServiceEndpoint(
                typeof(IUserService),
                tcpBinding,
                "/User");
			if (HostSocketPolicyServer) {
				fservice.AddServiceEndpoint(
					typeof(IPolicyService),
					new WebHttpBinding(),
					"http://localhost:80/")
					.Behaviors.Add(new WebHttpBehavior());
			}
            fservice.Description.Behaviors.Add(new ServiceMetadataBehavior
            {
                HttpGetEnabled = true,
                HttpGetUrl = new Uri("http://localhost:8080/Metadata")
            });

			fservice.Open();
		}

		public bool HostSocketPolicyServer { get; set; }
	}
}
