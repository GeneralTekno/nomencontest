using System.Runtime.Serialization;
using System.Windows.Media;
using Forturna.Base.Models;

namespace Forturna.Base.ViewModels
{
    [DataContract]
    public class PlayerVM : PropertyClass
    {
        private PlayerModel _teamModel;

        [DataMember]
        public PlayerModel Model
        {
            get { return _teamModel; }
            set
            {
                _teamModel = value;
                RaisePropertyChanged("Model");
                RaisePropertyChanged("Points");
                RaisePropertyChanged("Name");
                RaisePropertyChanged("TotalPoints");
            }
        }
        [IgnoreDataMember]
        public Brush PlayerColor
        {
            get { return new SolidColorBrush(_teamModel.Color); }
        }
        [DataMember]
        public string ColorCode
        {
            get
            {
                var hex = new ColorConverter().ConvertToString(PlayerColor);
                if (hex.Length > 7)
                    hex = hex.Remove(1, 2);
                return hex;
            }
            set { }
        }

        private bool _showBankrupt = false;
        [DataMember]
        public bool ShowBankrupt
        {
            get { return _showBankrupt; }
            set
            {
                _showBankrupt = value;
                RaisePropertyChanged("ShowBankrupt");
                RaisePropertyChanged("Points");
            }
        }
        
        private bool _isPlaying;
        [DataMember]
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }

        private bool _missATurn;
        [DataMember]
        public bool MissATurn
        {
            get { return _missATurn; }
            set
            {
                _missATurn = value;
                RaisePropertyChanged("MissATurn");
            }
        }

        [DataMember]
        public double Points
        {
            get
            {
                if (_showBankrupt) return -1000;
                return _teamModel.Points;
            }
            set
            {
                _teamModel.Points = value;
                RaisePropertyChanged("Points");
            }
        }

        [DataMember]
        public string Name
        {
            get
            {
                return _teamModel.Name;
            }
            set
            {
                _teamModel.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        [DataMember]
        public double TotalPoints
        {
            get { return _teamModel.TotalPoints; }
            set { }
        }

        public void CommitPoints()
        {
            _teamModel.TotalPoints += _teamModel.Points;
            RaisePropertyChanged("TotalPoints");
        }

        public void ResetPoints()
        {
            Points = 0;
        }


        public void ResetTotalPoints()
        {
            Points = 0;
            _teamModel.TotalPoints = 0;
            RaisePropertyChanged("TotalPoints");
        }

        public PlayerVM(PlayerModel teamModel)
        {
            MissATurn = false;
            _teamModel = teamModel;
            IsPlaying = true;
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Points");
        }
        public PlayerVM(string name, Color color)
        {
            IsPlaying = true;
            MissATurn = false;
            _teamModel = new PlayerModel(name,color);
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Points");
        }
    }
}
