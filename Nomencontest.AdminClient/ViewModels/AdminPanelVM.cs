using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Nomencontest.Base;
using Nomencontest.Base.Models;
using Nomencontest.Base.ViewModels;
using Nomencontest.Clients.Admin.Controls;
using Nomencontest.Core.Wrapper;

namespace Nomencontest.Clients.Admin.ViewModels
{
    public class AdminPanelVM : INotifyPropertyChanged
    {
        private const string ADMIN_KEY = "2edffbfc-0308-4648-adbc-32d088ecbf8d";

        private ItemDatabase _cachedItemDB;

        public ItemDatabase CachedItemDB
        {
            get { return _cachedItemDB; }
            private set
            {
                _cachedItemDB = value;
                RaisePropertyChanged("CachedItemDB");
                RaisePropertyChanged("FilteredItemEntries");
            }
        }

        public ObservableCollection<ItemEntry> FilteredItemEntries
        {
            get { return CachedItemDB != null ? new ObservableCollection<ItemEntry>(CachedItemDB.ItemEntries.Where(x => x.IsFinal == GameStatus.IsFinalRound)) : null; }
        }

        private Settings _settings;

        public Settings Settings
        {
            get { return _settings; }
            private set
            {
                _settings = value;
                RaisePropertyChanged("Settings");
            }
        }

        #region GameStatus
        private GameStatus _gameStatus;

        public GameStatus GameStatus
        {
            get { return _gameStatus; }
            private set
            {
                _gameStatus = value;
                _selectedCategories = new ObservableCollection<StringWithIDTransporter>(_gameStatus.SelectableCategories != null ? _gameStatus.SelectableCategories : new StringWithIDTransporter[0]{});
                _currentSelectedCategory = _selectedCategories.FirstOrDefault(x => x.ID == GameStatus.CurrentCategory.ID);
                RaisePropertyChanged("SelectedCategories");
                RaisePropertyChanged("GameStatus");
                RaisePropertyChanged("RoundCountString");
                RaisePropertyChanged("CurrentSelectedCategory");
                RaisePropertyChanged("FilteredItemEntries");
            }
        }

        private ObservableCollection<StringWithIDTransporter> _selectedCategories;

        public ObservableCollection<StringWithIDTransporter> SelectedCategories
        {
            get { return _selectedCategories; }
        }

        private ItemEntry _selectedCategory;
        public ItemEntry SelectedCategory {
            get { return _selectedCategory; }
            set {
                _selectedCategory = value;
                RaisePropertyChanged("SelectedCategory");
            }
        }

        private StringWithIDTransporter _currentSelectedCategory;
        public StringWithIDTransporter CurrentSelectedCategory
        {
            get { return _currentSelectedCategory; }
            set
            {
                _currentSelectedCategory = value;
                RaisePropertyChanged("CurrentSelectedCategory");
            }
        }

        #endregion

        public string RoundCountString
        {
            get { return Settings.RoundCount.ToString(); }
            set
            {
                Int32 parseint = 0;
                Int32.TryParse(value, out parseint);
                if (parseint > 0)
                {
                    if (Settings.RoundCount != parseint)
                    {
                        _settings.RoundCount = parseint;
                        _server.SetSettings(_settings);
                    }
                }
            }
        }


        private CountdownTimer _countdownTimer = new CountdownTimer();

        public CountdownTimer CountdownTimer
        {
            get { return _countdownTimer; }
        }

        private NomencontestServer _server = null;

        public AdminPanelVM()
        {
            _settings = new Settings();
            var popup = new ServerConnectPopup(Settings.ServerIPAddress, _settings.GamePort);
            popup.ShowDialog();
            //popup.IPAddress = "http://172.168.0.226:9002";
            if (popup.DialogResult == true)
            {
                var address = popup.IPAddress;

                _server = new NomencontestServer(address);

                _server.GameSetup += OnGameSetup;
                _server.ItemsUpdated += OnItemsUpdate;
                _server.StatusUpdated += OnStatusUpdate;
                _server.MusicStatusChanged += ServerOnMusicStatusChanged;
                _server.SettingsUpdated += OnSettingsUpdate;
                _server.ClockUpdated += OnClockUpdated;
                _server.Open();
                if (!_server.Register(PermissionLevel.PERMISSIONS_ADMIN, ADMIN_KEY))
                {
                    MessageBox.Show("Unable to find server!");
                    Application.Current.Shutdown();
                    return;
                }

                _newTeamName = string.Empty;


                //var GameViewWindow = new MainWindow();
                GameStatus = new GameStatus();
                RaisePropertyChanged("CurrentRound");
                RaisePropertyChanged("LettersToReveal");
                //GameViewWindow.DataContext = _gameStatus;
                //_gameVM.CurrViewWindow = GameViewWindow;
                //GameViewWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                //GameViewWindow.Show();

            }

        }

