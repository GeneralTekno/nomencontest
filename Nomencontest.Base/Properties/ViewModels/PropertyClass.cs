using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Nomencontest.Base.ViewModels
{
    [DataContract]
    public abstract class PropertyClass : INotifyPropertyChanged
    {
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
