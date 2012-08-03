using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Jukebox.Server.Models
{
    [DataContract]
    class SearchResult
    {
        [DataMember]
        public IList<Track> FoundTracks { get; set; }

        [DataMember]
        public int TotalCount { get; set; }
    }
}
