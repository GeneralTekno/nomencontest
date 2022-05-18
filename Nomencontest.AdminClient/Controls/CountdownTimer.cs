using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace Nomencontest.Clients.Admin.Controls
{
    /// <summary>
    /// Interaction logic for CountdownTimer.xaml
    /// </summary>
    public class CountdownTimer : INotifyPropertyChanged
    {
        private double _time;
        public double Time
        {
            get { return _time; }
            set
            {
                _time = value;
                RaisePropertyChanged("Time");
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
    }
}
