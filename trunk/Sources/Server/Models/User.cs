using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Jukebox.Server.Models
{
    [DataContract]
    class User : IEquatable<User>
    {
        [DataMember]
        public string UserIpAddress { get; set; }
        [DataMember]
        public int ActionPoints { get; set; }
        [DataMember]
        public double VolumeLevelDiffs { get; set; }

        public bool Equals(User other)
        {
            if (this.UserIpAddress == other.UserIpAddress)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            User user = obj as User;

            return this.Equals(user);
        }

        public override int GetHashCode()
        {
            return this.UserIpAddress.GetHashCode();
        }
    }
}
