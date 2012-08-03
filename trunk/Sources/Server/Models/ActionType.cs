using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Jukebox.Server.Models
{
    [DataContract]
    public enum ActionType
    {
        [EnumMember]
        PauseOrPlayAction,
        [EnumMember]
        ShuffleAction,
        [EnumMember]
        VolumeChangeAction
    }
}
