using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Nomencontest.Base.ViewModels;
using Nomencontest.Clients.Admin;
using Nomencontest.Clients.Admin.ViewModels;

namespace Nomencontest.Clients.Admin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow controlWindow = new MainWindow();
            
            AdminPanelVM controlpanel = new AdminPanelVM();
            if (controlpanel.GameStatus != null)
            {

                controlWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                controlWindow.DataContext = controlpanel;
                controlWindow.Closed += controlWindow_Closed;
                controlWindow.Show();
            }


            //client.Close();
        }

        void controlWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}
