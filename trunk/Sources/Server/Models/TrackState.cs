
namespace Jukebox.Server.Models {
	using System.Runtime.Serialization;

	[DataContract]
	public enum TrackState {
		[EnumMember]
		Unknown,
		[EnumMember]
		Downloading,
		[EnumMember]
		Ready,
		[EnumMember]
		Playing,
		[EnumMember]
		Finished,
		[EnumMember]
		Failed

	}
}
