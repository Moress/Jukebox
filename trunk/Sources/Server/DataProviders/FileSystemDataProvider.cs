
namespace Jukebox.Server.DataProviders {
	using System;
	using System.Collections.Generic;
	using Jukebox.Server.Models;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.IO;

	class FileSystemDataProvider : IDataProvider {

        public TrackSource GetSourceType()
        {
            return TrackSource.Cache;
        }
     
        public IList<Track> Search(string query) {
            var result = new List<Track>();
            try
            {
                //<DANGEROUS HARD CODE>
                var hashmap = Config.GetInstance().CacheDir + "hashmap.txt";
                //</DANGEROUS HARD CODE>

                if (!File.Exists(hashmap))
                {
                    return new ReadOnlyCollection<Track>(result);
                }

                List<string> queryWords = query.ToUpper().Split(' ').ToList();
           
                string[] lines = File.ReadAllLines(hashmap).Reverse().ToArray();
                List<KeyValuePair<int, string>> mostPossibleResults = new List<KeyValuePair<int, string>>();

                foreach (string line in lines)
                {
                    string tempLine = String.Join(" ",line.ToUpper().Split('|').Skip(1).Take(2).ToList());

                    bool hasAllQueryWords = true;
                    int relevance = 0;
                    int prevIndex = -1;
                    int currentIndex = -1;
                    foreach (string word in queryWords)
                    {
                        if (tempLine.Contains(word))
                        {
                            currentIndex = tempLine.IndexOf(word);
                            if (HasExactWord(tempLine, word))
                            {
                                currentIndex = ExactWordIndex(tempLine, word);
                                relevance += 3;
                            }
                            else
                            {
                                relevance++;
                            }

                            if (prevIndex != -1)
                            {
                                if (currentIndex > prevIndex)
                                {
                                    relevance++;
                                }
                            }
                            prevIndex = currentIndex;
                        }
                        else
                        {
                            hasAllQueryWords = false;
                            break;
                        }
                    }
                    if (hasAllQueryWords)
                    {
                        KeyValuePair<int, string> pair = new KeyValuePair<int, string>(relevance, line);
                        mostPossibleResults.Add(pair);
                    }
                }

                foreach(var mostPossibleRow in mostPossibleResults.OrderByDescending(x=>x.Key))
                {
                    Track track = new Track();

                    var values = mostPossibleRow.Value.Split('|');

                    var hash = values[0];
                    var singer = values[1];
                    var title = values[2];
                    var duration = values[3];

                    if (!File.Exists(Config.GetInstance().CacheDir + hash + ".mp3"))
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

        private bool HasExactWord(string line, string word)
        {
            if (ExactWordIndex(line, word) >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int ExactWordIndex(string line, string word)
        {
            int indexOfWord = line.IndexOf(word);
            if (indexOfWord == -1)
            {
                return -1;
            }
            int startPos = indexOfWord + word.Length;

            if (word == "")
            {
                return 0;
            }

            while (true)
            {
                bool hasSpaceBefore = false;
                bool hasSpaceAfter = false;

                if (indexOfWord == 0)
                {
                    hasSpaceBefore = true;
                }
                if ((indexOfWord + word.Length) == line.Length)
                {
                    hasSpaceAfter = true;
                }
                if ((indexOfWord - 1) >= 0 &&
                    !Char.IsLetterOrDigit(line, (indexOfWord - 1)))
                {
                    hasSpaceBefore = true;
                }
                if ((indexOfWord + word.Length) < (line.Length - 1) &&
                    !Char.IsLetterOrDigit(line, (indexOfWord + word.Length)))
                {
                    hasSpaceAfter = true;
                }

                if (hasSpaceBefore && hasSpaceAfter)
                {
                    return indexOfWord;
                }

                indexOfWord = line.IndexOf(word, startPos);
                if (indexOfWord == -1)
                {
                    break;
                }
                startPos = indexOfWord + word.Length;
            }
            return -1;
        }
	}
}
