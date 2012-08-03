
namespace Jukebox.Server.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum TrackSource
    {
        [EnumMember]
        VK,
        [EnumMember]
        Cache
    }
}
