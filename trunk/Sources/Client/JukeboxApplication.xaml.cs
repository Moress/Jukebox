
namespace Jukebox.Client {
	using System.Windows;
	using System.Windows.Controls;

	public partial class JukeboxApplication : Application {
		public JukeboxApplication() {
			this.Startup += this.Application_Startup;
			this.UnhandledException += this.Application_UnhandledException;

			InitializeComponent();
		}

		private void Application_Startup(object sender, StartupEventArgs e) {
			this.RootVisual = new Main();
		}

		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
			// If the app is running outside of the debugger then report the exception using
			// a ChildWindow control.
			//if (!System.Diagnostics.Debugger.IsAttached) {
				// NOTE: This will allow the application to continue running after an exception has been thrown
				// but not handled. 
				// For production applications this error handling should be replaced with something that will 
				// report the error to the website and stop the application.
				e.Handled = true;
				ChildWindow errorWin = new ErrorWindow(e.ExceptionObject);
				errorWin.Show();
			//}
		}
	}
}