        #region SelectCategory

        private ICommand _selectCategoryCommand;

        public ICommand SelectCategoryCommand
        {
            get { return _selectCategoryCommand ?? (_selectCategoryCommand = new RelayCommand(SelectCategoryCommandExecuted)); }
        }


        private void SelectCategoryCommandExecuted(object param)
        {
            if (GameStatus.RoundPhase == RoundPhase.NotStarted && _selectedCategories.Count < 2 && _selectedCategories.All(x => x.ID != _selectedCategory.ID))
            {
                _selectedCategories.Add(new StringWithIDTransporter() {ID = _selectedCategory.ID, Value = _selectedCategory.Category});
                SelectedCategory = null;
            }
        }

        #endregion


        #region RoundCategoryCommand

        private ICommand _roundCategoryCommand;

        public ICommand RoundCategoryCommand
        {
            get { return _roundCategoryCommand ?? (_roundCategoryCommand = new RelayCommand(RoundCategoryCommandExecuted)); }
        }


        private void RoundCategoryCommandExecuted(object param)
        {
            if (!(param is uint)) return;
            uint p = (uint) param;

            if (GameStatus.RoundPhase == RoundPhase.NotStarted)
            {
               var category = _selectedCategories.First(x=> x.ID == p);
                _selectedCategories.Remove(category);
            }
            else if (GameStatus.RoundPhase == RoundPhase.ShowCategories)
            {
                CurrentSelectedCategory = _selectedCategories.First(x => x.ID == p);
            }
        }
        #endregion


        #region ResetRound

        private ICommand _showCategoriesCommand;

        public ICommand ShowCategoriesCommand
        {
            get { return _showCategoriesCommand ?? (_resetRoundCommand = new RelayCommand(ShowCategoriesCommandExecuted)); }
        }


        private void ShowCategoriesCommandExecuted(object param)
        {
            if (GameStatus.RoundPhase == RoundPhase.NotStarted && _selectedCategories.Count>0)
            {
                var c = new uint[_selectedCategories.Count];
                for (int i = 0; i < c.Length; i++)
                {
                    c[i] = _selectedCategories[i].ID;
                }
                
                _server.ShowRoundCategories(c);
            }
        }

        #endregion

        #region ResetRound

        private ICommand _resetRoundCommand;

        public ICommand ResetRoundCommand
        {
            get { return _resetRoundCommand ?? (_resetRoundCommand = new RelayCommand(ResetRoundCommandExecuted)); }
        }


