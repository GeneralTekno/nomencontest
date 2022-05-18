using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Nomencontest.Base;
using Nomencontest.Base.Models;
using Nomencontest.Core.Wrapper.Server;

namespace Nomencontest.Core.Wrapper
{
    public class NomencontestServer : ICoreCallback
    {
        #region Setup

        private static DataTransporter _clientData;

        private object _clientLock = new object();

        private ICore _serverCore = null;
        private InstanceContext _context = null;
        private DuplexChannelFactory<ICore> _factory;
        private string _address;
        private PermissionLevel _permissionLevel = PermissionLevel.PERMISSIONS_OBSERVER;
        
        public NomencontestServer(string address)
        {
            _clientData = new DataTransporter();
            _context = null;
            _factory = null;
            _address = address + Settings.ServerURLPath;
        }

        public bool IsConnected
        {
            get { return _factory != null && _factory.State == CommunicationState.Opened; }
        }

        private System.Timers.Timer _pingTimer;
        public bool Open()
        {
            try
            {
                Connect(_address);
                if (IsConnected)
                {
                    _pingTimer.Elapsed += KeepAlive;
                    _pingTimer.Interval = 2 * 60 * 1000;
                    _pingTimer.AutoReset = true;
                    _pingTimer.Start();
                }

            }
            catch (Exception e)
            {

            }
            return IsConnected;
        }

        public void Close()
        {
            try
            {
                _factory.Close();
            }
            catch (Exception e)
            {

            }
        }

        private void KeepAlive(object sender, ElapsedEventArgs e)
        {
            if (_serverCore != null && _clientData != null)
            {
                UpdateClientInfo();
                try
                {
                    _serverCore.Ping(_clientData);
                }
                catch (Exception ex)
                {
                    Close();
                    Connect(_address);
                }

            }
        }


        private bool Connect(string address)
        {
            if (_context == null)
            {
                _context = new InstanceContext(this);

            }
            if (_factory == null || _factory.State != CommunicationState.Opened)
            {
                try
                {
                    if (_factory != null) _factory.Faulted -= FactoryOnFaulted;
                    _factory?.Close();
                }
                catch (Exception e)
                {
                }

                var binding = new NetHttpBinding();
                binding.Security.Mode = BasicHttpSecurityMode.None;
                binding.MaxBufferSize = 2147483647;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                var endpoint = new EndpointAddress(address);
                _factory = new DuplexChannelFactory<ICore>(this, binding, endpoint);
                _factory.Faulted += FactoryOnFaulted;
                ICore kernel = (ICore)_factory.CreateChannel();
                _serverCore = kernel;
                _address = address;
            }
            return IsConnected;
        }

        private void FactoryOnFaulted(object sender, EventArgs eventArgs)
        {
            _factory.Faulted -= FactoryOnFaulted;
            _factory = null;
        }

        #endregion

        #region registration
        public bool Register(PermissionLevel permissionLevel, string adminGuid)
        {
            lock (_clientLock)
            {
                try
                {
                    uint id = 0;
                    var clientData = new DataTransporter();
                    switch (permissionLevel)
                    {
                        case PermissionLevel.PERMISSIONS_ADMIN:
                            id = _serverCore.RegisterAdministrator(adminGuid);
                            _permissionLevel = PermissionLevel.PERMISSIONS_ADMIN;
                            break;
                        case PermissionLevel.PERMISSIONS_GUI:
                            id = _serverCore.RegisterMainGUI(clientData, adminGuid);
                            _permissionLevel = PermissionLevel.PERMISSIONS_GUI;
                            break;
                        case PermissionLevel.PERMISSIONS_OBSERVER:
                            id = _serverCore.RegisterObserver(clientData);
                            _permissionLevel = PermissionLevel.PERMISSIONS_OBSERVER;
                            break;
                        case PermissionLevel.PERMISSIONS_PLAYER:
                            id = _serverCore.RegisterPlayer(clientData);
                            _permissionLevel = PermissionLevel.PERMISSIONS_PLAYER;
                            break;
                    }
                    if (id > 0)
                    {
                        _clientData.ID = id;
                        return true;
                    }
                }
                catch (CommunicationException e)
                {
                }
                return false;
            }
        }

        public bool Unregister()
        {
            lock (_clientLock)
            {
                try
                {
                    var clientData = new DataTransporter() {ID = _clientData.ID};
                    switch (_permissionLevel)
                    {
                        case PermissionLevel.PERMISSIONS_ADMIN:
                            _serverCore.UnregisterAdministrator(clientData);
                            break;
                        case PermissionLevel.PERMISSIONS_GUI:
                            _serverCore.UnregisterMainGUI(clientData);
                            break;
                        case PermissionLevel.PERMISSIONS_OBSERVER:
                            _serverCore.UnregisterObserver(clientData);
                            break;
                        case PermissionLevel.PERMISSIONS_PLAYER:
                            _serverCore.UnregisterPlayer(clientData);
                            break;
                    }
                    return true;
                }
                catch (CommunicationException e)
                {
                }
                return false;
            }
        }

