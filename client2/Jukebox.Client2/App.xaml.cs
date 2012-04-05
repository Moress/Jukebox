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
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.RootVisual = new MainPage();
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // Если приложение выполняется вне отладчика, воспользуйтесь для сообщения об исключении
            // механизмом исключений браузера. В IE исключение будет отображаться в виде желтого значка оповещения 
            // в строке состояния, а в Firefox - в виде ошибки сценария.
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // ПРИМЕЧАНИЕ. Это позволит приложению выполняться после того, как исключение было выдано,
                // но не было обработано. 
                // Для рабочих приложений такую обработку ошибок следует заменить на код, 
                // оповещающий веб-сайт об ошибке и останавливающий работу приложения.
                e.Handled = true;
                //Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });

                var msg = e.ExceptionObject.Message;
                if (e.ExceptionObject.InnerException != null)
                    msg += "\n\n" + e.ExceptionObject.InnerException.Message;

                //MessageBox.Show(msg, "fffuuu", MessageBoxButton.OK);
                ErrorsWindow.AddError(msg);
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
