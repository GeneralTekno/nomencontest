using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nomencontest.Base;
using Nomencontest.Base.Models;
using Nomencontest.Clients.UI;
using Nomencontest.Core.Wrapper;

namespace Nomencontest.UIClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string GUI_KEY = "651432fc-24ff-480e-b671-b9be6bd8a383";

        public Settings Settings
        {
            get; private set;
        }

        private int _countdownTimer;
        public int CountdownTimer
        {
            get { return _countdownTimer; }
            set
            {
                _countdownTimer = value;
                RaisePropertyChanged("CountdownTimer");
            }
        }

        private SoundPlayer _soundPlayer;

        private GameStatus _gameStatus;
        public GameStatus GameStatus
        {
            get { return _gameStatus; }
            set
            {
                _gameStatus = value;
                RaisePropertyChanged("GameStatus");
                RaisePropertyChanged("CurrentNameText");

                RaisePropertyChanged("ShowTimer");
                RaisePropertyChanged("ShowCategories");
                RaisePropertyChanged("ShowSingleCategory");
                RaisePropertyChanged("ShowPlayers");
                RaisePropertyChanged("ShowSplashScreen");
                RaisePropertyChanged("ShowCurrentItem");
                RaisePropertyChanged("Player1Active");
                RaisePropertyChanged("Player2Active");

                SoundPlayer.PlayGuessBGM =_gameStatus.RoundPhase == RoundPhase.IsRunning && !_gameStatus.IsFinalRound;
                



            }
        }

        private NomencontestServer _server;
        public MainWindow()
        {
            DataContext = this;
            _soundPlayer = new SoundPlayer();
            Connect();
            InitializeComponent();
            Closed += controlWindow_Closed;
            StateChanged += ControlWindowOnStateChanged;
        }


        public void Connect()
        {
            var settings = new Settings();
            var popup = new ServerConnectPopup(settings.ServerIPAddress, settings.GamePort);
            popup.ShowDialog();
            //popup.IPAddress = "http://172.168.0.226:9002";
            if (popup.DialogResult == true)
            {
                var address = popup.IPAddress;


                GameStatus = new GameStatus();
                _server = new NomencontestServer(address);

                _server.GameSetup += OnGameSetup;
                _server.StatusUpdated += OnStatusUpdate;
                _server.MusicStatusChanged += ServerOnMusicStatusChanged;
                _server.ClockUpdated += OnClockUpdated;
                _server.AudioCue += OnAudioCue;
                _server.Open();
                
                if (!_server.Register(PermissionLevel.PERMISSIONS_GUI, GUI_KEY))
                {
                    MessageBox.Show("Unable to find server!");
                    Application.Current.Shutdown();
                    return;
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
            if (GameStatus.RoundPhase == RoundPhase.IsReady) CountdownTimer = Settings.RoundLength;
        }
        
        private void ServerOnMusicStatusChanged(object sender, BoolArgs boolArgs)
        {
            SoundPlayer.PlayMusic = boolArgs.Value;
        }

        private void OnClockUpdated(object sender, ClockArgs e)
        {
            CountdownTimer = e.ClockTransporter.CurrentTime;
            if (CountdownTimer == 1) SoundPlayer.PlayAsyncSound(SoundPlayer.END_ROUND);
        }
        void controlWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void ControlWindowOnStateChanged(object sender, EventArgs eventArgs)
        {
            var window = sender as MainWindow;
            window.WindowStyle = (window.WindowState == WindowState.Maximized ? WindowStyle.None : WindowStyle.SingleBorderWindow);
        }
        private void OnAudioCue(object sender, AudioArgs eventArgs)
        {
            switch (eventArgs.SoundCue)
            {
                case SoundCue.PickCategory:
                    SoundPlayer.PlayAsyncSound(SoundPlayer.PICK_CATEGORY);
                    break;
                case SoundCue.RightGuess:
                    SoundPlayer.PlayAsyncSound(SoundPlayer.GUESS_GOOD);
                    break;
                case SoundCue.RoundBGM:
                    SoundPlayer.PlayGuessBGM = !SoundPlayer.PlayGuessBGM;
                    break;
                case SoundCue.StartTheme:
                    SoundPlayer.PlayMusic = true;
                    break;
                case SoundCue.StopTheme:
                    SoundPlayer.PlayMusic = false;
                    break;
                case SoundCue.WrongGuess:
                    SoundPlayer.PlayAsyncSound(SoundPlayer.GUESS_BAD);
                    break;
                case SoundCue.Intro:
                    SoundPlayer.PlayAsyncSound(SoundPlayer.THEMESONGINTRO);
                    break;
                case SoundCue.Outro:
                    SoundPlayer.PlayAsyncSound(SoundPlayer.THEMSONGOUTRO);
                    break;
                case SoundCue.EndRound:
                   
                    break;
            }
        }

        public string CurrentNameText
        {
            get
            {
                return GameStatus.CurrentDisplayName;
            }
        }

        public bool Player1Active
        {
            get
            {
                //return true;
                return
                    (GameStatus.RoundPhase == RoundPhase.IsReady && GameStatus.CurrentPlayerIndex == 0) ||
                    (GameStatus.RoundPhase == RoundPhase.NotStarted) ||
                    (GameStatus.IsFinalRound && GameStatus.RoundPhase != RoundPhase.IsRunning) ||
                    (GameStatus.IsFinalRound && GameStatus.IsFinalRoundPlayerGuessing && GameStatus.CurrentPlayerIndex == 0);
            }
        }

        public bool Player2Active
        {
            get
            {
                //return true;
                return
                    (GameStatus.IsFinalRound && GameStatus.RoundPhase != RoundPhase.IsRunning) ||
                    (GameStatus.RoundPhase == RoundPhase.NotStarted) ||
                    (GameStatus.RoundPhase == RoundPhase.IsReady && GameStatus.CurrentPlayerIndex == 1) ||
                    (GameStatus.IsFinalRound && GameStatus.IsFinalRoundPlayerGuessing && GameStatus.CurrentPlayerIndex == 1);
            }
        }


        public bool ShowTimer
        {
            get
            {
                return (
                        GameStatus.RoundPhase == RoundPhase.IsRunning ||
                        GameStatus.RoundPhase == RoundPhase.IsOver)
                       && !GameStatus.IsFinalRound;
            }
        }
        public bool ShowPlayers
        {
            get
            {
                return (GameStatus.RoundPhase == RoundPhase.IsReady || 
                    GameStatus.RoundPhase == RoundPhase.NotStarted) ||
                       GameStatus.RoundPhase == RoundPhase.GameDone ||
                       GameStatus.IsFinalRound;
                    
            }
        }

        public bool ShowCurrentItem
        {
            get { return (GameStatus.RoundPhase == RoundPhase.IsRunning);  }
        }

        public bool ShowCategories
        {
            get { return GameStatus.RoundPhase == RoundPhase.ShowCategories && !GameStatus.IsFinalRound; }
        }

        public bool ShowSingleCategory
        {
            get { return GameStatus.RoundPhase == RoundPhase.IsReady; }
        }

        public bool ShowSplashScreen
        {
            get
            {
                return GameStatus.RoundPhase == RoundPhase.GameNotStarted ||
                       GameStatus.RoundPhase == RoundPhase.GameDone ||
                       GameStatus.RoundPhase == RoundPhase.NotStarted ||
                       GameStatus.RoundPhase == RoundPhase.IsOver;
            }
        }

    }
}
