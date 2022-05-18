using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Nomencontest.Base.ViewModels;

namespace Nomencontest.Base.Models
{
    [DataContract]
    public class ItemDatabase:INotifyPropertyChanged
    {
        private ObservableCollection<ItemEntry> _itemEntries;

        [IgnoreDataMember]
        public bool HasItems
        {
            get
            {
                return _itemEntries.Count != 0 && _itemEntries.All(item => !item.AlreadyUsed);
            }
        }
        [IgnoreDataMember]
        public int UsedItems
        {
            get
            {
                return _itemEntries.Count(item => item.AlreadyUsed);
            }
        }
        [IgnoreDataMember]
        public int Count
        {
            get
            {
                return _itemEntries.Count;
            }
        }

        [IgnoreDataMember]
        private bool _randomSorting = false;
        [IgnoreDataMember]
        public bool RandomSort
        {
            get { return _randomSorting; }
        }


        [DataMember]
        public ObservableCollection<ItemEntry> ItemEntries
        {
            get
            {
                return _itemEntries;
            }
            set
            {
                _itemEntries = value;
                RaisePropertyChanged("ItemEntries");
                RaisePropertyChanged("UsedItems");
                RaisePropertyChanged("HasItems");
            }
        }

        public ItemDatabase()
        {
            ItemEntries = new ObservableCollection<ItemEntry>();
        }

        public ItemEntry GetItemEntry(uint ID)
        {
            return ItemEntries.First(x => x.ID == ID);
        }

        public CategoryModel GetItem(uint ID)
        {
            return ItemEntries.First(x => x.ID == ID).CategoryModel;
        }

        public CategoryModel GetNewItem(bool isFinalItem)
        {
            Random random = new Random((int) DateTime.Now.Ticks);
            int count = 0;
            while (count < ItemEntries.Count())
            {
                var index = _randomSorting ? (int) Math.Floor(random.NextDouble()*ItemEntries.Count()) : count;
                if (!ItemEntries[index].AlreadyUsed && ItemEntries[index].IsFinal == isFinalItem)
                {
                    //ItemEntries[index].AlreadyUsed = true;
                    RaisePropertyChanged("UsedItems");
                    return ItemEntries[index].CategoryModel;
                }
                count++;
            }

            RaisePropertyChanged("UsedItems");
            //If we're here, we've used all questions. Return null.
            return null;
        }


        public List<CategoryModel> GetNewItems(int count, bool isFinalItem)
        {
            var items = new List<CategoryModel>();
            Random random = new Random((int)DateTime.Now.Ticks);
            int internalCount = 0;
            while (internalCount < ItemEntries.Count())
            {
                var index = _randomSorting ? (int)Math.Floor(random.NextDouble() * ItemEntries.Count()) : internalCount;
                if (!ItemEntries[index].AlreadyUsed && ItemEntries[index].IsFinal == isFinalItem)
                {
                    //ItemEntries[index].AlreadyUsed = true;
                    RaisePropertyChanged("UsedItems");
                    if (!items.Contains(ItemEntries[index].CategoryModel)) items.Add(ItemEntries[index].CategoryModel);
                    if (items.Count >= count) break;
                }
                internalCount++;
            }

            RaisePropertyChanged("UsedItems");
            //If we're here, we've used all questions. Return null.
            return items;
        }


        public bool OpenItemFile(string filename)
        {
            try
            {
                var file = File.ReadAllBytes(filename);
                if (OpenItemFile(file))
                {
                    return true;
                }
            }
            catch
            {
                MessageBox.Show("Unable to open file.");
            }
            return false;

        }

        public void ResetUsedItems()
        {
            foreach (var item in _itemEntries)
            {
                item.AlreadyUsed = false;
            }
            RaisePropertyChanged("UsedItems");
            RaisePropertyChanged("HasItems");
        }

        public void ClearItemFile()
        {
            _itemEntries.Clear();
            RaisePropertyChanged("ItemEntries");
            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("ItemFile");
            RaisePropertyChanged("UsedItems");
            RaisePropertyChanged("Count");
        }

        public bool DeleteItem(uint itemID)
        {
            var item = _itemEntries.First(x => x.ID == itemID);
            if (item != null)
            {
                _itemEntries.Remove(item);
                RaisePropertyChanged("ItemEntries");
                RaisePropertyChanged("HasItems");
                RaisePropertyChanged("ItemFile");
                RaisePropertyChanged("UsedItems");
                RaisePropertyChanged("Count");
                return true;
            }
            return false;
        }

        public bool OpenItemFile(byte[] file)
        {
            var returnValue = false;
            if (_itemEntries == null)
            {
                _itemEntries = new ObservableCollection<ItemEntry>();
            }
            try
            {
                var reader = new StreamReader(new MemoryStream(file));
                var currentLine = string.Empty;
                bool isFinal = false;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (currentLine.Trim() == "#CATEGORY")
                    {
                        string category = reader.ReadLine();
                        while ((currentLine = reader.ReadLine()) != null)
                        {
                            if (currentLine.Trim() == "#FINAL")
                            {
                                isFinal = true;
                                continue;
                            }
                            if (currentLine.Trim() == "#ITEMS")
                                break;
                        }
                        if (currentLine == null) break;
                        //Start reading items until #END is reached
                        List<string> items = new List<string>();
                        while ((currentLine = reader.ReadLine()) != null && currentLine != "#END")
                        {
                            if (!string.IsNullOrWhiteSpace(currentLine))
                            {
                                items.Add(currentLine.Trim().ToUpper());
                            }
                        }
                        var item = new CategoryModel(category, items.ToArray(), isFinal);
                        if (!_itemEntries.Any(x => x.Category == item.Category))
                        {
                            _itemEntries.Add(new ItemEntry(item));
                            returnValue = true;
                        }
                    }
                }
                reader.Close();

            }
            catch
            {
            }

            RaisePropertyChanged("ItemEntries");
            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("ItemFile");
            RaisePropertyChanged("UsedItems");
            RaisePropertyChanged("Count");
            return returnValue;
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

    [DataContract]
    public class ItemEntry
    {
        [IgnoreDataMember]
        public uint ID
        {
            get
            {
                return _categoryModel.ID;
            }
        }

        [IgnoreDataMember]
        public string Category
        {
            get
            {
                return _categoryModel.Category;
            }
        }
        [IgnoreDataMember]
        public bool IsFinal
        {
            get
            {
                return _categoryModel.IsFinal;
            }
        }

        [DataMember]
        private CategoryModel _categoryModel;
        [IgnoreDataMember]
        public CategoryModel CategoryModel
        {
            get
            {
                return _categoryModel;
            }
        }

        [DataMember]
        private bool _alreadyUsed = false;

        [IgnoreDataMember]
        public bool AlreadyUsed
        { 
            get { return _alreadyUsed; }
            set
            {
                _alreadyUsed = value;
                RaisePropertyChanged("AlreadyUsed");
            } 
        }
        
        public ItemEntry(CategoryModel category)
        {
            _categoryModel = category;
            AlreadyUsed = false;
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