        private void ResetRoundCommandExecuted(object param)
        {
            if (GameStatus.RoundPhase < RoundPhase.IsOver)
            {
                if (MessageBox.Show("Reset the game round?", "Reset Round", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    _server.ResetRound();
                }
            }
        }

        #endregion

        #region EndGame

        private ICommand _endGameCommand;

        public ICommand EndGameCommand
        {
            get { return _endGameCommand ?? (_endGameCommand = new RelayCommand(EndGameCommandExecuted)); }
        }


        private void EndGameCommandExecuted(object param)
        {
            if (GameStatus.CurrentRound > 0)
            {
                if (MessageBox.Show("End the current game?", "End Game", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {

                    _server.EndGame();
                }
            }
        }

        #endregion

        #region Flip Random Character

        private ICommand _flipRandomCharacter;

        public ICommand FlipRandomCharacter
        {
            get { return _flipRandomCharacter ?? (_flipRandomCharacter = new RelayCommand(FlipRandomCharacterExecuted)); }
        }


        private void FlipRandomCharacterExecuted(object param)
        {
            if (GameStatus.RoundPhase == RoundPhase.IsRunning)
            {
                _server.FlipRandomCharacter();
            }
        }

        #endregion


        private bool _manualPlayerSelectMode = false;

        public bool ManualPlayerSelectMode
        {
            get { return _manualPlayerSelectMode; }
            set
            {
                if (!value && value != _manualPlayerSelectMode)
                {

                    _server.SetCurrentPlayer(GameStatus.Players[GameStatus.CurrentPlayerIndex].Model.ID);
                }
                _manualPlayerSelectMode = value;
                RaisePropertyChanged("ManualPlayerSelectMode");
            }
        }
        
        private string _newTeamName;

        public string NewTeamName
        {
            get { return _newTeamName; }
            set
            {
                _newTeamName = value;
                RaisePropertyChanged("NewTeamName");
            }
        }

        private ICommand _startGameCommand;

        public ICommand StartGameCommand
        {
            get { return _startGameCommand ?? (_startGameCommand = new RelayCommand(o => StartGameCommandExecuted())); }
        }


        private void StartGameCommandExecuted()
        {
            _server.StartGame();
        }



        private ICommand _getRoundReadyCommand;

        public ICommand GetRoundReadyCommand
        {
            get
            {
                return _getRoundReadyCommand ??
                       (_startRoundCommand = new RelayCommand(GetRoundReadyCommandExecuted));
            }
        }

        private void GetRoundReadyCommandExecuted(object param)
        {
            {
                if (_currentSelectedCategory!= null &&  GameStatus.RoundPhase == RoundPhase.ShowCategories)
                { _server.SelectRoundCategory(_currentSelectedCategory.ID);}
            }
        }
        
        private ICommand _startRoundCommand;

        public ICommand StartRoundCommand
        {
            get
            {
                return _startRoundCommand ??
                       (_startRoundCommand = new RelayCommand(StartRoundCommandExecuted));
            }
        }

        private void StartRoundCommandExecuted(object param)
        {
            {
                _countdownTimer.Time = Settings.RoundLength;
                _server.StartRound();
            }
        }

        private ICommand _accidentalWordCommand;

        public ICommand AccidentalWordCommand
        {
            get
            {
                return _accidentalWordCommand ??
                       (_accidentalWordCommand = new RelayCommand(AccidentalWordCommandExecuted));
            }
        }

        private void AccidentalWordCommandExecuted(object param)
        {
            {
                if (!_gameStatus.IsFinalRound)
               
                {
                    _server.GuessItem(false, true);
                }
            }
        }

        private ICommand _nextNameCommand;

        public ICommand NextNameCommand
        {
            get
            {
                return _nextNameCommand ??
                       (_nextNameCommand = new RelayCommand(NextNameCommandExecuted));
            }
        }

        private bool guessItemValueFinalRound;
        private void NextNameCommandExecuted(object param)
        {
            {
                if (_gameStatus.IsFinalRound)
                {
                    if (_gameStatus.CurrentName == _gameStatus.CurrentDisplayName)
                    {
                        _server.GuessItem(guessItemValueFinalRound, false);
                    }
                    else
                    {
                        var b = bool.Parse(param.ToString());
                        guessItemValueFinalRound = b;
                        _server.FinalRoundToggleNameVisibility(true);
                    }
                }
                else
                {
                    var b = bool.Parse(param.ToString());
                    _server.GuessItem(b, false);
                }
            }
        }

        private ICommand _changeScoresCommand;

        public ICommand ChangeScoresCommand
        {
            get
            {
                return _changeScoresCommand ??
                       (_changeScoresCommand = new RelayCommand( ChangeScoresCommandExecuted));
            }
        }
        
        private void ChangeScoresCommandExecuted(object param)
        {
            var dialog = new UpdateScore((int)GameStatus.Players[0].Points, (int)GameStatus.Players[1].Points);
            var result = dialog.ShowDialog();
            if (dialog.Result && (GameStatus.Players[0].Points != dialog.Player1Score || GameStatus.Players[1].Points != dialog.Player2Score))
            {
                _server.OverridePlayerScores(dialog.Player1Score, dialog.Player2Score);
            }
            
        }

        private ICommand _endRoundCommand;

        public ICommand EndRoundCommand
        {
            get
            {
                return _endRoundCommand ??
                       (_endRoundCommand = new RelayCommand(EndRoundCommandExecuted));
            }
        }

        private void EndRoundCommandExecuted(object param)
        {
            _server.EndRound();
        }
        private ICommand _nextRoundCommand;

        public ICommand NextRoundCommand
        {
            get
            {
                return _nextRoundCommand ??
                       (_nextRoundCommand = new RelayCommand(NextRoundCommandExecuted));
            }
        }
        
        private void NextRoundCommandExecuted(object param)
        {
            _server.NextRound();
        }

        private ICommand _selectFinalRoundCategoryCommand;

        public ICommand SelectFinalRoundCategoryCommand
        {
            get
            {
                return _selectFinalRoundCategoryCommand ??
                       (_selectFinalRoundCategoryCommand = new RelayCommand(SelectFinalRoundCategoryCommandExecuted));
            }
        }

        private void SelectFinalRoundCategoryCommandExecuted(object param)
        {
            if (_gameStatus.IsFinalRound &&  param is uint)
            {
                _server.SelectRoundCategory((uint)param);
            }
        }

        private ICommand _finalRoundPlayerGuessCommand;

        public ICommand FinalRoundPlayerGuessCommand
        {
            get
            {
                return _finalRoundPlayerGuessCommand ??
                       (_finalRoundPlayerGuessCommand = new RelayCommand(FinalRoundPlayerGuessCommandExecuted));
            }
        }

        private void FinalRoundPlayerGuessCommandExecuted(object param)
        {
            uint p = uint.Parse(param.ToString());
            if (_gameStatus.IsFinalRound)
            {
                _server.FinalRoundPlayerGuesses(_gameStatus.Players[p].Model.ID);
            }
        }

        
        private ICommand _loadQuestionsCommand;

        public ICommand LoadQuestionsCommand
        {
            get
            {
                return _loadQuestionsCommand ??
                       (_loadQuestionsCommand = new RelayCommand(o => LoadQuestionsCommandExecuted()));
            }
        }

        private void LoadQuestionsCommandExecuted()
        {
            LoadQuestions();

            RaisePropertyChanged("Items");
        }


        public bool LoadQuestions()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Comma-separated values (*.csv)|*.csv";

            dlg.ShowDialog();
            string filename = dlg.FileName;
            if (File.Exists(filename))
            {
                byte[] file = null;
                try
                {
                    file = File.ReadAllBytes(filename);
                }
                catch (IOException e)
                {
                    MessageBox.Show(String.Format("Unable to open file: {0}", e.Message));
                    return false;
                }
                if (file.Length > 0)
                {

                    _server.AddItemDatabase(file);
                    return true;
                }
            }
            return false;
        }
        
        #region CloseQuestionsCommand

        private ICommand _closeQuestionsCommand;

        public ICommand CloseQuestionsCommand
        {
            get
            {
                return _closeQuestionsCommand ??
                       (_closeQuestionsCommand = new RelayCommand(o => CloseQuestionsCommandExecuted()));
            }
        }

        private void CloseQuestionsCommandExecuted()
        {

            _server.ClearItemDatabase();
        }

        #endregion

        private bool _playMusic;

        public bool PlayMusic
        {
            get { return _playMusic; }
            set
            {
                if (_playMusic != value)
                {
                    _playMusic = value;
                    _server.PlayMusic(value);
                }
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

        public void OnStatusUpdate(object obj, StatusArgs args)
        {
            GameStatus = args.GameStatus;
        }

        public void OnSettingsUpdate(object obj, SettingsArgs args)
        {
            Settings = args.SetupTransporter.Settings;
        }

        public void OnGameSetup(object obj, GameSetupArgs args)
        {
            GameStatus = args.GameStatus;
            Settings = args.SetupTransporter.Settings;
        }

        public void OnItemsUpdate(object obj, ItemArgs args)
        {
            CachedItemDB = args.Items.ItemDatabase;
        }

        private void ServerOnMusicStatusChanged(object sender, BoolArgs boolArgs)
        {
            _playMusic = boolArgs.Value;
            RaisePropertyChanged("PlayMusic");
        }

        private void OnClockUpdated(object sender, ClockArgs e)
        {
            _countdownTimer.Time = e.ClockTransporter.CurrentTime;
        }

    }
}
