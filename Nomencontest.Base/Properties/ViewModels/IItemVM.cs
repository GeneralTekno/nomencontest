using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Nomencontest.Base.ViewModels
{
    public interface IItemVM : INotifyPropertyChanged
    {
        string Category { get; }
        int CurrentItemIndex { get; }
        string CurrentItem { get; }
        
        int NextItem();
        bool ShuffleItems();
        
    }
    
}
