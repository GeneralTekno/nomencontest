using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Nomencontest.Core.Wrapper
{
    /// <summary>
    /// Interaction logic for RemotePopup.xaml
    /// </summary>
    public partial class ServerConnectPopup : Window, INotifyPropertyChanged
    {
        public ServerConnectPopup(string ip, int port1)
        {
            IPAddress = ip + ":" + port1;
            InitializeComponent();
        }

        private string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                RaisePropertyChanged("IPAddress");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event 
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        private void CancelCommandExecuted(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKCommandExecuted(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
