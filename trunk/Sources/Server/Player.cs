
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
		public Player(string deviceID) {
			Instance = this;
			Engine = new ISoundEngine(SoundOutputDriver.AutoDetect, SoundEngineOptionFlag.DefaultOptions, deviceID);
			Playlist = new Playlist();
            volumeChangedTreshold = 0;
            volumeChangeTimer = new System.Timers.Timer();
            volumeChangeTimer.AutoReset = true;
            volumeChangeTimer.Elapsed += new ElapsedEventHandler(OnVolumeChangeTimerElapsed);
            volumeChangeTimer.Interval = 10000;
      
			//Playlist.Tracks.CollectionChanged += OnPlaylistChanged;
			new Thread(() => {
				while (true) { PlayerThread(); Thread.Sleep(250); }
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
                CurrentTrack.PlayPosition = TimeSpan.FromMilliseconds(0);
                CurrentISound = Engine.Play2D(@Context.GetInstance().CacheDir + track.GetHash() + ".mp3");
			}
            else if (!isPlaying)
            {
                CurrentTrack = null;
            }
            else if (isPlaying)
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

        private static void OnVolumeChangeTimerElapsed(object source, ElapsedEventArgs e)
        {
            Player.Instance.volumeChangedTreshold = 0;
            Player.Instance.volumeChangeTimer.Enabled = false;
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
        
        public const double VOLUME_TRESHOLD = 15;
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
                if (volumeChangedTreshold < VOLUME_TRESHOLD)
                {
                    volumeChangedTreshold += Math.Abs(Engine.SoundVolume - value);
                    if ((float)(value) != Engine.SoundVolume)
                    {
                        Engine.SoundVolume = (float)(value);
                    }
                }
                else
                {
                    if (volumeChangeTimer.Enabled != true)
                    {
                        volumeChangeTimer.Enabled = true;
                    }
                }
            }
        }
		
		private ISoundEngine Engine { get; set; }
		private ISound CurrentISound { get; set; }
        private System.Timers.Timer volumeChangeTimer;
        private double volumeChangedTreshold;

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
