using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Jukebox.Client.Views.Pages {
	public partial class Playlist : Page {

        /// <summary>
        /// Инициализация обработчиков прошла?
        /// </summary>
        private bool pInitComplete;

		public Playlist() {
			InitializeComponent();
		}

		// Executes when the user navigates to this page.
		protected override void OnNavigatedTo(NavigationEventArgs e) {
			((ViewModels.Playlist)Resources["playlistViewModel"]).UpdatePlaylist();

            if (!pInitComplete)
            {
                ServiceManager.PlayerService.GetCurrentTrackCompleted += PlayerService_GetCurrentTrackCompleted;
                pInitComplete = true;
            }

            OnRefreshNowPlayingButtonClicked(this, null);
		}

        private void OnRefreshNowPlayingButtonClicked(object sender, RoutedEventArgs e)
        {
            ServiceManager.PlayerService.GetCurrentTrackAsync();
        }

        void PlayerService_GetCurrentTrackCompleted(object sender, JukeboxService.GetCurrentTrackCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                NowPlayingTextBlock.Text = "<nothing>";
                return;
            }

            NowPlayingTextBlock.Text = e.Result.Singer + " — " + e.Result.Title;
        }

	}
}
