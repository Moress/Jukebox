
namespace Jukebox.Server.DataProviders {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.Net;
	using System.Text;
	using System.Text.RegularExpressions;
	using Jukebox.Server.Models;

	class ProstopleerDataProvider : IDataProvider {

        public TrackSource GetSourceType()
        {
            return 0;
        }

		public IList<Track> Search(string query) {
			// Result
			var result = new List<Track>();
			try {
				Debug.Print(": " + query);

				// Download results
				var webClient = new WebClient();
				webClient.Encoding = Encoding.UTF8;
				webClient.Headers.Add("X-Requested-With", "XMLHttpRequest");
				var response = webClient.DownloadString("http://prostopleer.com/search?q=" + query.Replace(" ", "%20"));

				// Prepare and parse string
				response = response.Substring(25, response.Length - 25 - 2);
				var regExp = new Regex("<li (.*?)>", RegexOptions.Compiled);
				foreach (Match item in regExp.Matches(response)) {
					var xmlString = item.ToString().Replace("\\\"", "\"").Replace("<li ", "").Replace(">", "");

					Track tr = new Track();
					var regExp2 = new Regex("([^=,]*)=(\"[^\"]*\"|[^,\"]*)", RegexOptions.Compiled);
					foreach (Match item2 in regExp2.Matches(xmlString)) {
						var key = item2.Groups[1].ToString().Trim().ToLowerInvariant();
						var value = item2.Groups[2].ToString().Trim().ToLowerInvariant().Replace("\"", "");

						switch (key) {
						case "duration": tr.Duration = TimeSpan.FromSeconds(int.Parse(value)); break;
						case "singer": tr.Singer = value; break;
						case "song": tr.Title = value; break;
						case "file_id": tr.Id = value; break;
						}
					}
					result.Add(tr);
				}
			} catch {
				Debug.Print(": shit happens");
			}

			Debug.Print(": shomething found");
			return new ReadOnlyCollection<Track>(result);
		}

		public byte[] Download(Track track) {
			try {
				WebClient c = new WebClient();
				return c.DownloadData("http://prostopleer.com/download/" + track.Id);
			} catch {
				return null;
			}
		}
	}
}
