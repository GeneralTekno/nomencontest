using System.Runtime.Serialization;
using System.Windows.Media;
using Nomencontest.Base.Models;

namespace Nomencontest.Base.ViewModels
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

        private bool _hasPlayedRound;
        [DataMember]
        public bool HasPlayedRound
        {
            get { return _hasPlayedRound; }
            set
            {
                _hasPlayedRound = value;
                RaisePropertyChanged("HasPlayedRound");
            }
        }

        [DataMember]
        public double Points
        {
            get
            {
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

        public void ResetPoints()
        {
            Points = 0;
        }


        public PlayerVM(PlayerModel teamModel)
        {
            _hasPlayedRound = false;
            _teamModel = teamModel;
            IsPlaying = true;
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Points");
        }
        public PlayerVM(string name)
        {
            _teamModel = new PlayerModel(name);
            _hasPlayedRound = false;
            IsPlaying = true;
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Points");
        }
    }
}
