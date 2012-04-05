
namespace Jukebox.Server {
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading;
    using System.Security.Cryptography;
    using System.Text;
	using IrrKlang;
	using Jukebox.Server.DataProviders;
	using Jukebox.Server.Models;

	class Player {
		public Player(string deviceID) {
			Instance = this;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions, deviceID);
			Playlist = new Playlist();
			//Playlist.Tracks.CollectionChanged += OnPlaylistChanged;
			new Thread(() => {
				while (true) { PlayerThread(); Thread.Sleep(1000); }
			}).Start();
		}

		private void PlayerThread() {
			Track track = Playlist.Tracks.Where(x => x.State == TrackState.Ready).FirstOrDefault();
			bool isPlaying = CurrentISound == null ? false : !CurrentISound.Finished;
			if (track != null && !isPlaying) {
				Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track is playing: " + track);
				TrackChanged(this, new PlayerEventArgs() { Track = track });
				Playlist.Tracks.Remove(track);
				CurrentTrack = track;
                CurrentISound = Engine.Play2D(@Context.GetInstance().CacheDir + track.GetHash() + ".mp3");
			}
            else if (!isPlaying)
            {
                CurrentTrack = null;
            }

			foreach (Track t in Playlist.Tracks.Where(x => x.State == TrackState.Unknown)) {
                Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track has been enqueued: " + t);
                if (File.Exists(@Context.GetInstance().CacheDir + t.GetHash() + ".mp3"))
                {
                    Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track has been loaded from cache: " + t);
                    t.State = TrackState.Ready;
                    Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track has state: {0} {1}", t, t.State);
                    TrackStateChanged(this, new PlayerEventArgs() { Track = t });
                }
                else
                {
                    new Thread(() =>
                    {
                        t.State = TrackState.Downloading;
                        TrackStateChanged(this, new PlayerEventArgs() { Track = t });

                        byte[] data = DataProviderManager.Instance.Download(t);
                        if ((data != null) && !File.Exists(@Context.GetInstance().CacheDir + t.GetHash() + ".mp3"))
                        {
                            File.WriteAllBytes(@Context.GetInstance().CacheDir + t.GetHash() + ".mp3", data);
                            File.AppendAllText(@Context.GetInstance().CacheDir + "hashmap.txt", t.GetHash() + "|" + t.Singer.Trim() + "|" + t.Title.Trim() + "|" + t.Duration.ToString() + "\r\n");
                        }
                        t.State = data != null ? TrackState.Ready : TrackState.Failed;
                        Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track has state: {0} {1}", t, t.State);

                        TrackStateChanged(this, new PlayerEventArgs() { Track = t });
                    }).Start();
                }
			}
		}

		/*private void OnPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems == null) return;

			foreach (Track track in e.NewItems) {
				Debug.Print("Track enqueued: " + track);

				new Thread(() => {
					byte[] data = DataProviderManager.Instance.Download(track);
					File.WriteAllBytes(@"c:\temp\jukebox\" + track.Id + ".mp3", data);
					track.State = TrackState.Ready;
					Debug.Print("Track ready: " + track);
				}).Start();
			}
		}*/

		public event EventHandler<PlayerEventArgs> TrackChanged;
		public event EventHandler<PlayerEventArgs> TrackStateChanged;
        
		public static Player Instance { get; private set; }
		public Track CurrentTrack { get; private set; }
		public Playlist Playlist { get; set; }
        public double VolumeLevel {
            get
            {
                return Engine.SoundVolume;
            }
            set
            {
                if ((float)(value) != Engine.SoundVolume)
                {
                    Engine.SoundVolume = (float)(value);
                }
            }
        }
		
		private ISoundEngine Engine { get; set; }
		private ISound CurrentISound { get; set; }

        /// <summary>
        /// Прерывает воспроизведение текущей песни.
        /// </summary>
        public void Abort()
        {
            if (CurrentISound != null)
                CurrentISound.Stop();
        }
	}
}
