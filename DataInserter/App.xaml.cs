using DataInserter.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DataInserter
{
    public partial class App : Application
    {
        private static MainWindow app;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // For catching Global uncaught exception
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionOccured);

            var context = new MainViewModel();
            app = new MainWindow() { DataContext = context };
            app.Show();

            if (e.Args.Length == 1) //make sure an argument is passed
            {
                FileInfo file = new FileInfo(e.Args[0]);
                if (file.Exists) //make sure it's actually a file
                {
                    // Here, add you own code
                    // ((MainViewModel)app.DataContext).OpenFile(file.FullName);
                }
            }
        }

        static void UnhandledExceptionOccured(object sender, UnhandledExceptionEventArgs args)
        {
            // Here change path to the log.txt file
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                                                    + "\\" + Environment.UserName + "\\Tool_UI\\log.txt";

            // Show a message before closing application
            MessageBox.Show(
                "Oops, something went wrong and the application must close. Please find a " +
                "report on the issue at: " + path + Environment.NewLine +
                "If the problem persist, please contact cnsmacie (marcin.cierlik@consolis.com).",
                "Unhandled Error",
                MessageBoxButton.OK);

            Exception e = (Exception)args.ExceptionObject;
        }
    }
}
