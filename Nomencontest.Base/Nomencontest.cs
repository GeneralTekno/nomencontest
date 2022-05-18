using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Timers;
using Nomencontest.Base.Models;
using Nomencontest.Base.ViewModels;

namespace Nomencontest.Base
{
    [DataContract]
    public class NomencontestGame : PropertyClass
    {
        private static Settings _settings;
        public Settings Settings { get { return _settings; } }

        private System.Timers.Timer _countdownTimer;

        [DataMember]
        public int RoundCount
        {
            get { return _settings.RoundCount+1; }
            set { }
        }
        [DataMember]
        public bool IsFinalRound
        {
            get { return CurrentRound >= _settings.RoundCount; }
            set { }
        }

        private string _currentDisplayName;
        [DataMember]
        public string CurrentDisplayName
        {
            get { return _currentDisplayName; }
            set { _currentDisplayName = value; }
        }
        [DataMember]
        public bool IsGameOver
        {
            get { return CurrentRound >= _settings.RoundCount + 1; }
            set { }
        }

        [IgnoreDataMember] private int _scoreMultiplier = 1;

        [DataMember]
        public int AnswerValue
        {
            get { return IsFinalRound ? _scoreMultiplier * _settings.AnswerValue : _settings.AnswerValue; }
            set { }
        }
        [DataMember]
        public int FaceOffGoal
        {
            get { return _settings.FaceOffGoal; }
            set { }
        }

        private int _currentRound = 0;

        public int CurrentRound
        {
            get { return _currentRound; }
            set
            {
                _currentRound = Math.Min(Math.Max(0, value), RoundCount);
                RaisePropertyChanged("CurrentRound");
                RaisePropertyChanged("IsFinalRound");
            }
        }

        public void UpScoreMultiplier()
        {
            _scoreMultiplier++;
            RaisePropertyChanged("AnswerValue");
        }
        public void ResetScoreMultiplier()
        {
            _scoreMultiplier=1;
            RaisePropertyChanged("AnswerValue");
        }

        private ObservableCollection<PlayerVM> _players;

        [DataMember]
        public ObservableCollection<PlayerVM> Players
        {
            get { return _players; }
            set
            {
                _players = value;
                RaisePropertyChanged("Players");
            }
        }

        private int _currentPlayerIndex;

        [DataMember]
        public int CurrentPlayerIndex
        {
            get { return _currentPlayerIndex; }
            set
            {
                _currentPlayerIndex = value;
                if (_currentPlayerIndex >= _players.Count) _currentPlayerIndex = 0;
                RaisePropertyChanged("CurrentPlayerIndex");
                RaisePropertyChanged("CurrentPlayer");
            }
        }
        [DataMember]
        public PlayerVM CurrentPlayer
        {
            get
            {
                return Players[_currentPlayerIndex];
            }
            set
            {
                if (Players.Contains(value))
                {
                    CurrentPlayerIndex = Players.IndexOf(value);
                }
            }
        }

        private RoundPhase _roundPhase;

        [DataMember]
        public RoundPhase RoundPhase
        {
            get { return _roundPhase; }
            set
            {
                _roundPhase = value;
                RaisePropertyChanged("RoundPhase");
            }
        }

        private bool _isFinalRoundPlayerGuessing = false;
        [DataMember]
        public bool IsFinalRoundPlayerGuessing
        {
            get { return _isFinalRoundPlayerGuessing; }
            set
            {
                _isFinalRoundPlayerGuessing = value;
                RaisePropertyChanged("IsFinalRoundPlayerGuessing");
            }
        }

        private ItemDatabase _itemDatabase;


        [DataMember]
        public ItemDatabase ItemDatabase
        {
            get
            {
                return _itemDatabase;
            }
            set
            {
                _itemDatabase = value;
                RaisePropertyChanged("ItemDatabase");
            }
        }

        private CategoryVM _currentCategory;


        [DataMember]
        public CategoryVM CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                _currentCategory = value;
                RaisePropertyChanged("CurrentCategory");
            }
        }

        private List<CategoryVM> _nextItems;
        [DataMember]
        public List<CategoryVM> NextItems
        {
            get { return _nextItems; }
            set
            {
                _nextItems = value;
                RaisePropertyChanged("NextItems");
            }
        }


        public void ResetTimer()
        {
            if (_countdownTimer == null)
            {
                _countdownTimer = new System.Timers.Timer(1000);
                _countdownTimer.AutoReset = true;
                _countdownTimer.Interval = 1000;
                _countdownTimer.Enabled = false;
            }
            _countdownTimer.Enabled = false;
            _countdownTimer.Stop();
            _currentTimerValue = Settings.RoundLength;
        }

        public void StartTimer()
        {
            _countdownTimer.Elapsed -= TimerTick;
            _countdownTimer.Elapsed += TimerTick;
            _currentTimerValue = Settings.RoundLength;
            _countdownTimer.Enabled = true;
            _countdownTimer.Start();
        }

        public void EndTimer()
        {
            RoundPhase = RoundPhase.IsOver;
            _countdownTimer.Stop();
            _currentTimerValue = 0;
            _countdownTimer.Elapsed -= TimerTick;
            _countdownTimer.Enabled = false;
            OnRoundEnded?.Invoke(this, new EventArgs());
        }

        [IgnoreDataMember]
        private int _currentTimerValue = 0;
        [DataMember]
        public int CurrentTimerValue
        {
            get { return _currentTimerValue;}
            set { }
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            _currentTimerValue--;
            if (_currentTimerValue > 0)
            {
                OnTimerTick(this, new EventArgs());
            }
            else
            {
                EndTimer();
            }
        }

        public EventHandler OnTimerTick { get; set; }
        public EventHandler OnRoundEnded { get; set; }

        public NomencontestGame()
        {
            if (_settings == null) _settings = new Settings();
            Players = new ObservableCollection<PlayerVM>();
            ItemDatabase = new ItemDatabase();

            CurrentCategory = new CategoryVM(new CategoryModel(string.Empty, new string[0],false));
            NextItems = new List<CategoryVM>();

            //SetupRemote();
            //CurrentCategory = new QuestionVM(Questions.GetNewItem());
        }

        public bool LoadNewItem(CategoryModel category = null)
        {
            if (category == null)
            {
                category = _itemDatabase.GetNewItem(false);
            }
            if (category == null) return false;
            CurrentCategory.Model = category;
            RaisePropertyChanged("CurrentCategory");
            
            return true;
        }
    }
}
