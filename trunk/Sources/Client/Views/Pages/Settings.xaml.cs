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

namespace Jukebox.Client.Views.Pages {
	public partial class Settings : Page {
		public Settings() {
			InitializeComponent();
            ServiceManager.OpenComplete += new EventHandler(ServiceManager_OpenComplete);
		}

        void ServiceManager_OpenComplete(object sender, EventArgs e)
        {
            LogListBox.Items.Add("Initialization complete.");
            ApplyButton.IsEnabled = true;
        }

		private void OnApplyButtonClicked(object sender, RoutedEventArgs e) {
            LogListBox.Items.Add("Initialization, please wait...");
            ApplyButton.IsEnabled = false;
			ServiceManager.Open(Host.Text);
		}

	}
}
