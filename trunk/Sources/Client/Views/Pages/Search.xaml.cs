using System;
using System.Windows.Controls;
using System.Windows.Input;
using Jukebox.Client.JukeboxService;
using System.Windows.Threading;

namespace Jukebox.Client.Views.Pages {
	public partial class Search : Page {

        private DispatcherTimer pMessageTimer;

		public Search() {
			InitializeComponent();

            pMessageTimer = new DispatcherTimer();
            pMessageTimer.Interval = new TimeSpan(0, 0, 0, 3);
            pMessageTimer.Tick += new EventHandler(pMessageTimer_Tick);

            ((ViewModels.Search)Resources["searchViewModel"]).PropertyChanged +=
                new System.ComponentModel.PropertyChangedEventHandler(Search_PropertyChanged);
		}

        void Search_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var list = ((ViewModels.Search)Resources["searchViewModel"]).SearchResults;

            // Получили пустой список песен
            if (list != null && list.Count == 0)
                ShowMessage("Nothing was found.");
        }

        void pMessageTimer_Tick(object sender, EventArgs e)
        {
            MessageTextBlock.Text = null;
            pMessageTimer.Stop();
        }

		private void Query_KeyDown(object sender, KeyEventArgs e) {
			// ГОВНОКОД
			if (e.Key == Key.Enter) {
				((ViewModels.Search)Resources["searchViewModel"]).Find(Query.Text);
				Query.Text = String.Empty;
			}
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
			// ГОВНОКОД
			((ViewModels.Search)Resources["searchViewModel"]).AddTrack(
				(Track)((Button)e.OriginalSource).Tag);
		}

        /// <summary>
        ///  Отображает сообщение, которое пропадает через какое-то время
        /// </summary>
        /// <param name="message"></param>
        private void ShowMessage(string message)
        {
            MessageTextBlock.Text = message;
            pMessageTimer.Start();
        }
	}
}
