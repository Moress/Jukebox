
namespace Jukebox.Server {
	using System;
	using Jukebox.Server.Models;

	class PlayerEventArgs : EventArgs {
		public Track Track { get; set; }
	}
}
