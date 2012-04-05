
namespace Jukebox.Server.DataProviders {
	using System;
	using System.Collections.Generic;
	using Jukebox.Server.Models;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.IO;

	class FileSystemDataProvider : IDataProvider {

        public const TrackSource ProviderType = TrackSource.Cache;
        public IList<Track> Search(string query) {
            var result = new List<Track>();
            try
            {
                //<DANGEROUS HARD CODE>
                var hashmap = @Context.GetInstance().CacheDir + "hashmap.txt";
                //</DANGEROUS HARD CODE>

                if (!File.Exists(hashmap))
                {
                    return new ReadOnlyCollection<Track>(result);
                }

                string[] lines = File.ReadAllLines(hashmap);
               
                var fileContent = from line in lines
                              where line.Replace('|', ' ').ToUpper().Contains(query.ToUpper())
                              select line;

                foreach(var fileLine in fileContent)
                {
                    Track track = new Track();

                    var values = fileLine.Split('|');

                    var hash = values[0];
                    var singer = values[1];
                    var title = values[2];
                    var duration = values[3];

                    if (!File.Exists(@Context.GetInstance().CacheDir + hash + ".mp3"))
                    {
                        continue;
                    }

                    track.Id = hash;
                    track.Singer = singer;
                    track.Title = title;
                    TimeSpan tmp;
                    TimeSpan.TryParse(duration, out tmp);
                    track.Duration = tmp;
                    track.Source = TrackSource.Cache;
                    result.Add(track);

                }
            }
            catch
            {
                Console.WriteLine("FileSystemDataProvider error");
                Console.WriteLine("Query: " + query);
            }

            return new ReadOnlyCollection<Track>(result);
		}

		public byte[] Download(Track track) {
			throw new NotImplementedException();
		}

	}
}
