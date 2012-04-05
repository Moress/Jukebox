
namespace Jukebox.Server.Models {
	using System;
	using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;

	[DataContract]
	class Track {
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Singer { get; set; }

		[DataMember]
		public TimeSpan Duration { get; set; }

		[DataMember]
		public TrackState State { get; set; }

		[DataMember]
		public Uri Uri { get; set; }

        [DataMember]
        public TrackSource Source { get; set; }

        /*[DataMember]
        public bool IsCached { get; set; }*/

		public override string ToString() {
			return String.Format("{0} - {1}", Title, Singer);
		}

        public string GetHash()
        {
            string input = ((this.Singer.Trim() + this.Title.Trim() + this.Duration.ToString()).ToUpper());
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
	}
}
