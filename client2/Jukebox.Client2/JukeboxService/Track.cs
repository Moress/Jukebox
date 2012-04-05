using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Jukebox.Client2.JukeboxService
{
    public partial class Track
    {
        /// <summary>
        /// Песня уже добавлена в список.
        /// </summary>
        bool _isAdded;

        /// <summary>
        /// Песня уже добавлена в список.
        /// </summary>
        public bool IsAdded
        {
            get
            {
                return _isAdded;
            }
            set
            {
                _isAdded = value;
                RaisePropertyChanged("IsAdded");
            }
        }
    }
}
