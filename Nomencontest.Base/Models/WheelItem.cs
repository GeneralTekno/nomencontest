using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Threading;

namespace Forturna.Base.Models
{
    [DataContract]
    public class WheelItem : INotifyPropertyChanged
    {
        public SolidColorBrush TextColor
        {
            get
            {
                if (Color == null) return new SolidColorBrush();
                var color = (Color.Color.R * 0.299 + Color.Color.G * 0.587 + Color.Color.B * 0.114) > 186
                    ? System.Drawing.Color.Black
                    : System.Drawing.Color.White;
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public double Value { get; set; }

        public SolidColorBrush Color {
            get {
                var color = (Color)ColorConverter.ConvertFromString(ColorString);
                return new SolidColorBrush(color);
            }
        }

        [DataMember]
        public string ColorString { get; set; }
        private bool _isSelected = false;
        [DataMember]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        public WheelItem(string key, double value, string color)
        {
            Key = key;
            Value = value;

            ColorString = color;
        }

        public WheelItem(string key, double value, Color color)
        {
            Key = key;
            Value = value;
            ColorString = new ColorConverter().ConvertToString(color);
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

        public const string SPOKES_FILE = "Wheel.spokes";
        public static List<WheelItem> LoadWheelItems()
        {
            var masterList = new List<WheelItem>();
            try
            {
                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SPOKES_FILE);
                var stream = File.OpenRead(filename);
                var reader = new StreamReader(stream);
                var currentLine = string.Empty;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (!currentLine.StartsWith("#"))
                    {
                        //Format is "string value", data value, color code

                        var items = currentLine.Trim().Split(',');
                        if (items.Length != 3) continue;

                        var key = items[0].ToString();
                        int itemValue = 0;
                        Int32.TryParse(items[1].ToString(), out itemValue);
                        if (string.IsNullOrEmpty(items[2])) items[2] = "#ffffff";

                        WheelItem item = new WheelItem(key, itemValue, items[2]);

                        masterList.Add(item);
                    }
                }
                reader.Close();
                stream.Close();

            }
            catch
            {
                return null;
            }
            return masterList;
        }

        public override bool Equals(object obj)
        {
            double value = -1;
            if (obj is double) value = (double) obj;
            else if (obj is float) value = (float)obj;
            else if (obj is int) value = (int)obj;
            else if (obj is string) return Key.Equals((string)obj);
            else if (obj is WheelItem)
            {
                var wi = obj as WheelItem;
                return (wi.Key == Key && wi.Value.Equals(value));
            }
            else return false;
            return value.Equals(value);
        }
    }
}
