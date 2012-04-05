
namespace Jukebox.Client.ViewModels {
	public class Settings : ViewModel {
		public Settings() {
			Host = "net.tcp://192.168.5.201:4502/";
		}

		public void Apply() {
			ServiceManager.Open(Host);
		}

		public string Host { get; set; }
	}
}
