

namespace Jukebox.Client.ViewModels {
	using System.ComponentModel;

	public abstract class ViewModel : INotifyPropertyChanged {
		protected virtual void OnPropertyChanged(string property) {
			var evnt = PropertyChanged;
			if (evnt != null) evnt(this, new PropertyChangedEventArgs(property));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
