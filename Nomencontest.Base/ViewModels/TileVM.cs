using System.Runtime.Serialization;
using System.Windows;

namespace Forturna.Base.ViewModels
{
    [DataContract]
    public class TileVM : PropertyClass
    {
        [DataMember]
        private char _value;

        [DataMember]
        public char Value
        {
            get { return _value; }
            set { }

        }

        [DataMember]
        private Visibility _visibility;

        [DataMember]
        public Visibility Visibility
        {
            get { return _visibility; }
            set { }
        }

        public void HighlightTile()
        {
            _visibility = Visibility.Hidden;
            RaisePropertyChanged("Visibility");
            
        }
        public void BlankTile()
        {
            _visibility = Visibility.Collapsed;
            RaisePropertyChanged("Visibility");

        }

        [IgnoreDataMember]
        private Rect _currentPosition;
        [IgnoreDataMember]
        public Rect CurrentPosition
        {
            get { return _currentPosition; }
            set
            {
                _currentPosition = value;
                RaisePropertyChanged("CurrentPosition");
            }
        }

        [IgnoreDataMember]
        private bool _hideLetter;
        [DataMember]
        public bool HideLetter
        {
            get { return _hideLetter; }
            set
            {
                _hideLetter = value;
                RaisePropertyChanged("HideLetter");
            }
        }


        [DataMember]
        private int _row;
        [DataMember]
        public int Row
        {
            get { return _row; }
            set { }
        }
        [DataMember]
        private int _column;
        [DataMember]
        public int Column
        {
            get { return _column; }
            set { }
        }

        public void FlipTile()
        {
            _visibility = Visibility.Visible;
            RaisePropertyChanged("Visibility");

        }

        public TileVM(char value, int row, int column, Visibility visible = Visibility.Collapsed)
        {
            _row = row;
            _column = column;
            _value = value;
            _visibility = (value < 'A' || value > 'Z') ? Visibility.Visible : visible;
        }

        public override bool Equals(object obj)
        {
            var tile = obj as TileVM;
            if (tile == null) return false;

            return (tile.Row == Row && tile.Column == Column && tile.Value == Value);
        }

        public void CopyTileStatus(TileVM oldTile)
        {
            _visibility = oldTile.Visibility;
            RaisePropertyChanged("Visibility");
        }
    }
}
