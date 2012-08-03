
namespace Jukebox.Server {
	using System;
	using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Jukebox.Server.DataProviders;
    using Jukebox.Server.Models;
    using System.Collections.Generic;
    using System.Configuration;
    using IrrKlang;

	class JukeboxApplication {
		[STAThread]
		static void Main(string[] args) {
			Debug.Listeners.Add(new ConsoleTraceListener());
			Debug.AutoFlush = true;

			Debug.Print("Brainster's music server");
			Debug.Print("========================");

            new JukeboxServer().Run();
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
                            List<TrackSourceComboItem> sources = new List<TrackSourceComboItem>();
                            sources.Add(new TrackSourceComboItem() { Source = TrackSource.Cache,
                                                                     IsSelected = true});
                            sources.Add(new TrackSourceComboItem() { Source = TrackSource.VK,
                                                                     IsSelected = true});
                            SearchResult results = DataProviderManager.Instance.Search("Батька атаман", sources, 0, 50);
                            if (results.FoundTracks.Count != 0)
                            {
                               Player.Instance.Playlist.Tracks.Add(results.FoundTracks[0]);
                            }
                            break;
                        }
                    case "RESTORE POINTS":
                        {
                            UserManager.Instance.RestorePoints();
                            break;
                        }
                    default: { break; }
                }
            }
		}
	}
}
