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
using System.Collections.ObjectModel;
using Jukebox.Client2.JukeboxService;

namespace Jukebox.Client2
{
    public partial class SearchResultsControl : UserControl
    {
        public SearchResultsControl()
        {
            InitializeComponent();
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var track = button.Tag as Track;

            var service = ServiceManager.GetPlaylistServiceClient();
            service.AddAsync(track);

            // Отмечаем, чтобы убрать кнопку
            track.IsAdded = true;
        }
    }
}