        #endregion

        public void GetGameData()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.GetGameData(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void SetSettings(Settings settings)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    var setupTransporter = new SetupTransporter() {ID = _clientData.ID, Settings=settings, Timestamp=_clientData.Timestamp};
                    _serverCore.SetSettings(setupTransporter);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void AddItemDatabase(byte[] itemFile)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.AddItemDatabase(_clientData, itemFile);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void ClearItemDatabase()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.ClearItemDatabase(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }
        
        public void RemoveItem(uint itemID)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.RemoveItem(_clientData, itemID);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void GetItems()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.GetItems(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void ShuffleCategoryItems(uint itemID)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.ShuffleCategoryItems(_clientData, itemID);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void EnableShuffle(bool value)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.EnableShuffle(_clientData, value);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void SetCurrentPlayer(uint playerID)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.SetCurrentPlayer(_clientData, playerID);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void ResetRound()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.ResetRound(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void ShowRoundCategories(uint[] categoryIDs)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.ShowRoundCategories(_clientData, categoryIDs);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void SelectRoundCategory(uint categoryID)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.SelectRoundCategory(_clientData, categoryID);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void StartRound()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.StartRound(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void GuessItem(bool isCorrect, bool playSoundOnWrong)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.GuessItem(_clientData, isCorrect, playSoundOnWrong);
                }
                catch (CommunicationException e)
                {
                }
            }
        }
        public void OverridePlayerScores(int player1Score, int player2Score)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.OverridePlayerScores(_clientData, player1Score, player2Score);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void FlipRandomCharacter()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.FlipRandomCharacter();
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void FinalRoundToggleNameVisibility(bool showItem)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.FinalRoundToggleNameVisibility(_clientData, showItem);
                }
                catch (CommunicationException e)
                {
                }
            }
        }
        

        public void EndRound()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.EndRound(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void NextRound()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.NextRound(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void FinalRoundPlayerGuesses()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.NextRound(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void FinalRoundPlayerGuesses(uint playerID)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.FinalRoundPlayerGuesses(_clientData, playerID);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void FinalRoundDirectPlayerGuess()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.FinalRoundPlayerGuessesDirect(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void StartGame()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.StartGame(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void EndGame()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.EndGame(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void PlayMusic(bool playMusic)
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.PlayMusic(_clientData, playMusic);
                }
                catch (CommunicationException e)
                {
                }
            }
        }

        public void GetSettings()
        {
            lock (_clientLock)
            {
                try
                {
                    UpdateClientInfo();
                    _serverCore.GetSettings(_clientData);
                }
                catch (CommunicationException e)
                {
                }
            }
        }


        #region callback functions

        public void OnStatusUpdate(GameStatus gameStatus)
        {
            StatusUpdated?.Invoke(this, new StatusArgs(gameStatus));
        }
        
        public void OnItemsUpdate(ItemDatabaseTransporter questions)
        {
            ItemsUpdated?.Invoke(this, new ItemArgs(questions));
        }

        public void OnGameSetup(SetupTransporter setupTransporter, GameStatus gameData)
        {
            GameSetup?.Invoke(this, new GameSetupArgs(setupTransporter, gameData));
        }

        public void OnClockUpdate(ClockTransporter transporter)
        {
            ClockUpdated?.Invoke(this, new ClockArgs(transporter));
        }

        public void OnPlayMusic(bool playMusic)
        {
            MusicStatusChanged?.Invoke(this, new BoolArgs(playMusic));
        }

        public void OnSettingsUpdate(SetupTransporter transporter)
        {
            SettingsUpdated?.Invoke(this, new SettingsArgs(transporter));
        }
        public void OnAudioCue(SoundCue cue)
        {
            AudioCue?.Invoke(this, new AudioArgs(cue));
        }

        public event EventHandler<ClockArgs> ClockUpdated;
        public event EventHandler<SettingsArgs> SettingsUpdated;
        public event EventHandler<StatusArgs> StatusUpdated;
        public event EventHandler<ItemArgs> ItemsUpdated;
        public event EventHandler<GameSetupArgs> GameSetup;
        public event EventHandler<BoolArgs> MusicStatusChanged;
        public event EventHandler<AudioArgs> AudioCue;
        #endregion

        private void UpdateClientInfo()
        {
            if (_clientData != null) _clientData.Timestamp = DateTime.UtcNow.Ticks;
            Connect(_address);
        }

    }
}
