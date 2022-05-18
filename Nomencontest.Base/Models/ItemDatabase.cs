using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace Forturna.Base.Models
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

        public ItemModel GetNewItem()
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            int count = 0;
            while (count < ItemEntries.Count())
            {
                var index = (int)Math.Floor(random.NextDouble() * ItemEntries.Count());
                if (!ItemEntries[index].AlreadyUsed)
                {
                    //ItemEntries[index].AlreadyUsed = true;
                    RaisePropertyChanged("UsedItems");
                    return ItemEntries[index].ItemModel;
                }
                count++;
            }

            RaisePropertyChanged("UsedItems");
            //If we're here, we've used all questions. Return null.
            return null;
        }
        public void CloseItemFile()
        {
            _itemEntries.Clear();
            _itemFilename = null;
            RaisePropertyChanged("ItemFile");
            RaisePropertyChanged("ItemEntries");
            RaisePropertyChanged("ItemFile");
            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("UsedItems");
            RaisePropertyChanged("Count");
        }
        [IgnoreDataMember]
        public string ItemFile
        {
            get { return _itemFilename != null ? _itemFilename.Name : null; }

        }

        [DataMember]
        private FileInfo _itemFilename = null;


        public bool OpenItemFile(string filename)
        {
            try
            {
                var file = File.ReadAllBytes(filename);
                if (OpenItemFile(file))
                {
                    _itemFilename = new FileInfo(filename);
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

        public bool OpenItemFile(byte[] file)
        {
            _itemEntries = new ObservableCollection<ItemEntry>();
            try
            {
                var reader = new StreamReader(new MemoryStream(file));
                var currentLine = string.Empty;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (currentLine == "#ITEM")
                    {
                        //Found a answer; start reading stuff in.
                        string question = string.Empty;

                        for (var row = 0; row < 4; row++)
                        {
                            currentLine = reader.ReadLine();
                            if (currentLine == "#HINT") break;
                            if (question != string.Empty) question += '\n';
                            question += currentLine;
                            if (row == 3) currentLine = reader.ReadLine();
                        }
                        ItemModel questionModel = new ItemModel(question);

                        if (currentLine == "#HINT")
                        {
                            questionModel.Hint = reader.ReadLine();
                            questionModel.Hint = questionModel.Hint.ToUpper();
                        }

                        if (reader.ReadLine() == "#VALUE")
                            try
                            {
                                questionModel.BaseValue = Double.Parse(reader.ReadLine());
                            }
                            catch
                            {
                            }
                        _itemEntries.Add(new ItemEntry(questionModel));
                    }
                }
                reader.Close();

            }
            catch
            {
                _itemEntries.Clear();
            }

            RaisePropertyChanged("ItemEntries");
            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("ItemFile");
            RaisePropertyChanged("UsedItems");
            RaisePropertyChanged("Count");
            return _itemEntries.Count > 0;
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
        public int ID
        {
            get
            {
                return _itemModel.ID;
            }
        }

        [IgnoreDataMember]
        public string Answer
        {
            get
            {
                return _itemModel.Answer.Replace('\n',' ');
            }
        }

        [DataMember]
        private ItemModel _itemModel;
        [IgnoreDataMember]
        public ItemModel ItemModel
        {
            get
            {
                return _itemModel;
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

        [IgnoreDataMember]
        public double BasePoints
        {
            get { return _itemModel.BaseValue; }
        }

        public ItemEntry(ItemModel item)
        {
            _itemModel = item;
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
