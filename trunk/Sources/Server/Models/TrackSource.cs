
namespace Jukebox.Server.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    enum TrackSource
    {
        [EnumMember]
        VK,
        [EnumMember]
        Cache
    }
}
