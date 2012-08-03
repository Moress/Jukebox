using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Jukebox.Server.Models
{
    [DataContract]
    class TrackSourceComboItem
    {
        [DataMember]
        public TrackSource Source { get; set; }

        [DataMember]
        public bool IsSelected { get; set; }
    }
}
