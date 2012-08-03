
namespace Jukebox.Server {
	using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading;
    using System.Security.Cryptography;
    using System.Text;
    using System.Timers;
	using IrrKlang;
	using Jukebox.Server.DataProviders;
	using Jukebox.Server.Models;

	class Player {
		public Player() {
			Instance = this;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions, Config.GetInstance().DeviceId);
			Playlist = new Playlist();
      
			//Playlist.Tracks.CollectionChanged += OnPlaylistChanged;
			new Thread(() => {
				while (true) { PlayerThread(); Thread.Sleep(250); }
			}).Start();
		}

		private void PlayerThread() {
			Track track = Playlist.Tracks.Where(x => x.State == TrackState.Ready).FirstOrDefault();
			IsPlaying = CurrentISound == null ? false : !CurrentISound.Finished;
            if (track != null && !IsPlaying)
            {
				Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track is playing: " + track);
				TrackChanged(this, new PlayerEventArgs() { Track = track });
				Playlist.Tracks.Remove(track);
				CurrentTrack = track;
                CurrentTrack.PlayPosition = TimeSpan.FromMilliseconds(0);
                CurrentISound = Engine.Play2D(Config.GetInstance().CacheDir + track.Id + ".mp3");
			}
            else if (!IsPlaying)
            {
                CurrentTrack = null;
            }
            else if (IsPlaying)
            {
                CurrentTrack.PlayPosition = TimeSpan.FromMilliseconds((double)CurrentISound.PlayPosition);
            }

            List<Track> tracksToRemove = Playlist.Tracks.Where(x => x.State == TrackState.Failed).ToList<Track>();

            foreach (Track t in tracksToRemove)
            {
                Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Failed track has been removed: " + t);
                Playlist.Tracks.Remove(t);
            }

			foreach (Track t in Playlist.Tracks.Where(x => x.State == TrackState.Unknown)) {
                Debug.Print("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + "Track has been enqueued: " + t);
                if (File.Exists(Config.GetInstance().CacheDir + t.GetHash() + ".mp3"))
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
                        if ((data != null) && !File.Exists(Config.GetInstance().CacheDir + t.Id + ".mp3"))
                        {
                            File.WriteAllBytes(Config.GetInstance().CacheDir + t.Id + ".mp3", data);
                            File.AppendAllText(Config.GetInstance().CacheDir + "hashmap.txt", t.Id + "|" + t.Singer.Trim() + "|" + t.Title.Trim() + "|" + t.Duration.ToString() + "\r\n");
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
					track.State C:\Jukebox\repo\trunk\Sources\Server\DataProviders\VKComDataProvider.cs= TrackState.Ready;
					Debug.Print("Track ready: " + track);
				}).Start();
			}
		}*/

		public event EventHandler<PlayerEventArgs> TrackChanged;
		public event EventHandler<PlayerEventArgs> TrackStateChanged;
       
		public static Player Instance { get; private set; }
		public Track CurrentTrack { get; private set; }
		public Playlist Playlist { get; set; }
        public bool IsPlaying { get; set; }
        public double VolumeLevel {
            get
            {
                return Engine.SoundVolume;
            }
            set
            {
                Engine.SoundVolume = (float)(value);
            }
        }
		
		private ISoundEngine Engine { get; set; }
		public ISound CurrentISound { get; set; }

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
