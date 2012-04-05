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

namespace Jukebox.Client2
{
    public partial class ErrorsWindow : ChildWindow
    {
        //public List<string> Errors = new List<string>();
        ObservableCollection<String> _errors = new ObservableCollection<String>();
        private static ErrorsWindow _instance = new ErrorsWindow();

        public ErrorsWindow()
        {
            InitializeComponent();

            DataContext = _errors;
        }

        public static void AddError(string text)
        {
            _instance._errors.Add(text);
            _instance.Show();
        }
    }
}

