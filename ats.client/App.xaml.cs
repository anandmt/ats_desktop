using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ats.client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string fatalErrorMessage = e.Exception.Message;
            MessageBox.Show(fatalErrorMessage,
                caption: "An un-handled Exception",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            BootStrapper.Boot();
            ShowMainWindow();
        }
        private static void ShowMainWindow()
        {
            var window = BootStrapper.Container.Resolve<Shell>();
            Current.MainWindow = window;
            window.Show();
        }
    }
}
