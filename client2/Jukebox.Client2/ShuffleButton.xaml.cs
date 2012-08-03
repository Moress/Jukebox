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

namespace Jukebox.Client2
{
    public partial class ShuffleButton : UserControl
    {
        public event EventHandler ShuffleButtonClick;

       public ShuffleButton()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            var storyboard = (Storyboard)Resources["Storyboard1"];
            storyboard.Begin();
            Rectangle1.Fill = (Brush)Resources["HoverBrush"];
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            var storyboard = (Storyboard)Resources["Storyboard1"];
            storyboard.Stop();
            Rectangle1.Fill = (Brush)Resources["NormalBrush"];
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle1.Fill = (Brush)Resources["DownBrush"];

            var storyboard = (Storyboard)Resources["Storyboard1"];
            storyboard.Pause();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle1.Fill = (Brush)Resources["HoverBrush"];

            var storyboard = (Storyboard)Resources["Storyboard1"];
            storyboard.Resume();

            if (ShuffleButtonClick != null)
                ShuffleButtonClick(this, null);
        }
    }
}
