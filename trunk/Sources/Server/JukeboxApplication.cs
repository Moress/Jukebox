
namespace Jukebox.Server {
	using System;
	using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Jukebox.Server.DataProviders;
    using Jukebox.Server.Models;
    using System.Collections.Generic;
    using IrrKlang;

	class JukeboxApplication {
        const string FILES_PATH = @"c:\temp\jukebox\";

		[STAThread]
		static void Main(string[] args) {
			Debug.Listeners.Add(new ConsoleTraceListener());
			Debug.AutoFlush = true;

            // Проверяем наличие пути, пытаемся создать и бросаем исключение, если не получилось
            if (!Directory.Exists(FILES_PATH))
                Directory.CreateDirectory(FILES_PATH);

			Debug.Print("Brainster's music server");
			Debug.Print("========================");		

			Console.Write("Host (net.tcp://localhost:4502/): ");
			var host = Console.ReadLine();
			host = host.Length == 0 ? "net.tcp://localhost:4502/" : host;

			Console.Write("SocketPolicyServer (true): ");
			var ssp = Console.ReadLine();
			var sspb = ssp.Length == 0 ? true : bool.Parse(ssp);

            Console.WriteLine("Output devices: ");
            ISoundDeviceList outputDevices = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice);
            for (int i = 0; i < outputDevices.DeviceCount; i++)
            {
                Console.WriteLine(i.ToString() + ": " + outputDevices.getDeviceDescription(i));
            }
            Console.Write("Choose device (0): ");
            var deviceString = Console.ReadLine();
            int deviceIndex = deviceString.Length !=1 ? 0 : int.Parse(deviceString);
            string deviceID = outputDevices.getDeviceID(deviceIndex);
            Context.GetInstance().DeviceId = deviceID;
            
            Console.Write("Cache directory (" + @"C:\temp\jukebox\" + "): ");
            var cacheDir = Console.ReadLine();
            cacheDir = cacheDir.Length == 0 ? @"C:\temp\jukebox\" : cacheDir;
            Context.GetInstance().CacheDir = cacheDir;
            
			new JukeboxServer() { HostSocketPolicyServer = sspb }
				.Run(new Uri(host));
			Debug.Print("Jukebox started. Press <anykey> to quit.");

            while (true)
            {
                string command = Console.ReadLine();
                command = command.ToUpper();
                switch (command)
                {
                    case "LUNCH TIME":
                        {
                            Player.Instance.Abort();
                            Player.Instance.Playlist.Tracks.Clear();
                            List<Track> tracks = DataProviderManager.Instance.Search("Батька атаман").ToList();
                            if (tracks.Count != 0)
                            {
                                Player.Instance.Playlist.Tracks.Add(tracks[0]);
                            }
                            break;
                        }
                    default: { break; }
                }
            }
		}
	}
}
